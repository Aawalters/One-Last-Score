using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEnemyManager : MonoBehaviour
{
    public GameObject Player;
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

    [Header("FX")]
    // death fx
    public GameObject EnemyOnScreenDeathPrefab;
    public GameObject EnemyOffScreenDeathPrefab;
    private SpriteRenderer _OffScreenDeathRend;
    private float _OffScreenSpriteWidth;
    private float _OffScreenSpriteHeight;
    private Camera _Camera;

    void Awake() {
        Player = GameObject.FindGameObjectWithTag("Player");
        _Camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        _OffScreenDeathRend = EnemyOffScreenDeathPrefab.GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        var bounds = _OffScreenDeathRend.bounds;
        _OffScreenSpriteWidth = bounds.size.x / 2f;
        _OffScreenSpriteHeight = bounds.size.y / 2f;
        StartWaves();
    }

    public void StartWaves() {
        TotalNumberOfEnemiesLeft = waveConfigurations.Sum();
        StartCoroutine(SpawnWaveRoutine());
    }

    public void Death(GameObject enemy)
    {
        if (enemy != null) {
            TotalNumberOfEnemiesLeft -= 1;
            spawnedEnemies.Remove(enemy);
            StartCoroutine(WaitForSpawn(enemy));
        }
    }

    // NOTE: need to redo buff enemies so it applies to current AND FUTURE enemies
    public void BuffEnemies(int ExtraHealth, int ExtraDamage, float ExtraSpeed)
    {
        foreach(GameObject enemy in spawnedEnemies) {
            if (enemy != null) {
                Enemy enemyRef = enemy.GetComponent<Enemy>();
                // Code to execute for each item
                enemyRef.MaxHealth += ExtraHealth;
                enemyRef.CurrentHealth += ExtraHealth;
                enemyRef.ChaseSpeed += ExtraSpeed;
                enemyRef.PunchDamage += ExtraDamage;
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
        Enemy enemyRef = newEnemy.GetComponent<Enemy>();
        enemyRef.Player = Player;
        enemyRef.GameEnemyManager = this;
        spawnedEnemies.Add(newEnemy);
    }

    #region FX
    IEnumerator WaitForSpawn(GameObject enemy) {
        while (Time.timeScale != 1.0f) { // wait until after hit stop fx
            yield return null;
        }
        Destroy(enemy);
        DeathIndication(enemy);
    }

    // if enemy is offscreen, then spawn offscreen arrow pointing at them, otherwise normal death fx
    private void DeathIndication(GameObject enemy) {
        Vector3 screenPos = _Camera.WorldToViewportPoint(enemy.transform.position);
        bool isOffScreen = screenPos.x <= 0 || screenPos.x >= 1 || screenPos.y <= 0 || screenPos.y >= 1;
        
        if (isOffScreen) {
            GameObject indicator = Instantiate(EnemyOffScreenDeathPrefab);
            Vector3 spriteSizeInViewPort = _Camera.WorldToViewportPoint(new Vector3(_OffScreenSpriteWidth, _OffScreenSpriteHeight, 0))
                - _Camera.WorldToViewportPoint(Vector3.zero);

            screenPos.x = Mathf.Clamp(screenPos.x, spriteSizeInViewPort.x, 1 - spriteSizeInViewPort.x);
            screenPos.y = Mathf.Clamp(screenPos.y, spriteSizeInViewPort.y, 1 - spriteSizeInViewPort.y);

            Vector3 worldPosition = _Camera.ViewportToWorldPoint(screenPos);
            worldPosition.z = 0;
            indicator.transform.position = worldPosition;

            Vector3 direction = enemy.transform.position - indicator.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            indicator.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        } else {
            Instantiate(EnemyOnScreenDeathPrefab, enemy.transform.position, Quaternion.identity);
        }
    }
    #endregion
}
