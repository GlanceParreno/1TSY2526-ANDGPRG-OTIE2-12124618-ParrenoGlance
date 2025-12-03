using UnityEngine;
public class SimpleMover : MonoBehaviour
{
    public Vector3 direction = Vector3.forward;
    public float speed = 20f;
    public float lifetime = 5f;
    float t;
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        t += Time.deltaTime;
        if (t >= lifetime) Destroy(gameObject);
    }
}
