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

    [Header("Fork Lift Mechanism (Nâng Hạ)")]
    public Transform forkVisual; // Kéo thả khối "Càng nâng" vào đây
    public float minForkHeight = 0.0f; // Độ cao thấp nhất (Sát đất)
    public float maxForkHeight = 2.0f; // Độ cao tối đa bắt buộc
    public float liftSpeed = 1.0f;     // Tốc độ nâng hạ

    [Header("UI System")]
    public TMP_Text gameplayUIText;

    private float prepareTimer = 5.0f;
    private float workTimer = 60.0f;

    // Các trạng thái của Game
    private bool isPreparing = true;
    private bool isWorking = false;
    private bool isEnding = false;

    // Quản lý Thùng hàng
    private CargoBox currentBoxNear = null; // Thùng hàng đang đứng gần
    private CargoBox attachedBox = null;    // Thùng hàng đang móc trên xe
    private bool isBoxAttached = false;
    private bool isInDropZone = false;       // Xe đang nằm trong ô đặt hàng hay không

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (forkVisual != null)
        {
            // Cho càng nâng xuất phát ở vị trí thấp nhất
            forkVisual.localPosition = new Vector3(forkVisual.localPosition.x, minForkHeight, forkVisual.localPosition.z);
        }
    }

    void FixedUpdate()
    {
        // Chỉ cho phép lái xe khi đang trong ca làm việc chính thức (isWorking == true)
        if (!isWorking) return;
        if (Keyboard.current == null) return;

        // RÀNG BUỘC CHÍ MẠNG: Nếu đã móc hàng mà hàng CHƯA đạt độ cao tối đa -> Khóa di chuyển xe
        if (isBoxAttached)
        {
            if (forkVisual.localPosition.y < maxForkHeight - 0.05f)
            {
                // Dừng xe lại không cho đi tiếp
                rb.linearVelocity = Vector3.zero;
                return;
            }
        }

        // Nhận nút di chuyển WASD
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

        // 1. QUẢN LÝ THỜI GIAN VÀ TRẠNG THÁI CA LÀM VIỆC
        if (isPreparing)
        {
            prepareTimer -= Time.deltaTime;
            UpdateUI();
            if (prepareTimer <= 0)
            {
                isPreparing = false;
                isWorking = true; // Bắt đầu 60 giây làm việc!
            }
        }
        else if (isWorking)
        {
            workTimer -= Time.deltaTime;
            UpdateUI();

            // XỬ LÝ NÂNG / HẠ HÀNG BẰNG CHUỘT
            if (Mouse.current.leftButton.isPressed) // GIỮ CHUỘT TRÁI để nâng hàng lên
            {
                float newY = Mathf.Min(forkVisual.localPosition.y + liftSpeed * Time.deltaTime, maxForkHeight);
                forkVisual.localPosition = new Vector3(forkVisual.localPosition.x, newY, forkVisual.localPosition.z);
            }
            else if (Mouse.current.rightButton.isPressed) // GIỮ CHUỘT PHẢI để hạ hàng xuống
            {
                float newY = Mathf.Max(forkVisual.localPosition.y - liftSpeed * Time.deltaTime, minForkHeight);
                forkVisual.localPosition = new Vector3(forkVisual.localPosition.x, newY, forkVisual.localPosition.z);
            }

            // XỬ LÝ NHẤN PHÍM F ĐỂ MÓC / NHẢ HÀNG
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

    // Logic Móc/Nhả hàng bằng nút [F]
    void HandleAttachment()
    {
        if (!isBoxAttached)
        {
            // Trường hợp 1: Chưa có hàng -> Nhấn F để MÓC NHẶT HÀNG
            if (currentBoxNear != null)
            {
                attachedBox = currentBoxNear;
                attachedBox.SetPickedUpColor(); // Đổi sang màu Đỏ

                // Dính vật lý thùng hàng vào càng nâng để nâng lên hạ xuống theo xe
                attachedBox.transform.SetParent(forkVisual);

                isBoxAttached = true;
            }
        }
        else
        {
            // Trường hợp 2: Đang giữ hàng -> Nhấn F để NHẢ HÀNG
            // Kiểm tra xe phải đang nằm trên ô đặt hàng (DropZone) và càng nâng đã hạ thấp xuống
            if (isInDropZone && forkVisual.localPosition.y <= minForkHeight + 0.1f)
            {
                attachedBox.transform.SetParent(null); // Bỏ dính khỏi xe
                attachedBox.SetDroppedColor();         // Trả lại màu xanh

                // Cộng tiền thưởng ngay lập tức
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
            gameplayUIText.text = "CHUẨN BỊ CA LÀM: " + Mathf.CeilToInt(prepareTimer) + "s\nTỔNG TIỀN: $" + DataManager.TotalMoney;
        }
        else if (isWorking)
        {
            string constraintWarning = "";
            if (isBoxAttached && forkVisual.localPosition.y < maxForkHeight - 0.05f)
            {
                constraintWarning = "\n<color=red>⚠️ HÃY GIỮ CHUỘT TRÁI NÂNG HÀNG LÊN TỐI ĐA ĐỂ DI CHUYỂN!</color>";
            }
            gameplayUIText.text = "THỜI GIAN: " + Mathf.CeilToInt(workTimer) + "s\nTỔNG TIỀN: $" + DataManager.TotalMoney + constraintWarning;
        }
        else if (isEnding)
        {
            gameplayUIText.text = "<color=yellow>KẾT THÚC NGÀY LÀM VIỆC!</color>\nTỔNG TIỀN ĐÃ LƯU: $" + DataManager.TotalMoney;
        }
    }

    // Coroutine xử lý kết thúc ca làm, chờ 3 giây rồi tăng ngày, chuyển màn
    IEnumerator EndWorkShiftRoutine()
    {
        isWorking = false;
        isEnding = true;
        rb.linearVelocity = Vector3.zero; // Khóa di chuyển xe nâng hoàn toàn
        UpdateUI();

        yield return new WaitForSeconds(3.0f); // Chờ đúng 3 giây

        DataManager.AdvanceDay(); // Tăng biến ngày +1
        SceneManager.LoadScene("Scene1_Home"); // Tự động load lại Scene 1
    }

    // Kiểm tra va chạm cảm ứng vùng nhặt hàng và ô đặt hàng
    private void OnTriggerEnter(Collider other)
    {
        // Chạm vào thùng hàng
        CargoBox box = other.GetComponent<CargoBox>();
        if (box != null && !isBoxAttached)
        {
            currentBoxNear = box;
        }

        // Chạm vào ô đặt hàng (Drop Zone)
        if (other.gameObject.name == "DropZone")
        {
            isInDropZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Rời xa thùng hàng
        CargoBox box = other.GetComponent<CargoBox>();
        if (box != null && currentBoxNear == box)
        {
            currentBoxNear = null;
        }

        // Rời khỏi ô đặt hàng
        if (other.gameObject.name == "DropZone")
        {
            isInDropZone = false;
        }
    }
}