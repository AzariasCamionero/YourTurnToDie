using UnityEngine;
using TMPro; // Importar el namespace de TextMeshPro

public class ZombieCounter : MonoBehaviour
{
    public static ZombieCounter Instance; // Instancia global para acceso fácil
    public TextMeshProUGUI zombieCounterText; // Referencia al texto de TextMeshPro
    private int zombieCount = 0; // Contador de zombies eliminados

    private void Awake()
    {
        // Configurar la instancia única
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método para incrementar el contador
    public void IncrementZombieCount()
    {
        zombieCount++; // Aumentar el contador
        UpdateUI(); // Actualizar la UI
    }

    // Método para actualizar el texto en la UI
    private void UpdateUI()
    {
        if (zombieCounterText != null)
        {
            zombieCounterText.text = "" + zombieCount;
        }
    }
}