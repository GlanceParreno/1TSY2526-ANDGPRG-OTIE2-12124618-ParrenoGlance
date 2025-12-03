using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TD.UI;

namespace TD
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Core Stats")]
        public int coreLives = 20;

        [Header("Economy")]
        public int gold = 100;
        public int goldPerEnemy = 5;

        [Header("Waves")]
        public int currentWave = 0;

        [Header("State")]
        public bool isGameOver = false;


        private readonly List<Enemy> activeEnemies = new List<Enemy>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {

            UIManagerTMP.Instance?.UpdateGold(gold);
            UIManagerTMP.Instance?.UpdateWave(currentWave);
            UIManagerTMP.Instance?.UpdateLives(coreLives);
        }




        public void RegisterEnemy(Enemy e)
        {
            if (e != null && !activeEnemies.Contains(e))
                activeEnemies.Add(e);
        }

        public void NotifyEnemyDeath(Enemy e)
        {
            if (e != null && activeEnemies.Contains(e))
                activeEnemies.Remove(e);
        }





        public int ActiveEnemyCount()
        {
            return activeEnemies.Count;
        }




        public void CoreTakeDamage(int amount)
        {
            if (isGameOver) return;

            coreLives -= amount;
            if (coreLives < 0) coreLives = 0;

            UIManagerTMP.Instance?.UpdateLives(coreLives);

            if (coreLives <= 0)
                GameOver();
        }

        public void GameOver()
        {
            if (isGameOver) return;

            isGameOver = true;
            Debug.Log("[GameManager] GAME OVER");


            var spawners = FindObjectsOfType<EnemySpawner>();
            foreach (var s in spawners)
                s.enabled = false;





            UIManagerTMP.Instance?.ShowGameOver();
        }




        public void StartNextWave()
        {
            currentWave++;
            UIManagerTMP.Instance?.UpdateWave(currentWave);
        }




        public void AddGold(int amount)
        {
            gold += amount;
            if (gold < 0) gold = 0;

            UIManagerTMP.Instance?.UpdateGold(gold);
        }

        public bool SpendGold(int amount)
        {
            if (gold < amount)
                return false;

            gold -= amount;
            UIManagerTMP.Instance?.UpdateGold(gold);
            return true;
        }




        [ContextMenu("Give 100 Gold")]
        public void GiveGold()
        {
            AddGold(100);
        }

        [ContextMenu("Damage Core by 1")]
        public void DamageCoreDebug()
        {
            CoreTakeDamage(1);
        }

        [ContextMenu("Reset Values")]
        public void ResetGame()
        {
            coreLives = 20;
            gold = 100;
            currentWave = 0;

            UIManagerTMP.Instance?.UpdateGold(gold);
            UIManagerTMP.Instance?.UpdateWave(currentWave);
            UIManagerTMP.Instance?.UpdateLives(coreLives);

            isGameOver = false;
        }
        public void RestartGame()
        {

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitGame()
        {
            Application.Quit();
            Debug.Log("Quit Game Called");
        }

    }
}
