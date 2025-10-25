using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SpawnPrograms : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] mainPrefabs; // Debe tener exactamente 4 elementos
    public GameObject specialPrefab; // Puede ser null

    [Header("Configuración")]
    [Range(0f, 1f)]
    public float specialSpawnChance = 0.1f;
    public float spawnInterval = 2f;

    private int currentPrefabIndex = 0;
    private GameObject currentInstance = null;
    private Coroutine spawnCoroutine;
    private bool wasQTEActive = false;

    public static bool isSpecialSpawned = false;
    private bool lastWasSpecial = false;

    void Start()
    {
        if (mainPrefabs == null || mainPrefabs.Length != 4)
        {
            Debug.LogError("El campo 'Main Prefabs' debe contener exactamente 4 prefabs.");
            return;
        }

        spawnCoroutine = StartCoroutine(SpawnRoutine());
        isSpecialSpawned = false;
        lastWasSpecial = false;
    }

    void Update()
    {
        // Detectar transición de QTE
        if (!wasQTEActive && QuickTimeEvent.isQTEActive)
        {
            // QTE acaba de empezar → pausar
            PauseSpawner();
        }
        else if (wasQTEActive && !QuickTimeEvent.isQTEActive)
        {
            // QTE acaba de terminar → reanudar
            ResumeSpawner();
        }

        wasQTEActive = QuickTimeEvent.isQTEActive;
    }

    void PauseSpawner()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        // NOTA: NO destruimos currentInstance → se mantiene en pantalla durante QTE
    }

    void ResumeSpawner()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // Esperar el intervalo completo
            yield return new WaitForSeconds(spawnInterval);

            // Si durante la espera el QTE se activó, no hacer nada y esperar a que termine
            if (QuickTimeEvent.isQTEActive)
            {
                // La corrutina ya fue detenida en Update, pero por seguridad:
                yield break;
            }

            // Solo spawneamos si NO hay QTE activo
            SpawnNext();
        }
    }

    public void SpawnNext()
    {
        // Destruir el anterior (solo si no estamos en QTE, lo cual ya está garantizado)
        if (currentInstance != null)
        {
            Destroy(currentInstance);
        }

        GameObject prefabToSpawn;
        bool isSpecial = false;

        // Evitar dos especiales seguidos
        if (!lastWasSpecial && specialPrefab != null && Random.value < specialSpawnChance)
        {
            prefabToSpawn = specialPrefab;
            isSpecial = true;
            isSpecialSpawned = true;
        }
        else
        {
            prefabToSpawn = mainPrefabs[currentPrefabIndex];
            isSpecial = false;
            isSpecialSpawned = false;
        }

        lastWasSpecial = isSpecial;
        currentPrefabIndex = (currentPrefabIndex + 1) % 4;

        if (prefabToSpawn != null)
        {
            currentInstance = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
            if (isSpecial)
            {
                currentInstance.AddComponent<SpecialPrefabDestroyLogger>();
            }
        }
        else
        {
            Debug.LogWarning("Prefab a spawnear es null.");
        }
    }

    void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        if (currentInstance != null)
        {
            Destroy(currentInstance);
        }
    }
}

public class SpecialPrefabDestroyLogger : MonoBehaviour
{
    void OnDestroy()
    {
        if(QuickTimeEvent.winnedQTE)
        {
            QuickTimeEvent.winnedQTE = false;
        }
        else
            NPCGenerator.SliderValueNPC += 10;
    }
}