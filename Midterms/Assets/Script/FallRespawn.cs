using UnityEngine;

public class FallRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;  // Assign in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            // Reset position
            other.transform.position = respawnPoint.position;

            // Reset movement
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log("↩️ Player respawned at start point!");
        }
    }
}
