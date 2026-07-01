using UnityEngine;

public enum BoxType { Blue, Yellow, Red }

public class CargoBox : MonoBehaviour
{
    [Header("Box Settings")]
    public BoxType boxType = BoxType.Blue;

    [HideInInspector] public float originalMass = 1.0f;
    private Material boxMaterial;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        // Cache the original mass set in Inspector
        originalMass = rb.mass;

        // Automatically set properties based on type
        ConfigureBoxType();
    }

    private void ConfigureBoxType()
    {
        // Try to get material to change colors dynamically if needed
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null) boxMaterial = renderer.material;

        switch (boxType)
        {
            case BoxType.Blue:
                rb.mass = 1.0f;
                if (boxMaterial != null) boxMaterial.color = Color.blue;
                break;

            case BoxType.Yellow:
                rb.mass = 5.0f; // Make it heavy so it naturally affects physics if pushed
                if (boxMaterial != null) boxMaterial.color = Color.yellow;
                break;

            case BoxType.Red:
                rb.mass = 1.0f;
                if (boxMaterial != null) boxMaterial.color = Color.red;
                break;
        }
    }

    // Visual feedback when forklift picks it up
    public void SetPickedUpColor()
    {
        // Optional: change transparency or secondary color when attached
    }

    // Visual feedback when forklift drops it properly
    public void SetDroppedColor()
    {
        if (boxMaterial != null)
        {
            // Turn it green to show it was successfully delivered
            boxMaterial.color = Color.green;
        }
    }
}