using UnityEngine;
using System.Collections;

public class NPCGenerator : MonoBehaviour
{
    [Header("Configuración del Spawner")]
    [Tooltip("Prefab del NPC/enemigo a instanciar")]
    public GameObject npcPrefab;

    [Tooltip("Tiempo en segundos entre cada generación de NPC")]
    public float spawnInterval = 3f;

    [Tooltip("¿Generar en una posición aleatoria dentro de un radio alrededor del spawner?")]
    public bool randomSpawn = false;

    [Tooltip("Radio alrededor del spawner para aparición aleatoria (solo si randomSpawn está activado)")]
    public float spawnRadius = 5f;

    [Tooltip("¿Destruir el spawner después de un número fijo de spawns?")]
    public bool limitedSpawns = false;

    [Tooltip("Número máximo de NPCs a generar (solo si limitedSpawns está activo)")]
    public int maxSpawns = 10;

    private int currentSpawnCount = 0;
    private bool isSpawning = true;

    void Start()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("¡No se ha asignado un prefab de NPC en el spawner!", this);
            enabled = false;
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (limitedSpawns && currentSpawnCount >= maxSpawns)
            {
                isSpawning = false;
                break;
            }

            Vector2 spawnPos = randomSpawn
                ? (Vector2)transform.position + (Vector2)Random.insideUnitCircle * spawnRadius
                : (Vector2)transform.position;

            Instantiate(npcPrefab, spawnPos, Quaternion.identity);
            currentSpawnCount++;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Métodos públicos opcionales para control externo
    public void StopSpawning() => isSpawning = false;

    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    // Visualización en el editor
    private void OnDrawGizmosSelected()
    {
        if (randomSpawn)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.3f);
        }
    }
}
