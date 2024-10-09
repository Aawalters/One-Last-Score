using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapScript : MonoBehaviour
{
    public void PlayLevel_1()
    {
        SceneManager.LoadScene("Level1");

    }

    public void PlayLevel_2()
    {
        SceneManager.LoadScene("Level2");

    }

    public void PlayLevel_3()
    {
        SceneManager.LoadScene("Level3");

    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");

    }

    public void Map()
    {
        SceneManager.LoadScene("MapMenuScene");

    }

    // public void Shop() 
    // {
    //     SceneManager.LoadScene("Shop");
    // }
}
