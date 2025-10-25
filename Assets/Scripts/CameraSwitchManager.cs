using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitchManager : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private int priorityBoostAmount = 10;

    [SerializeField] private Canvas thirdPersonCanvas;
    [SerializeField] private Canvas aimCanvas;

    [Header("C�maras")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;  // C�mara de tercera persona
    [SerializeField] private Camera firstPersonCamera;  // C�mara de primera persona
    [SerializeField] private CinemachineVirtualCamera aimCameraVCam;  // C�mara de apuntado (Cinemachine)

    [Header("Configuraci�n de la c�mara en primera persona")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private float mouseSensitivity = 100f;

    [Header("Ret�cula")]
    [SerializeField] private Canvas crosshairCanvas;

    private InputAction aimAction;
    private InputAction switchToFirstPersonAction;
    public bool isInFirstPersonMode = false;

    private float xRotation = 0f;

    // M�todo p�blico para acceder a 'isInFirstPersonMode'
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
                Debug.LogError("No se encontr� un componente PlayerInput en este GameObject. Aseg�rate de asignarlo en el Inspector.");
                return;
            }
        }

        aimAction = playerInput.actions["Aim"];
        switchToFirstPersonAction = playerInput.actions["SwitchCamera"];

        // Inicializar las c�maras
        isInFirstPersonMode = false;
        virtualCamera?.gameObject.SetActive(true);
        firstPersonCamera?.gameObject.SetActive(false);
        aimCameraVCam?.gameObject.SetActive(true);  // Mantener la c�mara de apuntado activa

        EnsureSingleAudioListener(); // Asegurarnos de que solo un AudioListener est� activo
    }

    private void OnEnable()
    {
        if (playerInput == null)
        {
            Debug.LogError("playerInput no est� asignado.");
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
            Debug.LogError("aimCameraVCam no est� asignado. No se puede iniciar la acci�n de apuntar.");
            return;
        }

        // Aumentar la prioridad solo cuando se comienza a apuntar
        aimCameraVCam.Priority += priorityBoostAmount;

        // Asegurarse de que la c�mara de apuntado est� activa
        if (!aimCameraVCam.gameObject.activeSelf)
        {
            aimCameraVCam.gameObject.SetActive(true); // Asegurarse de que la c�mara de apuntado no se desactive
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
            Debug.LogError("aimCameraVCam no est� asignado. No se puede cancelar la acci�n de apuntar.");
            return;
        }

        // Reducir la prioridad de la c�mara de apuntado cuando se cancela el apuntado
        aimCameraVCam.Priority -= priorityBoostAmount;

        if (aimCanvas != null)
        {
            aimCanvas.enabled = false;
        }
    }

    private void ToggleFirstPersonMode(InputAction.CallbackContext context)
    {
        // Verificar el estado actual de la c�mara
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
        // Desactivar solo la c�mara de tercera persona (sin afectar la AimCamera)
        if (virtualCamera.gameObject.activeSelf)
        {
            virtualCamera.gameObject.SetActive(false);
        }

        // Activar la c�mara de primera persona
        if (!firstPersonCamera.gameObject.activeSelf)
        {
            firstPersonCamera.gameObject.SetActive(true);
        }

        // Asegurarse de que la c�mara de apuntado se mantenga activa
        aimCameraVCam.Priority += priorityBoostAmount; // Se le da alta prioridad cuando estamos en primera persona

        Debug.Log("C�mara de Primera Persona activada.");
    }

    private void SwitchToThirdPersonMode()
    {
        // Desactivar solo la c�mara de primera persona (sin afectar la AimCamera)
        if (firstPersonCamera.gameObject.activeSelf)
        {
            firstPersonCamera.gameObject.SetActive(false);
        }

        // Activar la c�mara de tercera persona
        if (!virtualCamera.gameObject.activeSelf)
        {
            virtualCamera.gameObject.SetActive(true);
        }

        // Reducir la prioridad de la c�mara de apuntado cuando no estamos en primera persona
        aimCameraVCam.Priority -= priorityBoostAmount;

        Debug.Log("C�mara de Tercera Persona activada.");
    }

    private void FirstPersonLook()
    {
        if (playerInput == null)
        {
            Debug.LogError("playerInput no est� asignado. No se puede procesar la rotaci�n.");
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