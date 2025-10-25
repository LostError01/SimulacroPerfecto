using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class QuickTimeEvent : MonoBehaviour
{
    [Header("Configuración del QTE")]
    public float qteDuration = 5f;
    public float endDelay = 1f;
    private bool completed = false;

    [Header("Referencias a Prefabs de Flechas")]
    public GameObject arrowUpPrefab;
    public GameObject arrowDownPrefab;
    public GameObject arrowLeftPrefab;
    public GameObject arrowRightPrefab;

    [Header("Spawners (de izquierda a derecha)")]
    public Transform[] spawners; // Debe tener exactamente 4 elementos

    private KeyCode[] correctSequence;
    private int currentInputIndex = 0;
    public static bool isQTEActive = false;
    private bool isLocked = false;
    private List<GameObject> spawnedArrows = new List<GameObject>();

    private SpawnPrograms scriptProgramas;

    public static bool winnedQTE = false;

    void Start()
    {
        //Ejecutar el metodo SpawnNext del script SpawnPrograms para spawnear el siguiente programa
        scriptProgramas = FindFirstObjectByType<SpawnPrograms>();
    }
    void Update()
    {
        if (!isLocked && Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            // Verificar si se hizo clic en este objeto y si el especial está activo
            if (hit.collider != null && hit.collider.gameObject == gameObject && SpawnPrograms.isSpecialSpawned)
            {
                StartCoroutine(StartQuickTimeEvent());
            }

            // Verificar si se hizo clic en este objeto y si el especial NO está activo
            if (hit.collider != null && hit.collider.gameObject == gameObject && !SpawnPrograms.isSpecialSpawned)
            {
                Debug.Log("¡Este objeto no activa un QTE ahora mismo!");
                NPCGenerator.SliderValueNPC += 5; // Penalización por clic incorrecto
            }

            //Verificar si no se hizo cli
        }

        if (isQTEActive && currentInputIndex < 4)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                CheckInput(KeyCode.UpArrow);
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                CheckInput(KeyCode.DownArrow);
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                CheckInput(KeyCode.LeftArrow);
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                CheckInput(KeyCode.RightArrow);
        }
    }

    IEnumerator StartQuickTimeEvent()
    {
        isLocked = true;
        isQTEActive = true;
        currentInputIndex = 0;
        ClearSpawnedArrows();

        GameObject[] arrowPrefabs = { arrowUpPrefab, arrowDownPrefab, arrowLeftPrefab, arrowRightPrefab };
        KeyCode[] keyCodes = { KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow };

        correctSequence = new KeyCode[4];
        spawnedArrows = new List<GameObject>(); // ¡Lista vacía, pero vamos a llenarla con Add!

        // Generar y spawnear secuencia
        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, arrowPrefabs.Length);
            correctSequence[i] = keyCodes[randomIndex];
            GameObject arrow = Instantiate(arrowPrefabs[randomIndex], spawners[i].position, Quaternion.identity);
            spawnedArrows.Add(arrow); // ✅ Así se hace correctamente
        }

        float timer = 0f;
        completed = false;

        while (timer < qteDuration && currentInputIndex < 4)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        if (currentInputIndex >= 4)
        {
            NPCGenerator.SliderValueNPC -= 8; // Recompensa por éxito
            winnedQTE = true;
        }
        else
        {
            Debug.Log("QTE fallido.");
            NPCGenerator.SliderValueNPC += 10; // Penalización por fallo
            // Cambiar flechas no completadas a rojo
            for (int i = currentInputIndex; i < spawnedArrows.Count; i++)
            {
                if (spawnedArrows[i] != null)
                    SetArrowColor(spawnedArrows[i], Color.red);
            }
        }

        yield return new WaitForSeconds(endDelay);
        scriptProgramas.SpawnNext();
        EndQTE();
    }

    void CheckInput(KeyCode pressedKey)
    {
        if (currentInputIndex >= spawnedArrows.Count)
        {
            // Seguridad extra: no debería pasar, pero por si acaso
            return;
        }

        if (pressedKey == correctSequence[currentInputIndex])
        {
            if (spawnedArrows[currentInputIndex] != null)
                SetArrowColor(spawnedArrows[currentInputIndex], Color.white);

            currentInputIndex++;

            if (currentInputIndex >= 4)
            {
                Debug.Log("¡QTE completado correctamente!");
                isQTEActive = false;
            }
        }
        else
        {
            Debug.Log("QTE fallido.");
            isQTEActive = false;

            // Cambiar todas las flechas desde currentInputIndex en adelante a rojo
            for (int i = currentInputIndex; i < spawnedArrows.Count; i++)
            {
                if (spawnedArrows[i] != null)
                    SetArrowColor(spawnedArrows[i], Color.red);
            }

            StartCoroutine(DelayAfterFailure());
        }
    }

    IEnumerator DelayAfterFailure()
    {
        yield return new WaitForSeconds(endDelay);
        EndQTE();
    }

    void SetArrowColor(GameObject arrow, Color color)
    {
        SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = color;
    }

    void EndQTE()
    {
        isQTEActive = false;
        isLocked = false;
        ClearSpawnedArrows();
    }

    void ClearSpawnedArrows()
    {
        foreach (GameObject arrow in spawnedArrows)
        {
            if (arrow != null)
                Destroy(arrow);
        }
        spawnedArrows.Clear();
    }
}
