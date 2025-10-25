using UnityEngine;
using TMPro; // Importar el namespace de TextMeshPro

public class ZombieCounter : MonoBehaviour
{
    public static ZombieCounter Instance; // Instancia global para acceso f�cil
    public TextMeshProUGUI zombieCounterText; // Referencia al texto de TextMeshPro
    private int zombieCount = 0; // Contador de zombies eliminados

    private void Awake()
    {
        // Configurar la instancia �nica
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // M�todo para incrementar el contador
    public void IncrementZombieCount()
    {
        zombieCount++; // Aumentar el contador
        UpdateUI(); // Actualizar la UI
    }

    // M�todo para actualizar el texto en la UI
    private void UpdateUI()
    {
        if (zombieCounterText != null)
        {
            zombieCounterText.text = "" + zombieCount;
        }
    }
}