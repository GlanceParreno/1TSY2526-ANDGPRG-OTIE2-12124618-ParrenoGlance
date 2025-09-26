using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 10;
    public float speed = 3f;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);

        if (transform.position.z < -22f)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
    if (GameManager.Instance != null)
    {
        GameManager.Instance.AddScore(10);
    }

    Destroy(gameObject);
    }

}
