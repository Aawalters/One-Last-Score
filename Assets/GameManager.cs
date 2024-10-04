using System;
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
    public GameObject Canvas;
    public bool mobile;

    private GameObject PlayScreen;
    private GameObject WagerScreen;
    private GameObject PauseScreen;
    private GameObject DeathScreen;
    private GameObject WinScreen;


    public float quota;

    private PlayerController p;
    private TextMeshProUGUI wager_text;
    private TextMeshProUGUI quota_text;

    private bool paused = false;

    void Awake()
    {
        PlayScreen = Canvas.transform.Find("Play Screen").gameObject;
        WagerScreen = Canvas.transform.Find("Wager Screen").gameObject;
        PauseScreen = Canvas.transform.Find("Pause Screen").gameObject;
        DeathScreen = Canvas.transform.Find("Death Screen").gameObject;
        WinScreen = Canvas.transform.Find("Win Screen").gameObject;

        p = Player.GetComponent<PlayerController>();
        wager_text = PlayScreen.transform.Find("Wager").gameObject.GetComponent<TextMeshProUGUI>();
        quota_text = PlayScreen.transform.Find("Quota").gameObject.GetComponent<TextMeshProUGUI>();
        wager_text.text = "Wager:" + p.p.wager.ToString();
    }

    void Update()
    {
        if (GameEnemyManager.TotalNumberOfEnemiesLeft <= 0)
        {
            int total_score = ((int)(p.p.wager * p.p.multiplier));
            if (total_score >= quota)
            {
                Win();
            }
            else
            {
                Death();
            }
        } 
    }

    public void freeze(bool status)
    {
        if (status)
        {
            p.SetControls(false);
            Time.timeScale = 0f;
        } 
        else
        {
            p.SetControls(true);
            Time.timeScale = 1f;
        }   
    }

    public void updateWager()
    {
        // Debug.Log("player cur wager: " + p.p.wager);
        // Debug.Log("player cur multiplier: " + p.p.multiplier);
        int total_score = ((int)(p.p.wager * p.p.multiplier));
        wager_text.text = "Wager:" + total_score.ToString();
    }

    public void Wager()
    {
        freeze(true);
        PlayScreen.SetActive(false);
        WagerScreen.SetActive(true);
    }
    public void WagerChoice(int value)
    {
        freeze(false);
        p.p.wager = value;
        PlayScreen.SetActive(true);
        WagerScreen.SetActive(false);
        updateWager();
    }
    public void Pause()
    {
        if (!paused)
        {
            // pause
            freeze(true);
            paused = true;
            PlayScreen.SetActive(false);
            PauseScreen.SetActive(true);
        }
        else
        {
            // unpause
            freeze(false);
            paused = false;
            PlayScreen.SetActive(true);
            PauseScreen.SetActive(false);
        }
    }
    public void Death()
    {
        p.SetControls(false);
        // animate death here
        int final_payout = (int)(p.p.wager * p.p.multiplier - quota);
        DeathScreen.SetActive(true);
        TextMeshProUGUI ScoreText = DeathScreen.GetComponentInChildren<TextMeshProUGUI>();
        ScoreText.text = "Final Payout: " + final_payout.ToString();
    }
    public void Win()
    {
        p.SetControls(false);
        // animate win here (gangnam style)
        int final_payout = (int)(p.p.wager * p.p.multiplier - quota);
        WinScreen.SetActive(true);
        TextMeshProUGUI ScoreText = WinScreen.GetComponentInChildren<TextMeshProUGUI>();
        ScoreText.text = "Final Payout: " + final_payout.ToString();
    }
    public void Restart(string scene_name)
    {
        SceneManager.LoadScene(scene_name); // "DiegoTestingScene"
    }
    public void Map()
    {
        SceneManager.LoadScene("MapMenuScene");
    }
    public void Menu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}