using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI ScoreText;
    public void Setup(int score)
    {
        gameObject.SetActive(true);
        ScoreText.text = "Final Payout: " + score.ToString();
    }
    public void Restart(int score)
    {
        SceneManager.LoadScene("DiegoTestingScene");

    }
    public void Menu(int score)
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
