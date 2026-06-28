using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ForkliftController : MonoBehaviour
{
    [Header("Forklift Movement")]
    public float speed = 8.0f;
    public float rotationSpeed = 150.0f;
    private Rigidbody rb;

    [Header("Fork Lift Mechanism (Lift/Lower)")]
    public Transform forkVisual; // Drag and drop the "Fork" visual object here
    public float minForkHeight = 0.0f; // Lowest height (ground level)
    public float maxForkHeight = 2.0f; // Required maximum height
    public float liftSpeed = 1.0f;     // Lifting/lowering speed

    [Header("UI System")]
    public TMP_Text gameplayUIText;

    private float prepareTimer = 5.0f;
    private float workTimer = 60.0f;

    // Game States
    private bool isPreparing = true;
    private bool isWorking = false;
    private bool isEnding = false;

    // Cargo Management
    private CargoBox currentBoxNear = null; // Cargo box currently standing near
    private CargoBox attachedBox = null;    // Cargo box currently attached to the forklift
    private bool isBoxAttached = false;
    private bool isInDropZone = false;       // Whether the forklift is in the drop zone

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (forkVisual != null)
        {
            // Initialize the fork position at the lowest level
            forkVisual.localPosition = new Vector3(forkVisual.localPosition.x, minForkHeight, forkVisual.localPosition.z);
        }
    }

    void FixedUpdate()
    {
        // Only allow movement during the active work shift (isWorking == true)
        if (!isWorking) return;
        if (Keyboard.current == null) return;

        // CRITICAL CONSTRAINT: If cargo is attached but has NOT reached max height -> Lock movement
        if (isBoxAttached)
        {
            if (forkVisual.localPosition.y < maxForkHeight - 0.08f)
            {
                // Stop the vehicle from moving forward
                rb.linearVelocity = Vector3.zero;
                return;
            }
        }

        // Get WASD / Arrow keys movement input
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y = -1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x = 1f;

        Vector3 movement = transform.forward * moveInput.y * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        float turnDirection = moveInput.x;
        if (moveInput.y < 0) turnDirection = -turnDirection;
        float turn = turnDirection * rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }

    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null) return;

        // 1. TIME AND WORKSHIFT STATE MANAGEMENT
        if (isPreparing)
        {
            prepareTimer -= Time.deltaTime;
            UpdateUI();
            if (prepareTimer <= 0)
            {
                isPreparing = false;
                isWorking = true; // Start the 60-second shift!
            }
        }
        else if (isWorking)
        {
            workTimer -= Time.deltaTime;
            UpdateUI();

            // LIFT / LOWER CARGO HANDLING VIA MOUSE
            if (Mouse.current.leftButton.isPressed) // HOLD LEFT MOUSE to lift cargo up
            {
                float newY = Mathf.Min(forkVisual.localPosition.y + liftSpeed * Time.deltaTime, maxForkHeight);
                forkVisual.localPosition = new Vector3(forkVisual.localPosition.x, newY, forkVisual.localPosition.z);
            }
            else if (Mouse.current.rightButton.isPressed) // HOLD RIGHT MOUSE to lower cargo down
            {
                float newY = Mathf.Max(forkVisual.localPosition.y - liftSpeed * Time.deltaTime, minForkHeight);
                forkVisual.localPosition = new Vector3(forkVisual.localPosition.x, newY, forkVisual.localPosition.z);
            }

            // PRESS [F] TO ATTACH / DETACH CARGO
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                HandleAttachment();
            }

            if (workTimer <= 0)
            {
                workTimer = 0;
                StartCoroutine(EndWorkShiftRoutine());
            }
        }
    }

    // Attachment logic via [F] key
    void HandleAttachment()
    {
        if (!isBoxAttached)
        {
            // Case 1: No cargo -> Press F to ATTACH/PICK UP CARGO
            if (currentBoxNear != null)
            {
                attachedBox = currentBoxNear;
                attachedBox.SetPickedUpColor(); // Change color to Red

                // Parent the cargo box to the fork mechanism so it moves along with it
                attachedBox.transform.SetParent(forkVisual);

                isBoxAttached = true;
            }
        }
        else
        {
            // Case 2: Holding cargo -> Press F to DETACH/DROP CARGO
            // Check if the forklift is within the DropZone and the fork is lowered to the ground
            if (isInDropZone && forkVisual.localPosition.y <= minForkHeight + 0.1f)
            {
                attachedBox.transform.SetParent(null); // Detach from the forklift
                attachedBox.SetDroppedColor();         // Revert color back to Green

                // Reward bonus immediately
                DataManager.AddMoney(100);

                attachedBox = null;
                isBoxAttached = false;
            }
        }
    }

    void UpdateUI()
    {
        if (gameplayUIText == null) return;

        if (isPreparing)
        {
            gameplayUIText.text = "PREPARING SHIFT: " + Mathf.CeilToInt(prepareTimer) + "s\nTOTAL MONEY: $" + DataManager.TotalMoney;
        }
        else if (isWorking)
        {
            string constraintWarning = "";
            if (isBoxAttached && forkVisual.localPosition.y < maxForkHeight - 0.08f)
            {
                constraintWarning = "\n<color=red>⚠️ KEEP THE LEFT MOUSE! </color>";
            }
            gameplayUIText.text = "TIME: " + Mathf.CeilToInt(workTimer) + "s\nMONEY: $" + DataManager.TotalMoney + constraintWarning;
        }
        else if (isEnding)
        {
            gameplayUIText.text = "<color=yellow>END OF WORKDAY!</color>\nMONEY: $" + DataManager.TotalMoney;
        }
    }

    // Coroutine to handle the end of workshift, wait 3 seconds, advance day count, and load Scene 1
    IEnumerator EndWorkShiftRoutine()
    {
        isWorking = false;
        isEnding = true;
        rb.linearVelocity = Vector3.zero; // Fully lock forklift movement
        UpdateUI();

        yield return new WaitForSeconds(3.0f); // Wait for exactly 3 seconds

        DataManager.AdvanceDay(); // Increment day count +1
        SceneManager.LoadScene("Scene1_Home"); // Automatically reload Scene 1
    }

    // Handle trigger interactions with cargo boxes and drop zone
    private void OnTriggerEnter(Collider other)
    {
        // Approaching a cargo box
        CargoBox box = other.GetComponent<CargoBox>();
        if (box != null && !isBoxAttached)
        {
            currentBoxNear = box;
        }

        // Entering the Drop Zone
        if (other.gameObject.name == "DropZone")
        {
            isInDropZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Moving away from a cargo box
        CargoBox box = other.GetComponent<CargoBox>();
        if (box != null && currentBoxNear == box)
        {
            currentBoxNear = null;
        }

        // Leaving the Drop Zone
        if (other.gameObject.name == "DropZone")
        {
            isInDropZone = false;
        }
    }
}