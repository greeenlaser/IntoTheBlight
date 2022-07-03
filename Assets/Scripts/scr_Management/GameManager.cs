using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game version")]
    public string str_GameVersion;
    [SerializeField] private TMP_Text txt_GameVersion;

    [Header("Factions")]
    public PlayerFaction playerFaction;
    public enum PlayerFaction
    {
        unassigned,
        Scientists,
        Geifers,
        Annies,
        Verbannte,
        Raiders,
        Military,
        Verteidiger,
        Others
    }
    public int vsScientists;
    public int vsGeifers;
    public int vsAnnies;
    public int vsVerbannte;
    public int vsRaiders;
    public int vsMilitary;
    public int vsVerteidiger;
    public int vsOthers;
    public List<GameObject> gameFactions;

    [Header("Framerate")]
    [SerializeField] private TMP_Text txt_fpsValue;

    [Header("Scripts")]
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private Manager_GameSaving GameSavingScript;
    [SerializeField] private Manager_Graphics GraphicsScript;

    //public but hidden variables
    [HideInInspector] public List<GameObject> thrownGrenades;

    //private variables
    private float timer;
    private float deltaTime;

    private void Awake()
    {
        txt_GameVersion.text = str_GameVersion;

        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
        QualitySettings.vSyncCount = 0;

        //get current scene index
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        //show fps and game version in main scene
        if (sceneIndex == 0)
        {
            txt_GameVersion.transform.localPosition -= new Vector3(0, 75, 0);
            txt_fpsValue.transform.localPosition -= new Vector3(0, 125, 0);
        }
        //show debug menu in game scene
        else if (sceneIndex == 1)
        {
            PauseMenuScript.PauseGame();
            UIReuseScript.LoadUIManager();
            ConsoleScript.LoadConsole();

            Manager_Console ConsoleScipt = GetComponent<Manager_Console>();
            ConsoleScipt.CreateNewConsoleLine("---GAME VERSION: " + GetComponent<GameManager>().str_GameVersion + "---", "CONSOLE_INFO_MESSAGE");
            ConsoleScipt.CreateNewConsoleLine("", "CONSOLE_INFO_MESSAGE");
            ConsoleScipt.CreateNewConsoleLine("---Type help to list all game commands---", "CONSOLE_INFO_MESSAGE");

            GetComponent<Manager_Console>().Command_ToggleDebugMenu();

            PlayerMovementScript.LoadPlayer();

            GameSavingScript.LoadSaveData();
            GraphicsScript.LoadData();
        }
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float msec = Mathf.FloorToInt(deltaTime * 1000.0f);
        float fps = Mathf.FloorToInt(1.0f / deltaTime);

        timer += Time.unscaledDeltaTime;
        if (timer > 0.1f)
        {
            txt_fpsValue.text = fps + " (" + msec + ")";
            timer = 0;
        }

        if (Input.GetKeyDown(KeyCode.F12))
        {
            Screenshot();
        }
    }

    //press F12 for screenshot
    private void Screenshot()
    {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff\Screenshots";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var screenshotName = "Screenshot_" + System.DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss") + ".png";
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, screenshotName), 4);
    }
}