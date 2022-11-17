using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void NewGame(int character)
    {
        if (character == 0)
        {
            GameData.chosencharacter = character;
        }
        else
        {
            GameData.chosencharacter = character;
        }
        StartGame();
    }
}
