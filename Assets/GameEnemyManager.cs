using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnemyManager : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject EnemyPrefab;
    public Transform playerTransform;
    public int numberOfEnemies;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject newEnemy = Instantiate(EnemyPrefab, spawnPoint.position, spawnPoint.rotation);
            Enemy_Basic enemyRef = newEnemy.GetComponent<Enemy_Basic>();
            enemyRef.player = playerTransform;
            spawnedEnemies.Add(newEnemy);
        }
    }

    // Update is called once per frame
    public void death(GameObject enemy)
    {
        spawnedEnemies.Remove(enemy);
        Destroy(enemy);
    }

    public void BuffEnemies(int ExtraHealth, int ExtraDamage)
    {
        foreach(GameObject enemy in spawnedEnemies) {
            Enemy_Basic enemyRef = enemy.GetComponent<Enemy_Basic>();
            // Code to execute for each item
            enemyRef.maxHealth += ExtraHealth;
            enemyRef.currentHealth += ExtraHealth;

            enemyRef.punchDamage += ExtraDamage;
        }
    }
}
