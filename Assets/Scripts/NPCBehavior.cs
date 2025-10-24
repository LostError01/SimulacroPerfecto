using Unity.VisualScripting;
using UnityEngine;

public class NPCBehavior : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float changeDirectionInterval = 2f;
    public float goalSeekCooldown = 0.3f; // Tiempo mínimo entre reorientaciones hacia la meta4
    public float timeToGoalMode = 30f; // Tiempo antes de cambiar a modo búsqueda de meta

    [Header("VariablesNPC")]
    private int reputacionNPC = 0;

    private Vector2 currentDirection;
    private float timer;
    private float goalTimer;
    private Rigidbody2D rb;
    private bool hasReachedGoal = false;
    private GameObject goalTarget;
    private float lastGoalSeekTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("NPCWanderer requiere un componente Rigidbody2D.");
            enabled = false;
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;

        // 👇 Primera dirección: SIEMPRE hacia abajo (-Y)
        currentDirection = Vector2.down;

        timer = changeDirectionInterval;
        goalTimer = 0f;
        hasReachedGoal = false;

        reputacionNPC = Random.Range(0, 100); // Valor entre 0 y 100
    }

    void Update()
    {
        //Si la reputacion es igual o menor a 40, el npc es de color rojo, si es mayor, verde
        if(reputacionNPC<=20)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.green;
        }

        goalTimer += Time.deltaTime;

        // Buscar la meta solo una vez (o si aún no se ha encontrado)
        if (!hasReachedGoal && goalTarget == null)
        {
            goalTarget = GameObject.FindGameObjectWithTag("Meta");
            if (goalTarget == null)
            {
                Debug.LogWarning("No se encontró ningún objeto con el tag 'Meta'. El NPC seguirá merodeando.");
            }
        }

        // Modo merodeo (primeros 20 segundos o sin meta)
        if (goalTimer < timeToGoalMode || goalTarget == null)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                SetRandomDirection();
                timer = changeDirectionInterval;
            }
        }
        else
        {
            // Modo búsqueda de meta
            if (!hasReachedGoal)
            {
                // Reorientar suavemente hacia la meta (con un pequeño cooldown para no hacerlo cada frame)
                if (Time.time - lastGoalSeekTime >= goalSeekCooldown)
                {
                    SeekGoal();
                    lastGoalSeekTime = Time.time;
                }
            }
        }

        if (Input.GetMouseButtonDown(0)) // Botón izquierdo
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                if(reputacionNPC <= 20)
                {
                    Debug.Log("NPC Eliminado correcto");
                }
                else
                {
                    Debug.Log("NPC Eliminado incorrecto");
                }
                Destroy(gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        if (!hasReachedGoal)
        {
            rb.linearVelocity = currentDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Detenerse al llegar
        }
    }

    void SetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        currentDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
    }

    void SeekGoal()
    {
        if (goalTarget == null) return;

        Vector2 directionToGoal = (goalTarget.transform.position - transform.position).normalized;
        currentDirection = directionToGoal;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("ColisionPared"))
        {
            // Si ya está en modo meta, desviarse pero manteniendo el objetivo
            if (goalTimer >= 20f && goalTarget != null && !hasReachedGoal)
            {
                RedirectAwayFromWallSmart(col);
            }
            else
            {
                // Modo merodeo: desviación amplia
                RedirectAwayFromWall(col);
            }
        }
    }

    // Desviación inteligente: alejarse de la pared, pero sesgada hacia la meta
    void RedirectAwayFromWallSmart(Collision2D col)
    {
        // 1. Dirección de escape (como antes)
        Vector2 collisionNormal = Vector2.zero;
        foreach (ContactPoint2D contact in col.contacts)
        {
            collisionNormal += contact.normal;
        }
        collisionNormal = collisionNormal.normalized;
        Vector2 awayFromWall = -collisionNormal;

        // 2. Dirección hacia la meta
        if (goalTarget == null) return;
        Vector2 toGoal = (goalTarget.transform.position - transform.position).normalized;

        // 3. Combinar ambas direcciones: priorizar escape, pero sesgar hacia la meta
        Vector2 combined = (awayFromWall + toGoal).normalized;

        // Si la combinación da cero (caso raro), usar solo awayFromWall
        if (combined == Vector2.zero)
        {
            combined = awayFromWall;
        }

        // Añadir un poco de ruido aleatorio para evitar bucles
        float randomAngle = Random.Range(-30f, 30f);
        float angle = Mathf.Atan2(combined.y, combined.x) * Mathf.Rad2Deg + randomAngle;
        currentDirection = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;
    }

    // Versión simple para modo merodeo
    void RedirectAwayFromWall(Collision2D col)
    {
        Vector2 collisionNormal = Vector2.zero;
        foreach (ContactPoint2D contact in col.contacts)
        {
            collisionNormal += contact.normal;
        }
        collisionNormal = collisionNormal.normalized;

        Vector2 awayDirection = -collisionNormal;
        float deviationAngle = Random.Range(-90f, 90f);

        float currentAngle = Mathf.Atan2(awayDirection.y, awayDirection.x) * Mathf.Rad2Deg;
        float newAngle = currentAngle + deviationAngle;

        currentDirection = new Vector2(
            Mathf.Cos(newAngle * Mathf.Deg2Rad),
            Mathf.Sin(newAngle * Mathf.Deg2Rad)
        ).normalized;
    }

    // Opcional: detectar si llegó a la meta
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Meta"))
        {
            hasReachedGoal = true;
            rb.linearVelocity = Vector2.zero;
            Debug.Log("¡NPC ha llegado a la meta!");
        }

        if(other.CompareTag("CityLimits"))
        {
            Destroy(gameObject);
        }
    }

}
