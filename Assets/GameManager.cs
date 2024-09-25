using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameEnemyManager GameEnemyManager;
    // public int previousN = 0;

    void Start()
    {

    }

    void Update()
    {
        if (GameEnemyManager.TotalNumberOfEnemiesLeft <= 0)
        {
            // You Win
            SceneManager.LoadScene("WinScreen");
        }
    }
}