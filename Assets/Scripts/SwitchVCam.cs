using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchVCam : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private int priorityBoostAmount = 10;

    [SerializeField] private Canvas thirdPersonCanvas;
    [SerializeField] private Canvas aimCanvas;

    [Header("Cámaras")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Camera firstPersonCamera;

    [Header("Configuración de la cámara en primera persona")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Retícula")]
    [SerializeField] private Canvas crosshairCanvas;

    private InputAction aimAction;
    private InputAction switchToFirstPersonAction;
    public bool isInFirstPersonMode = false;

    private float xRotation = 0f;

    public bool GetFirstPersonMode()
    {
        return isInFirstPersonMode;
    }

    private void Awake()
    {
        // Asignar el PlayerInput si es nulo
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("No se encontró un componente PlayerInput en este GameObject. Asegúrate de asignarlo en el Inspector.");
                return;
            }
        }

        if (playerInput != null)
        {
            aimAction = playerInput.actions["Aim"];
            switchToFirstPersonAction = playerInput.actions["SwitchCamera"];
        }

        // Inicializar las cámaras
        isInFirstPersonMode = false;
        virtualCamera?.gameObject.SetActive(true);
        firstPersonCamera?.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (playerInput == null)
        {
            Debug.LogError("playerInput no está asignado. No se pueden suscribir las acciones.");
            return;
        }

        // Suscribirse a las acciones de entrada
        aimAction.performed += context => StartAim(context);
        aimAction.canceled += context => CancelAim(context);
        switchToFirstPersonAction.performed += context => ToggleFirstPersonMode(context);
    }

    private void OnDisable()
    {
        // Evitar errores si playerInput no está asignado
        if (playerInput == null || aimAction == null || switchToFirstPersonAction == null)
        {
            Debug.LogWarning("playerInput o las acciones no están asignados al desactivar SwitchVCam.");
            return;
        }

        // Desuscripción de las acciones de entrada
        aimAction.performed -= StartAim;
        aimAction.canceled -= CancelAim;
        switchToFirstPersonAction.performed -= ToggleFirstPersonMode;
    }
    private void Update()
    {
        if (isInFirstPersonMode)
        {
            FirstPersonLook();
        }
    }

    private void StartAim(InputAction.CallbackContext context)
    {
        if (virtualCamera == null)
        {
            Debug.LogError("virtualCamera no está asignada. No se puede iniciar la acción de apuntar.");
            return;
        }

        // Aumentar la prioridad de la cámara al apuntar
        virtualCamera.Priority += priorityBoostAmount;

        // Asegúrate de que aimCanvas esté asignado antes de habilitarlo
        if (aimCanvas != null)
        {
            aimCanvas.enabled = true;
        }
    }

    private void CancelAim(InputAction.CallbackContext context)
    {
        if (virtualCamera == null)
        {
            Debug.LogError("virtualCamera no está asignada. No se puede cancelar la acción de apuntar.");
            return;
        }

        // Restaurar la prioridad de la cámara
        virtualCamera.Priority -= priorityBoostAmount;

        // Asegúrate de que aimCanvas esté asignado antes de deshabilitarlo
        if (aimCanvas != null)
        {
            aimCanvas.enabled = false;
        }
    }

    private void ToggleFirstPersonMode(InputAction.CallbackContext context)
    {
        if (playerInput == null)
        {
            Debug.LogError("playerInput es nulo. No se puede cambiar de cámara.");
            return;
        }

        if (virtualCamera == null || firstPersonCamera == null)
        {
            Debug.LogError("Faltan referencias de cámara. No se puede cambiar de cámara.");
            return;
        }

        // Cambiar entre primera y tercera persona
        isInFirstPersonMode = !isInFirstPersonMode;

        if (isInFirstPersonMode)
        {
            // Activar la cámara de primera persona
            virtualCamera.gameObject.SetActive(false); // Desactivar tercera persona
            firstPersonCamera.gameObject.SetActive(true); // Activar primera persona
            ToggleAudioListener(true); // Asegurarse de que solo haya un AudioListener

            // Asegúrate de que thirdPersonCanvas y crosshairCanvas estén asignados antes de deshabilitarlos
            if (thirdPersonCanvas != null)
            {
                thirdPersonCanvas.enabled = false;
            }
            if (crosshairCanvas != null)
            {
                crosshairCanvas.enabled = false;
            }
        }
        else
        {
            // Activar la cámara de tercera persona
            virtualCamera.gameObject.SetActive(true); // Activar tercera persona
            firstPersonCamera.gameObject.SetActive(false); // Desactivar primera persona
            ToggleAudioListener(false); // Asegurarse de que solo haya un AudioListener

            // Asegúrate de que thirdPersonCanvas y crosshairCanvas estén asignados antes de habilitarlos
            if (thirdPersonCanvas != null)
            {
                thirdPersonCanvas.enabled = true;
            }
            if (crosshairCanvas != null)
            {
                crosshairCanvas.enabled = true;
            }
        }

        Debug.Log($"Modo cambiado a: {(isInFirstPersonMode ? "Primera Persona" : "Tercera Persona")}");

        // Actualizar el estado en PlayerController (si existe)
        var playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.SetFirstPersonMode(isInFirstPersonMode);
        }
    }

    private void FirstPersonLook()
    {
        if (playerInput == null)
        {
            Debug.LogError("playerInput no está asignado. No se puede procesar la rotación.");
            return;
        }

        Vector2 lookInput = playerInput.actions["Look"].ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        playerBody.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        firstPersonCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    // Método para asegurarse de que solo haya un AudioListener activo
    private void ToggleAudioListener(bool isFirstPerson)
    {
        if (firstPersonCamera != null)
        {
            AudioListener firstPersonListener = firstPersonCamera.GetComponent<AudioListener>();
            if (firstPersonListener != null) firstPersonListener.enabled = isFirstPerson;
        }

        if (virtualCamera != null && virtualCamera.gameObject != firstPersonCamera.gameObject)
        {
            AudioListener thirdPersonListener = virtualCamera.GetComponent<AudioListener>();
            if (thirdPersonListener != null) thirdPersonListener.enabled = !isFirstPerson;
        }
    }
}