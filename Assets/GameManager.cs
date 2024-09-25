using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameEnemyManager GameEnemyManager;
    public GameObject Player;
    public PlayerController p;
    public TextMeshProUGUI wager_text;
    public TextMeshProUGUI quota_text;
    public float quota;
    // public int previousN = 0;

    void Start()
    {
        wager_text.text = "Wager:" + p.p.wager.ToString();
        p = Player.GetComponent<PlayerController>();
    
    }

    void Update()
    {
        if (GameEnemyManager.TotalNumberOfEnemiesLeft <= 0)
        {
            // You Win
            if ((p.p.wager * p.p.multiplier) >= quota)
            {
                SceneManager.LoadScene("WinScreen");
            }
            else
            {
                // SceneManager.LoadScene("WinScreen");
                p.p.DeathScreen.Setup(((int)(p.p.wager * p.p.multiplier)));
            }
        } 
    }

    public void updateWager()
    {
        wager_text.text = "Wager:" + (p.p.wager * p.p.multiplier).ToString();
    }
}