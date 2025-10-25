using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Configuraci�n de la bala")]
    [SerializeField] private GameObject bulletDecal; // Prefab del decal de impacto
    [SerializeField] private GameObject bloodParticlesPrefab; // Prefab de part�culas de sangre
    [SerializeField] private float speed = 50f; // Velocidad de la bala
    [SerializeField] private float timeToDestroy = 3f; // Tiempo de vida de la bala antes de destruirse
    [SerializeField] public float damage = 10f; // Da�o que inflige la bala
    [SerializeField] private Color enemyHitColor = Color.red; // Color del decal al impactar un enemigo
    [SerializeField] private AudioClip shootSound; // Sonido al disparar
    [SerializeField] private AudioClip impactSound; // Sonido al impactar
    [SerializeField] private AudioSource audioSource; // Componente AudioSource para reproducir sonidos

    public Vector3 target { get; set; } // Punto hacia el que se dirige la bala
    public bool hit { get; set; } // Indica si la bala ha impactado algo
    private bool hasHit = false; // Evita da�o repetido

    private void OnEnable()
    {
        Destroy(gameObject, timeToDestroy); // Destruye la bala autom�ticamente despu�s de cierto tiempo

        // Reproducir sonido de disparo
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }

    void Update()
    {
        // Mueve la bala hacia el objetivo
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Si no impacta y est� cerca del objetivo, destruye la bala
        if (!hit && Vector3.Distance(transform.position, target) < 0.1f)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return; // Evita da�o repetido
        hasHit = true;

        // Reproducir sonido de impacto
        if (audioSource != null && impactSound != null)
        {
            audioSource.PlayOneShot(impactSound);
        }

        // Si impacta contra un enemigo
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Determinamos la parte del cuerpo impactada por el tag
                string hitboxTag = collision.transform.tag; // Usar el tag del objeto impactado, como "Head", "Torso", etc.

                // Aplicamos da�o seg�n la hitbox
                enemy.TakeDamage(damage, hitboxTag);

                // Crear decal y ajustarlo al punto de impacto
                ContactPoint contact = collision.GetContact(0);
                Vector3 decalPosition = contact.point + contact.normal * 0.01f; // Ajustar 0.01f seg�n sea necesario
                Quaternion decalRotation = Quaternion.LookRotation(contact.normal);
                GameObject decal = Instantiate(bulletDecal, decalPosition, decalRotation);
                decal.transform.SetParent(collision.transform); // Hacer que el decal sea hijo del enemigo

                // Cambiar el color del decal a rojo (solo para enemigos)
                ChangeDecalColor(decal, enemyHitColor);

                // Crear part�culas de sangre en el punto de impacto
                if (bloodParticlesPrefab != null)
                {
                    Instantiate(bloodParticlesPrefab, contact.point, Quaternion.identity);
                }
            }
        }
        else
        {
            // Si impacta cualquier otra superficie
            ContactPoint contact = collision.GetContact(0);
            Vector3 decalPosition = contact.point + contact.normal * 0.01f;
            Quaternion decalRotation = Quaternion.LookRotation(contact.normal);
            GameObject decal = Instantiate(bulletDecal, decalPosition, decalRotation);
        }

        // Destruye la bala despu�s del impacto
        Destroy(gameObject);
    }

    // M�todo para cambiar el color del decal
    private void ChangeDecalColor(GameObject decal, Color color)
    {
        // Aseg�rate de que el prefab del decal tenga un Renderer
        Renderer renderer = decal.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = color;
        }
    }
}
