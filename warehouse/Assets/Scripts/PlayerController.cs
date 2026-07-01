using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 720.0f;
    private Rigidbody rb;
    private Vector3 moveDirection;

    [Header("Camera Settings (Mouse Orbit)")]
    public Transform mainCamera;
    public float cameraDistance = 4.5f;
    public float cameraHeightOffset = 1.2f;
    public float mouseXSpeed = 15.0f;
    public float mouseYSpeed = 15.0f;
    public float cameraYMinLimit = -10f;
    public float cameraYMaxLimit = 75f;

    private float cameraXRotation = 0.0f;
    private float cameraYRotation = 0.0f;

    [Header("UI System")]
    [Tooltip("Drag and drop the Text UI from the Canvas here.")]
    public TMP_Text statusText;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogWarning("PlayerController needs a Rigidbody.");
    }

    void Start()
    {
        if (mainCamera != null)
        {
            Vector3 angles = mainCamera.eulerAngles;
            cameraXRotation = angles.y;
            cameraYRotation = angles.x;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UpdateUI();
    }

    // CRITICAL FIX: Force UI to update every single time this scene is loaded/enabled
    private void OnEnable()
    {
        UpdateUI();
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ = -1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;

        if (mainCamera != null)
        {
            Vector3 camForward = mainCamera.forward;
            Vector3 camRight = mainCamera.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            moveDirection = (camForward * moveZ + camRight * moveX).normalized;
        }
        else
        {
            moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        cameraXRotation += mouseDelta.x * mouseXSpeed * Time.deltaTime;
        cameraYRotation -= mouseDelta.y * mouseYSpeed * Time.deltaTime;
        cameraYRotation = Mathf.Clamp(cameraYRotation, cameraYMinLimit, cameraYMaxLimit);
    }

    void FixedUpdate()
    {
        Vector3 velocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        if (mainCamera != null)
        {
            Vector3 targetLookAtPosition = transform.position + Vector3.up * cameraHeightOffset;
            Quaternion camRotation = Quaternion.Euler(cameraYRotation, cameraXRotation, 0);
            Vector3 positionOffset = camRotation * new Vector3(0.0f, 0.0f, -cameraDistance);

            mainCamera.rotation = camRotation;
            mainCamera.position = targetLookAtPosition + positionOffset;
        }
    }

    private void UpdateUI()
    {
        if (statusText != null)
        {
            statusText.text = "Money: $" + DataManager.TotalMoney + " | Day: " + DataManager.CurrentDay;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "ChangeSceneTrigger")
        {
            SceneManager.LoadScene("Scene2_Warehouse");
        }
    }
}