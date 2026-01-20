using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _ViT.CodeBase
{
    public class GameEventsHandler : MonoBehaviour
    {
        [Header("Pirate Ships Event")]
        [SerializeField] private GameObject pirateShipPrefab;
        [SerializeField] private int pirateShipsCount = 3;
        [SerializeField] private List<Transform> pirateShipPositions;
        
        [Header("Werewolf Event")]
        [SerializeField] private GameObject werewolfPrefab;
        [SerializeField] private int werewolvesCount = 3;
        [SerializeField] private List<Transform> werewolfPositions;
    
        public void OnSeaDetected()
        {
            UnityEngine.Debug.Log($"<color=cyan>═══ OnSeaDetected() ВЫЗВАН! ═══</color>");
            UnityEngine.Debug.Log($"Префаб: {(pirateShipPrefab != null ? pirateShipPrefab.name : "НЕ НАЗНАЧЕН!")}");
            UnityEngine.Debug.Log($"Количество: {pirateShipsCount}");
        
            if (pirateShipPrefab == null)
            {
                UnityEngine.Debug.LogError("ПРЕФАБ НЕ НАЗНАЧЕН! Создаю тестовые кубы...");
                SpawnTestCubes();
                return;
            }
        
            StartCoroutine(SpawnShips());
        }
        
        public void OnNightForestDetected()
        {
            UnityEngine.Debug.Log($"<color=magenta>═══ OnNightForestDetected() ВЫЗВАН! ═══</color>");
            UnityEngine.Debug.Log($"Префаб: {(werewolfPrefab != null ? werewolfPrefab.name : "НЕ НАЗНАЧЕН!")}");
            UnityEngine.Debug.Log($"Количество: {werewolvesCount}");
    
            if (werewolfPrefab == null)
            {
                UnityEngine.Debug.LogError("ПРЕФАБ НЕ НАЗНАЧЕН! Создаю тестовые капсулы...");
                SpawnTestCubes();
                return;
            }
    
            StartCoroutine(SpawnWerewolves());
        }
    
        void SpawnTestCubes()
        {
            for (int i = 0; i < pirateShipsCount; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Vector3 pos = Camera.main.transform.position + new Vector3(i * 3f, 2, 10f);
                cube.transform.position = pos;
                cube.transform.localScale = new Vector3(2, 1, 3);
                cube.name = $"TestShip_{i}";
                cube.GetComponent<Renderer>().material.color = new Color(0.6f, 0.3f, 0f);
            
                UnityEngine.Debug.Log($"<color=yellow>✓ Test cube created at {pos}</color>");
            }
        }
    
        IEnumerator SpawnShips()
        {
            for (int i = 0; i < pirateShipsCount; i++)
            {
                Vector3 pos = pirateShipPositions[i].position;
                GameObject ship = Instantiate(pirateShipPrefab, pos, Quaternion.identity);
                ship.name = $"PirateShip_{i}";
            
                UnityEngine.Debug.Log($"<color=lime>✓ Ship spawned: {ship.name} at {pos}</color>");
            
                yield return new WaitForSeconds(0.3f);
            }
        
            UnityEngine.Debug.Log($"<color=lime>✓✓✓ ВСЕ {pirateShipsCount} КОРАБЛЕЙ СОЗДАНЫ! ✓✓✓</color>");
        }
        
        IEnumerator SpawnWerewolves()
        {
            for (int i = 0; i < werewolvesCount; i++)
            {
                Vector3 pos = werewolfPositions[i].position;
                GameObject werewolf = Instantiate(werewolfPrefab, pos, Quaternion.identity);
                werewolf.name = $"Werewolf_{i}";
        
                UnityEngine.Debug.Log($"<color=lime>✓ Werewolf spawned: {werewolf.name} at {pos}</color>");
        
                yield return new WaitForSeconds(0.5f);
            }
    
            UnityEngine.Debug.Log($"<color=lime>✓✓✓ ВСЕ {werewolvesCount} ОБОРОТНЕЙ СОЗДАНЫ! ✓✓✓</color>");
        }
    }
}