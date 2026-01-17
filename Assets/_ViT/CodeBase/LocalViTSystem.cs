using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class LocalViTSystem : MonoBehaviour
{
    [SerializeField] private List<EventRule> eventRules = new();
    private RenderTexture renderTexture;
    private Camera gameCamera;

    private string serverUrl = "http://localhost:8000";

    private float detectionInterval = 3f;
    private float confidenceThreshold = 0.5f;
    
    private bool isAnalyzing;

    [System.Serializable]
    public class EventRule
    {
        public string eventName;
        public List<string> requiredLabels = new();
        [Range(0f, 1f)]
        public float minConfidence = 0.15f;
        public bool spawnOnce = true;
        public UnityEvent onEventTriggered;
        public bool hasTriggered;
    }
    
    [System.Serializable]
    private class Prediction
    {
        public string label;
        public float score;
    }
    
    [System.Serializable]
    private class PredictionWrapper
    {
        public Prediction[] predictions;
    }

    private void Start()
    {
        if (gameCamera == null) gameCamera = Camera.main;
        renderTexture = new RenderTexture(224, 224, 24);
        
        foreach (var rule in eventRules)
        {
            rule.hasTriggered = false;
        }
        
        StartCoroutine(CheckServer());
    }
    
    IEnumerator CheckServer()
    {
        Debug.Log("Проверка сервера...");
        UnityWebRequest request = UnityWebRequest.Get($"{serverUrl}/health");
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("<color=green>✓ Сервер работает!</color>");
            StartCoroutine(PeriodicDetection());
        }
        else
            Debug.LogError($"<color=red>✗ Сервер недоступен: {request.error}</color>");
    }
    
    IEnumerator PeriodicDetection()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            if (!isAnalyzing)
            {
                yield return StartCoroutine(CaptureAndAnalyze());
            }
            yield return new WaitForSeconds(detectionInterval);
        }
    }
    
    IEnumerator CaptureAndAnalyze()
    {
        isAnalyzing = true;
        
        Debug.Log("→ Захват скриншота...");
        Texture2D screenshot = CaptureScreenshot();
        byte[] imageBytes = screenshot.EncodeToJPG(85);
        Debug.Log($"→ Отправка на сервер ({imageBytes.Length / 1024}KB)...");
        
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("file", imageBytes, "screenshot.jpg", "image/jpeg"));
        
        UnityWebRequest request = UnityWebRequest.Post($"{serverUrl}/predict", formData);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("→ Ответ получен, обрабатываю...");
            ProcessPredictions(request.downloadHandler.text);
        }
        else
            Debug.LogError($"Ошибка: {request.error}");

        Destroy(screenshot);
        request.Dispose();
        isAnalyzing = false;
    }
    
    Texture2D CaptureScreenshot()
    {
        RenderTexture currentRT = RenderTexture.active;
        gameCamera.targetTexture = renderTexture;
        gameCamera.Render();
        
        RenderTexture.active = renderTexture;
        Texture2D screenshot = new Texture2D(224, 224, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, 224, 224), 0, 0);
        screenshot.Apply();
        
        gameCamera.targetTexture = null;
        RenderTexture.active = currentRT;
        
        return screenshot;
    }
    
    void ProcessPredictions(string jsonResponse)
    {
        try
        {
            string wrappedJson = "{\"predictions\":" + jsonResponse + "}";
            PredictionWrapper wrapper = JsonUtility.FromJson<PredictionWrapper>(wrappedJson);
            
            if (wrapper.predictions == null)
            {
                Debug.LogWarning("Предсказания не получены!");
                return;
            }
            
            Debug.Log($"→ Получено {wrapper.predictions.Length} предсказаний");
            
            var top5 = wrapper.predictions.OrderByDescending(p => p.score).Take(5).ToList();
            foreach (var p in top5)
            {
                Debug.Log($"  {p.label}: {p.score:P1}");
            }
            
            var confidentPredictions = wrapper.predictions
                .Where(p => p.score >= confidenceThreshold)
                .OrderByDescending(p => p.score)
                .ToList();
            
            Debug.Log($"→ Проверяю теги (порог {confidenceThreshold:P1})...");
            Debug.Log($"  Прошло фильтр: {confidentPredictions.Count} предсказаний");
            CheckEventRules(confidentPredictions);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Parse error: {e.Message}");
        }
    }
    
    void CheckEventRules(List<Prediction> predictions)
    {
        Debug.Log($"CheckEventRules: получено {predictions.Count} предсказаний для проверки");
        
        foreach (var rule in eventRules)
        {
            Debug.Log($"  Правило: {rule.eventName}");
            
            if (rule.spawnOnce && rule.hasTriggered)
            {
                Debug.Log($"    <color=yellow>УЖЕ СРАБОТАЛО РАНЕЕ - пропускаем</color>");
                continue;
            }
            
            Debug.Log($"    Проверяю {predictions.Count} предсказаний...");
            
            foreach (var prediction in predictions)
            {
                Debug.Log($"      Предсказание: '{prediction.label}' = {prediction.score:P1}");
                Debug.Log($"      Порог правила: {rule.minConfidence:P1}");
                
                if (prediction.score < rule.minConfidence)
                {
                    Debug.Log($"      <color=red>НЕ ПРОШЛО ПОРОГ</color>");
                    continue;
                }
                
                Debug.Log($"      <color=green>ПРОШЛО ПОРОГ! Ищу совпадения...</color>");
                
                string predLabel = prediction.label.ToLower();
                string[] labelParts = predLabel.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                Debug.Log($"      Части метки: [{string.Join(", ", labelParts)}]");
                Debug.Log($"      Ищу теги: [{string.Join(", ", rule.requiredLabels)}]");
                
                foreach (var requiredLabel in rule.requiredLabels)
                {
                    string reqLower = requiredLabel.ToLower();
                    bool matched = labelParts.Any(part => part.Contains(reqLower) || reqLower.Contains(part));
                    
                    if (!matched) matched = predLabel.Contains(reqLower);
                    
                    Debug.Log($"        Тег '{reqLower}': {(matched ? "<color=lime>НАЙДЕН!</color>" : "не найден")}");
                    
                    if (matched)
                    {
                        Debug.Log($"════════════════════════════════════════");
                        Debug.Log($"🎯 <color=lime>ТЕГ НАЙДЕН!</color>");
                        Debug.Log($"Метка ИИ: '{prediction.label}'");
                        Debug.Log($"Уверенность: {prediction.score:P1} ({prediction.score:F3})");
                        Debug.Log($"Совпал с тегом: '{requiredLabel}'");
                        Debug.Log($"Событие: {rule.eventName}");
                        Debug.Log($"Подписчиков: {(rule.onEventTriggered != null ? rule.onEventTriggered.GetPersistentEventCount() : 0)}");
                        Debug.Log($"Вызываю событие...");
                        Debug.Log($"════════════════════════════════════════");
                        
                        rule.onEventTriggered?.Invoke();
                        rule.hasTriggered = true;
                        return;
                    }
                }
            }
        }
        
        Debug.Log("<color=yellow>НИ ОДИН ТЕГ НЕ СОВПАЛ!</color>");
    }
    
    void OnDestroy()
    {
        if (renderTexture != null) renderTexture.Release();
    }
}