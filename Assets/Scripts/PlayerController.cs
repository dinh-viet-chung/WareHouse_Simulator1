using UnityEngine;
using UnityEngine.InputSystem; // Keep your new Input System
using UnityEngine.SceneManagement; // Added for Scene transition
using TMPro; // Added for UI Text control

/// <summary>
/// Moves forward/backward and rotates with WASD/Arrow keys.
/// Includes UI update and Scene Transition logic.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Tooltip("Forward/back speed (units/sec).")]
    public float speed = 5.0f;

    [Tooltip("Turn speed (degrees/sec).")]
    public float rotationSpeed = 120.0f;

    [Tooltip("Drag and drop the Text UI from the Canvas here.")]
    public TMP_Text statusText; // Added variable to display Money/Day UI

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogWarning("PlayerController needs a Rigidbody.");

        // Update the interface right upon entering the game using saved data
        UpdateUI();
    }

    private void FixedUpdate()
    {
        // If there is no keyboard connected, skip to prevent game crash
        if (Keyboard.current == null) return;

        Vector2 moveInput = Vector2.zero;

        // Forward/backward
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y = -1f;

        // Left/right (rotation)
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x = 1f;

        // Move in facing direction 
        Vector3 movement = transform.forward * moveInput.y * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // Y-axis rotation (invert when going backwards)
        float turnDirection = moveInput.x;
        if (moveInput.y < 0)
            turnDirection = -turnDirection;

        float turn = turnDirection * rotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    // Function to update the displayed text string for Money and Day
    private void UpdateUI()
    {
        if (statusText != null)
        {
            statusText.text = "Money: " + DataManager.TotalMoney + " | Day: " + DataManager.CurrentDay;
        }
    }

    // Handle Trigger collision to change Scene to the warehouse
    private void OnTriggerEnter(Collider other)
    {
        // When the character passes through the invisible area named ChangeSceneTrigger
        if (other.gameObject.name == "ChangeSceneTrigger")
        {
            SceneManager.LoadScene("Scene2_Warehouse");
        }
    }
}