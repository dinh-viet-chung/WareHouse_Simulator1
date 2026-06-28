using UnityEngine;

public class CargoBox : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Color originalColor = Color.green; // Original color: Green

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Set the initial green color for the shipping container.
            meshRenderer.material.color = originalColor;
        }
    }

    // The function changes the color of the cargo container to red when it is chained/hooked to the vehicle.
    public void SetPickedUpColor()
    {
        if (meshRenderer != null) meshRenderer.material.color = Color.red;
    }

    // The function returns green when the item is released.
    public void SetDroppedColor()
    {
        if (meshRenderer != null) meshRenderer.material.color = originalColor;
    }
}