using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Portal : MonoBehaviour
{
    [Header("Optional UI Message")]
    public TMP_Text endMessage; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.AllCoinsCollected())
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

            // If there is another level, load it
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log($"Loading next level (index {nextSceneIndex})...");
                SceneManager.LoadScene(nextSceneIndex);
            }
            // If this is the final level, end the game here
            else
            {
                Debug.Log("Final level complete! Game Over.");

                // Freeze the game
                Time.timeScale = 0f;

                if (endMessage != null)
                {
                    endMessage.text = "You Win!";
                    endMessage.gameObject.SetActive(true);
                }
            }
        }
    }
}
