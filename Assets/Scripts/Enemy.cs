using UnityEngine;
using UnityEngine.AI; // Necesario para usar NavMeshAgent
using System.Collections; // Necesario para usar IEnumerator

public class Enemy : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    public Transform player; // Referencia al jugador
    public float followDistance = 10f; // Distancia máxima para seguir al jugador
    public float health = 50f; // Vida del enemigo
    public float headDamageMultiplier = 2f; // Multiplicador de daño para la cabeza
    public float torsoDamageMultiplier = 1f; // Daño normal para el torso
    public float legDamageMultiplier = 0.5f; // Daño reducido para las piernas

    [Header("Sonidos")]
    public AudioClip deathSound; // Sonido al morir
    private AudioSource audioSource; // Componente para reproducir sonidos

    private NavMeshAgent agent; // Referencia al NavMeshAgent
    private EnemySpawner spawner;
    private Animator animator; // Referencia al Animator
    private Rigidbody rb;

    void Start()
    {

        if (ZombieCounter.Instance != null)
        {
            ZombieCounter.Instance.IncrementZombieCount();
        }

        // Encuentra el spawner en la escena
        spawner = FindObjectOfType<EnemySpawner>();

        // Obtiene el componente NavMeshAgent y Rigidbody
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        // Obtiene el componente Animator
        animator = GetComponent<Animator>();

        // Obtiene o añade un AudioSource si no existe
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configura el AudioSource
        audioSource.playOnAwake = false;

        // Busca automáticamente al jugador si no está asignado
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void Update()
    {
        if (player != null && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            // Calcula la distancia al jugador
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Si está dentro de la distancia de seguimiento, sigue al jugador
            if (distanceToPlayer <= followDistance)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.SetDestination(transform.position); // Detiene el movimiento
            }

            // Actualiza el parámetro "isMoving" en el Animator
            bool isMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("isMoving", isMoving);
        }
    }

    // Método para aplicar daño dependiendo de la parte impactada
    public void TakeDamage(float damage, string hitboxTag)
    {
        float finalDamage = damage;

        // Aplicar multiplicador de daño según la hitbox
        switch (hitboxTag)
        {
            case "Head":
                finalDamage *= headDamageMultiplier;
                break;
            case "Torso":
                finalDamage *= torsoDamageMultiplier;
                break;
            case "Legs":
                finalDamage *= legDamageMultiplier;
                break;
            default:
                finalDamage = damage; // Si no hay un multiplicador, daño normal
                break;
        }

        health -= finalDamage; // Reduce la vida del enemigo
        Debug.Log($"Impacto en: {hitboxTag}. Daño recibido: {finalDamage}, Vida restante: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (spawner != null)
        {
            spawner.EnemyDefeated();
        }

        if (ZombieCounter.Instance != null)
        {
            ZombieCounter.Instance.IncrementZombieCount(); // Incrementar el contador
        }

        // Detener el NavMeshAgent
        if (agent != null)
        {
            agent.enabled = false;
        }

        // Activar la física del Rigidbody
        if (rb != null)
        {
            rb.isKinematic = false; // Habilita las fuerzas físicas
            rb.useGravity = true;
        }

        // Reproducir la animación de muerte
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Reproducir sonido de muerte
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        Debug.Log("El enemigo ha muerto.");
        StartCoroutine(DestroyAfterDelay());
    }
    private IEnumerator DestroyAfterDelay()
    {
        // Obtén la duración de la animación de muerte
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationDuration = stateInfo.length;

        // Espera a que termine la animación de muerte
        yield return new WaitForSeconds(animationDuration);

        // Deja el zombie tirado en el suelo durante unos segundos (por ejemplo, 5 segundos)
        yield return new WaitForSeconds(5f);

        // Desactiva el objeto visualmente y lo destruye después
        gameObject.SetActive(false); // Desaparece visualmente
        yield return new WaitForSeconds(10f); // Tiempo opcional antes de eliminarlo completamente

        Destroy(gameObject);
    }

    // Método para detectar impactos en las hitboxes
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            BulletController bulletController = collision.gameObject.GetComponent<BulletController>();
            if (bulletController != null)
            {
                // Determinamos la parte del cuerpo impactada usando el tag
                string hitboxTag = collision.transform.tag; // Usamos el tag del objeto impactado ("Head", "Torso", etc.)

                // Aplicamos daño según la hitbox
                TakeDamage(bulletController.damage, hitboxTag);
            }
        }
    }
}
