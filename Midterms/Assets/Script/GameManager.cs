using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Coin UI")]
    public TMP_Text coinText;   // Uses TextMeshPro
    public int totalCoins;
    private int collectedCoins;

    void Awake()
    {
        // Singleton pattern for easy access
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        collectedCoins = 0;
        UpdateUI();
    }

    public void AddCoin()
    {
        collectedCoins++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {collectedCoins} / {totalCoins}";
    }

    public bool AllCoinsCollected()
    {
        return collectedCoins >= totalCoins;
    }
}
