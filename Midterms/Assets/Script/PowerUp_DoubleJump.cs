using UnityEngine;

public class PowerUp_DoubleJump : MonoBehaviour
{
    public float rotateSpeed = 60f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.EnableDoubleJump();
            }

            Debug.Log("💙 Double Jump Power-Up Collected!");
            Destroy(gameObject);
        }
    }
}
