using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import the TextMeshPro namespace

public class GameManager : MonoBehaviour
{
    public int enemyCount; // Total number of enemies
    public float timer = 60f; // Timer countdown in seconds
    public TextMeshProUGUI winText; // UI Text for "You Win"
    public TextMeshProUGUI loseText; // UI Text for "You Lose"
    public TextMeshProUGUI timerText; // UI Text for the timer display

    private bool gameEnded = false; // To lock the win/lose conditions

    void Start()
    {
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (gameEnded)
            return; // Lock the game state if already ended

        // Countdown the timer
        timer -= Time.deltaTime;
        UpdateTimerDisplay();

        // Check if the timer has run out
        if (timer <= 0)
        {
            timer = 0; // Clamp the timer to zero
            GameOver(false); // Trigger lose condition
        }
    }

    // Method to decrement the enemy counter when an enemy dies
    public void OnEnemyKilled()
    {
        if (gameEnded)
            return; // Do nothing if the game is already over

        enemyCount--;

        // Check if all enemies are defeated
        if (enemyCount <= 0)
        {
            GameOver(true); // Trigger win condition
        }
    }

    // Method to handle the game over conditions
    private void GameOver(bool won)
    {
        gameEnded = true; // Lock the game state
        if (won)
        {
            winText.gameObject.SetActive(true); // Show "You Win" message
        }
        else
        {
            loseText.gameObject.SetActive(true); // Show "You Lose" message
        }
    }

    // Update the timer display on the UI
    private void UpdateTimerDisplay()
    {
        timerText.text = Mathf.Ceil(timer).ToString(); // Show the time remaining
    }
}