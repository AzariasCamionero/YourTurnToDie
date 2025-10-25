using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitchManager : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private int priorityBoostAmount = 10;

    [SerializeField] private Canvas thirdPersonCanvas;
    [SerializeField] private Canvas aimCanvas;

    [Header("Cámaras")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;  // Cámara de tercera persona
    [SerializeField] private Camera firstPersonCamera;  // Cámara de primera persona
    [SerializeField] private CinemachineVirtualCamera aimCameraVCam;  // Cámara de apuntado (Cinemachine)

    [Header("Configuración de la cámara en primera persona")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Retícula")]
    [SerializeField] private Canvas crosshairCanvas;

    private InputAction aimAction;
    private InputAction switchToFirstPersonAction;
    public bool isInFirstPersonMode = false;

    private float xRotation = 0f;

    // Método público para acceder a 'isInFirstPersonMode'
    public bool GetFirstPersonMode()
    {
        return isInFirstPersonMode;
    }

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogError("No se encontró un componente PlayerInput en este GameObject. Asegúrate de asignarlo en el Inspector.");
                return;
            }
        }

        aimAction = playerInput.actions["Aim"];
        switchToFirstPersonAction = playerInput.actions["SwitchCamera"];

        // Inicializar las cámaras
        isInFirstPersonMode = false;
        virtualCamera?.gameObject.SetActive(true);
        firstPersonCamera?.gameObject.SetActive(false);
        aimCameraVCam?.gameObject.SetActive(true);  // Mantener la cámara de apuntado activa

        EnsureSingleAudioListener(); // Asegurarnos de que solo un AudioListener esté activo
    }

    private void OnEnable()
    {
        if (playerInput == null)
        {
            Debug.LogError("playerInput no está asignado.");
            return;
        }

        aimAction.performed += StartAim;
        aimAction.canceled += CancelAim;
        switchToFirstPersonAction.performed += ToggleFirstPersonMode;
    }

    private void OnDisable()
    {
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
        if (aimCameraVCam == null)
        {
            Debug.LogError("aimCameraVCam no está asignado. No se puede iniciar la acción de apuntar.");
            return;
        }

        // Aumentar la prioridad solo cuando se comienza a apuntar
        aimCameraVCam.Priority += priorityBoostAmount;

        // Asegurarse de que la cámara de apuntado esté activa
        if (!aimCameraVCam.gameObject.activeSelf)
        {
            aimCameraVCam.gameObject.SetActive(true); // Asegurarse de que la cámara de apuntado no se desactive
        }

        if (aimCanvas != null)
        {
            aimCanvas.enabled = true;
        }
    }

    private void CancelAim(InputAction.CallbackContext context)
    {
        if (aimCameraVCam == null)
        {
            Debug.LogError("aimCameraVCam no está asignado. No se puede cancelar la acción de apuntar.");
            return;
        }

        // Reducir la prioridad de la cámara de apuntado cuando se cancela el apuntado
        aimCameraVCam.Priority -= priorityBoostAmount;

        if (aimCanvas != null)
        {
            aimCanvas.enabled = false;
        }
    }

    private void ToggleFirstPersonMode(InputAction.CallbackContext context)
    {
        // Verificar el estado actual de la cámara
        if (isInFirstPersonMode)
        {
            // Si estamos en primera persona, cambiar a tercera persona
            Debug.Log("Cambiando a TERCERA PERSONA...");
            SwitchToThirdPersonMode();
            isInFirstPersonMode = false;
        }
        else
        {
            // Si estamos en tercera persona, cambiar a primera persona
            Debug.Log("Cambiando a PRIMERA PERSONA...");
            SwitchToFirstPersonMode();
            isInFirstPersonMode = true;
        }
    }

    private void SwitchToFirstPersonMode()
    {
        // Desactivar solo la cámara de tercera persona (sin afectar la AimCamera)
        if (virtualCamera.gameObject.activeSelf)
        {
            virtualCamera.gameObject.SetActive(false);
        }

        // Activar la cámara de primera persona
        if (!firstPersonCamera.gameObject.activeSelf)
        {
            firstPersonCamera.gameObject.SetActive(true);
        }

        // Asegurarse de que la cámara de apuntado se mantenga activa
        aimCameraVCam.Priority += priorityBoostAmount; // Se le da alta prioridad cuando estamos en primera persona

        Debug.Log("Cámara de Primera Persona activada.");
    }

    private void SwitchToThirdPersonMode()
    {
        // Desactivar solo la cámara de primera persona (sin afectar la AimCamera)
        if (firstPersonCamera.gameObject.activeSelf)
        {
            firstPersonCamera.gameObject.SetActive(false);
        }

        // Activar la cámara de tercera persona
        if (!virtualCamera.gameObject.activeSelf)
        {
            virtualCamera.gameObject.SetActive(true);
        }

        // Reducir la prioridad de la cámara de apuntado cuando no estamos en primera persona
        aimCameraVCam.Priority -= priorityBoostAmount;

        Debug.Log("Cámara de Tercera Persona activada.");
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

    private void EnsureSingleAudioListener()
    {
        // Encontrar todos los AudioListeners en la escena
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();

        // Desactivar todos excepto el activo
        foreach (var listener in listeners)
        {
            listener.enabled = false;
        }

        // Activar solo el AudioListener correspondiente
        if (isInFirstPersonMode && firstPersonCamera != null)
        {
            var firstPersonListener = firstPersonCamera.GetComponent<AudioListener>();
            if (firstPersonListener != null) firstPersonListener.enabled = true;
        }
        else if (virtualCamera != null)
        {
            var thirdPersonListener = virtualCamera.GetComponent<AudioListener>();
            if (thirdPersonListener != null) thirdPersonListener.enabled = true;
        }
    }
}