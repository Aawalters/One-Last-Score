using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEnemyManager : MonoBehaviour
{
    public Transform playerTransform;
    public List<GameObject> spawnedEnemies = new List<GameObject>();
    public int TotalNumberOfEnemiesLeft;


    [Header("Spawn Settings")]
    public List<Transform> spawnPoints;  // List of possible spawn points
    public GameObject enemyPrefab;
    public float firstSpawnDelay = 7f;   // initial delay before first wave
    public float spawnInterval = 5f;     // Time between each wave

    [Header("Wave Settings")]
    public List<int> waveConfigurations; // List of enemy count per wave
    private int currentWave = 0;         // Current wave number
    // Start is called before the first frame update
    void Start()
    {
        // for (int i = 0; i < numberOfEnemies; i++)
        // {
        //     GameObject newEnemy = Instantiate(EnemyPrefab, spawnPoint.position, spawnPoint.rotation);
        //     Enemy_Basic enemyRef = newEnemy.GetComponent<Enemy_Basic>();
        //     enemyRef.player = playerTransform;
        //     spawnedEnemies.Add(newEnemy);
        // }
        StartWaves();
    }

    public void StartWaves() {
        TotalNumberOfEnemiesLeft = waveConfigurations.Sum();
        StartCoroutine(SpawnWaveRoutine());
    }

    public void death(GameObject enemy)
    {
        if (enemy != null) {
            TotalNumberOfEnemiesLeft -= 1;
            spawnedEnemies.Remove(enemy);
            Destroy(enemy);
        }
    }

    public void BuffEnemies(int ExtraHealth, int ExtraDamage, float ExtraSpeed)
    {
        foreach(GameObject enemy in spawnedEnemies) {
            if (enemy != null) {
                Enemy_Basic enemyRef = enemy.GetComponent<Enemy_Basic>();
                // Code to execute for each item
                enemyRef.maxHealth += ExtraHealth;
                enemyRef.currentHealth += ExtraHealth;
                enemyRef.chaseSpeed += ExtraSpeed;
                enemyRef.punchDamage += ExtraDamage;
            }
        }
    }

    private IEnumerator SpawnWaveRoutine()
    {
        while (currentWave < waveConfigurations.Count)
        {
            if (currentWave < 1) {
                yield return new WaitForSeconds(firstSpawnDelay);
            } else {
                yield return new WaitForSeconds(spawnInterval);
            }

            // Get the number of enemies to spawn for the current wave
            int enemiesToSpawn = waveConfigurations[currentWave];

            // Choose a random number of spawn points, capped by the total spawn points available
            int randomSpawnPoints = Mathf.Min(enemiesToSpawn, spawnPoints.Count);
            List<Transform> selectedSpawnPoints = GetRandomSpawnPoints(randomSpawnPoints);

            // Spawn enemies equally spread out among the selected spawn points
            SpawnEnemies(enemiesToSpawn, selectedSpawnPoints);

            // Move to the next wave
            currentWave++;
        }
    }

    private List<Transform> GetRandomSpawnPoints(int count)
    {
        // Create a temporary list to avoid modifying the original spawn points list
        List<Transform> tempSpawnPoints = new List<Transform>(spawnPoints);

        // List to hold the selected spawn points
        List<Transform> selectedPoints = new List<Transform>();

        // Randomly select the requested number of spawn points
        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, tempSpawnPoints.Count);
            selectedPoints.Add(tempSpawnPoints[randomIndex]);
            tempSpawnPoints.RemoveAt(randomIndex); // Remove to avoid duplicates
        }

        return selectedPoints;
    }

    private void SpawnEnemies(int totalEnemies, List<Transform> spawnPoints)
    {
        int enemiesPerPoint = totalEnemies / spawnPoints.Count;   // Divide enemies equally
        int remainingEnemies = totalEnemies % spawnPoints.Count;  // Handle any leftover enemies

        foreach (Transform spawnPoint in spawnPoints)
        {
            for (int i = 0; i < enemiesPerPoint; i++)
            {
                SpawnEnemy(spawnPoint);
            }

            // Spawn one extra enemy if there are remaining enemies
            if (remainingEnemies > 0)
            {
                SpawnEnemy(spawnPoint);
                remainingEnemies--;
            }
        }
    }

    private void SpawnEnemy(Transform spawnPoint) {
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Enemy_Basic enemyRef = newEnemy.GetComponent<Enemy_Basic>();
        enemyRef.player = playerTransform;
        enemyRef.GameEnemyManager = this;
        spawnedEnemies.Add(newEnemy);
    }
}
