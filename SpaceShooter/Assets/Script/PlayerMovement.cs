using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Move on X–Z plane (forward/back = Z, left/right = X)
        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // Clamp inside camera view
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Camera.main.ViewportToWorldPoint(pos);

        // Keep Y fixed at 0 so player stays on the ground plane
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }
}
