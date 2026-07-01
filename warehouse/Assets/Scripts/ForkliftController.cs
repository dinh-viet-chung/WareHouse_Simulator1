using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for Slider Control
using System.Collections;
using TMPro;

public class ForkliftController : MonoBehaviour
{
    [Header("Forklift Movement")]
    public float baseSpeed = 8.0f;
    public float rotationSpeed = 150.0f;
    private Rigidbody rb;
    private float currentSpeed;

    [Header("Fork Lift Mechanism (Lift/Lower)")]
    public Transform forkVisual;
    public float minForkHeight = 0.0f;
    public float maxForkHeight = 2.0f;
    public float liftSpeed = 1.0f;

    [Header("Fuel System")]
    public float maxFuel = 100f;
    public float fuelBurnRate = 1.5f; // Fuel lost per second
    public Slider fuelSlider; // Drag and drop UI Slider here
    private float currentFuel;

    [Header("UI System")]
    public TMP_Text gameplayUIText;

    private float prepareTimer = 5.0f;
    private float workTimer = 60.0f;

    private bool isPreparing = true;
    private bool isWorking = false;
    private bool isEnding = false;

    // Cargo Management
    private CargoBox currentBoxNear = null;
    private CargoBox attachedBox = null;
    private bool isBoxAttached = false;
    private bool isInDropZone = false;

    // Fragile Red Box Check variables
    private float lastForkHeight = 0.0f;
    private bool isDroppingTooFast = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = baseSpeed;
        currentFuel = maxFuel;

        if (fuelSlider != null)
        {
            fuelSlider.maxValue = maxFuel;
            fuelSlider.value = currentFuel;
        }

        if (forkVisual != null)
        {
            forkVisual.localPosition = new Vector3(forkVisual.localPosition.x, minForkHeight, forkVisual.localPosition.z);
            lastForkHeight = forkVisual.localPosition.y;
        }
    }

    void FixedUpdate()
    {
        if (!isWorking) return;
        if (Keyboard.current == null) return;

        // CRITICAL CONSTRAINT: If cargo attached but not at max height -> Lock movement
        if (isBoxAttached)
        {
            if (forkVisual.localPosition.y < maxForkHeight - 0.08f)
            {
                rb.linearVelocity = Vector3.zero;
                return;
            }
        }

        // WASD Input
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y = -1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x = 1f;

        // Apply movement using calculated speed modifiers
        Vector3 movement = transform.forward * moveInput.y * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        float turnDirection = moveInput.x;
        if (moveInput.y < 0) turnDirection = -turnDirection;
        float turn = turnDirection * rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }

    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null) return;

        if (isPreparing)
        {
            prepareTimer -= Time.deltaTime;
            UpdateUI();
            if (prepareTimer <= 0)
            {
                isPreparing = false;
                isWorking = true;
            }
        }
        else if (isWorking)
        {
            workTimer -= Time.deltaTime;

            // 1. FUEL MANAGEMENT
            currentFuel -= fuelBurnRate * Time.deltaTime;
            if (fuelSlider != null) fuelSlider.value = currentFuel;

            if (currentFuel <= 0)
            {
                currentFuel = 0;
                StartCoroutine(EndWorkShiftRoutine()); // Out of fuel! End immediately
                return;
            }

            // 2. SPEED MODIFIER BASED ON BOX TYPE
            if (isBoxAttached && attachedBox != null && attachedBox.boxType == BoxType.Yellow)
            {
                currentSpeed = baseSpeed * 0.5f; // Slow down 50%
            }
            else
            {
                currentSpeed = baseSpeed;
            }

            // 3. LIFT / LOWER AND FRAGILE RED BOX CHECK
            float currentHeight = forkVisual.localPosition.y;
            if (Mouse.current.leftButton.isPressed)
            {
                float newY = Mathf.Min(currentHeight + liftSpeed * Time.deltaTime, maxForkHeight);
                forkVisual.localPosition = new Vector3(forkVisual.localPosition.x, newY, forkVisual.localPosition.z);
            }
            else if (Mouse.current.rightButton.isPressed)
            {
                float newY = Mathf.Max(currentHeight - liftSpeed * Time.deltaTime, minForkHeight);
                forkVisual.localPosition = new Vector3(forkVisual.localPosition.x, newY, forkVisual.localPosition.z);

                // Check speed of lowering
                float speedOfLowering = (lastForkHeight - currentHeight) / Time.deltaTime;
                // If it drops faster than 80% of normal liftSpeed, mark it as crashing down
                if (isBoxAttached && attachedBox != null && attachedBox.boxType == BoxType.Red)
                {
                    if (speedOfLowering > (liftSpeed * 0.8f))
                    {
                        isDroppingTooFast = true;
                    }
                }
            }
            lastForkHeight = forkVisual.localPosition.y;

            // PRESS F TO ATTACH/DETACH
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                HandleAttachment();
            }

            UpdateUI();

            if (workTimer <= 0)
            {
                workTimer = 0;
                StartCoroutine(EndWorkShiftRoutine());
            }
        }
    }

    void HandleAttachment()
    {
        if (!isBoxAttached)
        {
            if (currentBoxNear != null)
            {
                attachedBox = currentBoxNear;
                attachedBox.SetPickedUpColor();
                attachedBox.transform.SetParent(forkVisual);
                isBoxAttached = true;
                isDroppingTooFast = false; // Reset warning state
            }
        }
        else
        {
            if (isInDropZone && forkVisual.localPosition.y <= minForkHeight + 0.1f)
            {
                attachedBox.transform.SetParent(null);
                attachedBox.SetDroppedColor();

                // Reward Calculation based on Box Type
                int reward = 100;
                string typeBonusMessage = "";

                if (attachedBox.boxType == BoxType.Yellow) reward = 300;
                if (attachedBox.boxType == BoxType.Red)
                {
                    if (isDroppingTooFast)
                    {
                        reward = 20; // Heavy penalty for dropping too fast
                        Debug.LogWarning("Fragile box broken due to fast dropping! Small reward.");
                    }
                    else
                    {
                        reward = 400; // Perfect bonus for slow dropping
                    }
                }

                DataManager.AddMoney(reward);

                attachedBox = null;
                isBoxAttached = false;
                isDroppingTooFast = false;
            }
        }
    }

    // Function to add fuel when running over a fuel station item
    public void Refuel(float amount)
    {
        currentFuel = Mathf.Min(currentFuel + amount, maxFuel);
        if (fuelSlider != null) fuelSlider.value = currentFuel;
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
            else if (isBoxAttached && attachedBox != null && attachedBox.boxType == BoxType.Red && isDroppingTooFast)
            {
                constraintWarning = "\n<color=yellow>⚠️ DROP SLOWER! IT'S FRAGILE!</color>";
            }

            gameplayUIText.text = "TIME: " + Mathf.CeilToInt(workTimer) + "s\nMONEY: $" + DataManager.TotalMoney + constraintWarning;
        }
        else if (isEnding)
        {
            gameplayUIText.text = "<color=yellow>END OF WORKDAY!</color>\nMONEY: $" + DataManager.TotalMoney;
        }
    }

    IEnumerator EndWorkShiftRoutine()
    {
        isWorking = false;
        isEnding = true;
        rb.linearVelocity = Vector3.zero;
        UpdateUI();

        yield return new WaitForSeconds(3.0f);

        DataManager.AdvanceDay();
        SceneManager.LoadScene("Scene1_Home");
    }

    private void OnTriggerEnter(Collider other)
    {
        CargoBox box = other.GetComponent<CargoBox>();
        if (box != null && !isBoxAttached)
        {
            currentBoxNear = box;
        }

        if (other.gameObject.name == "DropZone")
        {
            isInDropZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CargoBox box = other.GetComponent<CargoBox>();
        if (box != null && currentBoxNear == box)
        {
            currentBoxNear = null;
        }

        if (other.gameObject.name == "DropZone")
        {
            isInDropZone = false;
        }
    }
}