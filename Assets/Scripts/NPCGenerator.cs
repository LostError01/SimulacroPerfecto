using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NPCGenerator : MonoBehaviour
{
    [Header("Configuraci�n del Spawner")]
    [Tooltip("Prefab del NPC/enemigo a instanciar")]
    public GameObject npcPrefab;

    [Tooltip("Tiempo en segundos entre cada generaci�n de NPC")]
    public float spawnInterval = 3f;

    [Tooltip("�Generar en una posici�n aleatoria dentro de un radio alrededor del spawner?")]
    public bool randomSpawn = false;

    [Tooltip("Radio alrededor del spawner para aparici�n aleatoria (solo si randomSpawn est� activado)")]
    public float spawnRadius = 5f;

    [Tooltip("�Destruir el spawner despu�s de un n�mero fijo de spawns?")]
    public bool limitedSpawns = false;

    [Tooltip("N�mero m�ximo de NPCs a generar (solo si limitedSpawns est� activo)")]
    public int maxSpawns = 10;

    [Header("Variables del Slider")]
    private Slider reputacionSlider;

    private int currentSpawnCount = 0;
    private bool isSpawning = true;

    public static int SliderValueNPC = 0;

    void Start()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("�No se ha asignado un prefab de NPC en el spawner!", this);
            enabled = false;
            return;
        }

        //Encontrar el Slider con el nombre
        reputacionSlider = GameObject.Find("BarraDeMalestar").GetComponent<Slider>();

        // Configurar el slider de reputaci�n
        if (reputacionSlider != null)
        {
            reputacionSlider.minValue = 0;
            reputacionSlider.maxValue = 100;
            reputacionSlider.value = 0;
        }

        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        //Evitar que el valor del Slider sea menor a 0
        if (reputacionSlider.value < 0)
        {
            reputacionSlider.value = 0;
        }
        else
        {
            reputacionSlider.value = SliderValueNPC;
        }
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

    // M�todos p�blicos opcionales para control externo
    public void StopSpawning() => isSpawning = false;

    public void ResumeSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    // Visualizaci�n en el editor
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
