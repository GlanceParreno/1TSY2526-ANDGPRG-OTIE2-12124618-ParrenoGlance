using UnityEngine;

public class Coin : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddCoin();
            Destroy(gameObject);
        }
    }
}
