using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UI_MainMenu : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject par_MainMenuContent;
    [SerializeField] private GameObject par_GameSaves;
    [SerializeField] private GameObject par_Credits;
    [SerializeField] private GameObject img_loadingLogo;
    [SerializeField] private GameObject par_LoadingScreen;

    [Header("Save list")]
    [SerializeField] private GameObject saveContent;
    [SerializeField] private TMP_Text txt_SaveTitle;
    [SerializeField] private TMP_Text txt_SaveDate;
    [SerializeField] private TMP_Text txt_LocationName;
    [SerializeField] private Button btn_SaveButtonTemplate;
    [SerializeField] private Button btn_LoadGame; 

    //private variables
    private bool startedSceneSwitch;
    private float time;
    private string gameSavePath;
    private readonly List<Button> saveButtons = new List<Button>();

    private void Awake()
    {
        Time.timeScale = 1;
        ReturnToMainMenu();

        //create game folder directory
        string gamePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff";
        if (!Directory.Exists(gamePath))
        {
            Directory.CreateDirectory(gamePath);
        }
        //create game save directory
        gameSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff\GameSaves";
        if (!Directory.Exists(gameSavePath))
        {
            Directory.CreateDirectory(gameSavePath);
        }
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
        string loadFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff" + @"\LoadFile.txt";

        //using a text editor to write text to the game save file in the saved file path
        using StreamWriter loadFile = File.CreateText(loadFilePath);

        loadFile.WriteLine("true");

        par_LoadingScreen.SetActive(true);
        startedSceneSwitch = true;
    }

    public void ShowGameSaves()
    {
        par_MainMenuContent.SetActive(false);
        par_GameSaves.SetActive(true);

        ClearSaveData();
        RebuildSaveList();
    }

    public void ShowCredits()
    {
        par_MainMenuContent.SetActive(false);
        par_Credits.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        par_MainMenuContent.SetActive(true);
        par_GameSaves.SetActive(false);
        par_Credits.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void RebuildSaveList()
    {
        btn_LoadGame.interactable = false;

        string[] files = Directory.GetFiles(gameSavePath);

        if (files.Length > 0)
        {
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                DateTime creationDate = File.GetCreationTime(file);

                Button btn_New = Instantiate(btn_SaveButtonTemplate);
                btn_New.transform.SetParent(saveContent.transform, false);

                btn_New.GetComponentInChildren<TMP_Text>().text = fileName.Replace(".txt", "");

                btn_New.onClick.AddListener(delegate { ShowSaveData(fileName, creationDate.ToString()); });

                saveButtons.Add(btn_New);
            }
        }

        txt_SaveTitle.text = "";
        txt_SaveDate.text = "Created on:";
        txt_LocationName.text = "Location:";
    }
    public void ShowSaveData(string fileName, string creationDate)
    {
        txt_SaveTitle.text = fileName.Replace(".txt", "");
        txt_SaveDate.text = "Created on: " + creationDate.ToString();
        //txt_LocationName.text = "Location:";

        btn_LoadGame.interactable = true;
        btn_LoadGame.onClick.RemoveAllListeners();
        btn_LoadGame.onClick.AddListener(delegate { LoadGame(fileName); });
    }
    public void LoadGame(string fileName)
    {
        string loadFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff" + @"\LoadFile.txt";

        if (File.Exists(gameSavePath + @"\" + fileName))
        {
            //using a text editor to write text to the game save file in the saved file path
            using StreamWriter loadFile = File.CreateText(loadFilePath);

            loadFile.WriteLine("false");
            loadFile.WriteLine(fileName.Replace(".txt", ""));

            par_LoadingScreen.SetActive(true);
            startedSceneSwitch = true;
        }
    }
    private void ClearSaveData()
    {
        foreach (Button btn in saveButtons)
        {
            Destroy(btn.gameObject);
        }
        saveButtons.Clear();
    }
}