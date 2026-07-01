using UnityEngine;

public class FuelCanister : MonoBehaviour
{
    [Tooltip("Amount of fuel restored when collected.")]
    public float fuelRestoreAmount = 30f;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that hit the canister is the Forklift
        ForkliftController forklift = other.GetComponent<ForkliftController>();
        if (forklift != null)
        {
            forklift.Refuel(fuelRestoreAmount);
            Destroy(gameObject); // Remove the canister from the scene after pickup
        }
    }
}