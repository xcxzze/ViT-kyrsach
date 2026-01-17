using System.Collections;
using UnityEngine;

namespace _ViT.CodeBase
{
    public class GameEventsHandler : MonoBehaviour
    {
        [Header("Pirate Ships Event")]
        [SerializeField] private GameObject pirateShipPrefab;
        [SerializeField] private int pirateShipsCount = 3;
    
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
                Vector3 pos = Camera.main.transform.position + new Vector3(i * 5f, 2, 10f);
                GameObject ship = Instantiate(pirateShipPrefab, pos, Quaternion.identity);
                ship.name = $"PirateShip_{i}";
            
                UnityEngine.Debug.Log($"<color=lime>✓ Ship spawned: {ship.name} at {pos}</color>");
            
                yield return new WaitForSeconds(0.3f);
            }
        
            UnityEngine.Debug.Log($"<color=lime>✓✓✓ ВСЕ {pirateShipsCount} КОРАБЛЕЙ СОЗДАНЫ! ✓✓✓</color>");
        }
    }
}