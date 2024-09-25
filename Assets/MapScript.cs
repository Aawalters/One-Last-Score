using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapScript : MonoBehaviour
{
    public void PlayLevel_1()
    {
        SceneManager.LoadScene("DiegoTestingScene");

    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");

    }
}
