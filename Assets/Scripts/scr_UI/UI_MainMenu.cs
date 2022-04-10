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
    [SerializeField] private GameObject img_loadingLogo;
    [SerializeField] private GameObject par_LoadingScreen;

    //private variables
    private bool startedSceneSwitch;
    private float time;

    private void Awake()
    {
        Time.timeScale = 1;
        ReturnToMainMenu();
    }

    private void Update()
    {
        if (startedSceneSwitch)
        {
            img_loadingLogo.transform.eulerAngles -= new Vector3(0, 0, 100) * Time.deltaTime;

            time += Time.deltaTime;
            if (time > 0.2f)
            {
                SceneManager.LoadScene(1);
            }
        }
    }

    public void StartGame()
    {
        par_LoadingScreen.SetActive(true);
        startedSceneSwitch = true;
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