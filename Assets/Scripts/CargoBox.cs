using UnityEngine;

public class CargoBox : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Color originalColor = Color.green; // Màu ban đầu: Xanh lá

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Thiết lập màu xanh lá ban đầu cho thùng hàng
            meshRenderer.material.color = originalColor;
        }
    }

    // Hàm đổi màu thùng hàng sang Đỏ khi bị xích/móc vào xe
    public void SetPickedUpColor()
    {
        if (meshRenderer != null) meshRenderer.material.color = Color.red;
    }

    // Hàm trả lại màu Xanh lá khi nhả hàng ra
    public void SetDroppedColor()
    {
        if (meshRenderer != null) meshRenderer.material.color = originalColor;
    }
}