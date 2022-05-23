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

    [Header("Message send test")]
    [SerializeField] private GameObject logo;
    [SerializeField] private GameObject par_Managers;

    [Header("Tip list")]
    public List<string> tips;

    //public but hidden variables
    [HideInInspector] public List<GameObject> thrownGrenades;

    //private variables
    private float timer;
    private float deltaTime;

    private void Awake()
    {
        txt_GameVersion.text = str_GameVersion;

        Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
        QualitySettings.vSyncCount = 0;

        //get current scene index
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (str_GameVersion.Contains("indev"))
        {
            //show fps and game version in main scene
            if (sceneIndex == 0)
            {
                txt_GameVersion.transform.localPosition -= new Vector3(0, 75, 0);
                txt_fpsValue.transform.localPosition -= new Vector3(0, 125, 0);
            }
            //show debug menu in game scene
            else if (sceneIndex == 1)
            {
                Manager_Console ConsoleScipt = par_Managers.GetComponent<Manager_Console>();
                ConsoleScipt.CreateNewConsoleLine("---GAME VERSION: " + par_Managers.GetComponent<GameManager>().str_GameVersion + "---");
                ConsoleScipt.CreateNewConsoleLine("");
                ConsoleScipt.CreateNewConsoleLine("---Type help to list all game commands---");

                par_Managers.GetComponent<Manager_Console>().Command_ToggleDebugMenu();
            }
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

        if (Input.GetKeyDown(KeyCode.P)
            && !par_Managers.GetComponent<UI_PauseMenu>().isGamePaused
            && !par_Managers.GetComponent<Manager_Console>().consoleOpen)
        {
            string messageTime = par_Managers.GetComponent<Manager_WorldClock>().time;
            string senderName = "System";
            string messageContent = "this shit is crazy, how are they even surviving after that crazy rad-storm, a regular human wouldve died in 5 seconds but it looks like this rad storm only made them stronger! ive never seen anythng like that...";
            par_Managers.GetComponent<UI_TabletMessages>().SendMessage(logo, messageTime, senderName, messageContent);
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