using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_MainMenu : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject par_MainMenuContent;
    [SerializeField] private GameObject par_KeyCommands;
    [SerializeField] private GameObject par_TeamMembers;

    private void Start()
    {
        ReturnToMainMenu();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowKeyCommands()
    {
        par_MainMenuContent.SetActive(false);
        par_KeyCommands.SetActive(true);
    }

    public void ShowTeamMembers()
    {
        par_MainMenuContent.SetActive(false);
        par_TeamMembers.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        par_MainMenuContent.SetActive(true);
        par_KeyCommands.SetActive(false);
        par_TeamMembers.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}