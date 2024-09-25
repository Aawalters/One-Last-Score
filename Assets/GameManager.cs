using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameEnemyManager GameEnemyManager;
    public Player Player;
    public int quota = 2300;
    // public int previousN = 0;

    void Start()
    {

    }

    void Update()
    {
        if (GameEnemyManager.TotalNumberOfEnemiesLeft <= 0 && (Player.wager * Player.multiplier) >= quota)
        {
            // You Win
            SceneManager.LoadScene("WinScreen");
        } else {
            // SceneManager.LoadScene("WinScreen");
        }
    }
}