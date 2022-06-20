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
    [SerializeField] private GameObject par_Graphics;
    [SerializeField] private GameObject par_Credits;
    [SerializeField] private GameObject img_loadingLogo;
    [SerializeField] private GameObject par_LoadingScreen;

    [Header("Save list")]
    [SerializeField] private GameObject saveContent;
    [SerializeField] private TMP_Text txt_SaveTitle;
    [SerializeField] private TMP_Text txt_SaveDate;
    [SerializeField] private Button btn_SaveButtonTemplate;
    [SerializeField] private Button btn_LoadGame; 

    //private variables
    private bool startedSceneSwitch;
    private float time;
    private string path;
    private List<Button> saveButtons = new List<Button>();

    private void Awake()
    {
        Time.timeScale = 1;
        ReturnToMainMenu();

        path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff\GameSaves";
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

    public void ShowGameSaves()
    {
        par_MainMenuContent.SetActive(false);
        par_GameSaves.SetActive(true);

        RebuildSaveList();
        ClearSaveData();
    }

    public void ShowGraphics()
    {
        par_MainMenuContent.SetActive(false);
        par_Graphics.SetActive(true);
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
        par_Graphics.SetActive(false);
        par_Credits.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void RebuildSaveList()
    {
        btn_LoadGame.gameObject.SetActive(false);

        string[] files = Directory.GetFiles(path);

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
    }
    public void ShowSaveData(string fileName, string creationDate)
    {
        txt_SaveTitle.text = fileName.Replace(".txt", "");
        txt_SaveDate.text = creationDate.ToString();

        btn_LoadGame.gameObject.SetActive(true);
        btn_LoadGame.onClick.RemoveAllListeners();
        btn_LoadGame.onClick.AddListener(delegate { LoadGame(fileName); });
    }
    public void LoadGame(string fileName)
    {
        string loadFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff" + @"\LoadFile.txt";

        if (File.Exists(path + @"\" + fileName))
        {
            //using a text editor to write text to the game save file in the saved file path
            using StreamWriter loadFile = File.CreateText(loadFilePath);

            loadFile.WriteLine("Load file for Lights Off Version " + gameObject.GetComponent<GameManager>().str_GameVersion);
            loadFile.WriteLine("Read more info about the game from https://greeenlaser.itch.io/lightsoff");
            loadFile.WriteLine("Download game versions from https://drive.google.com/drive/folders/12kvUT6EEndku0nDvZVrVd4QRPt50QV7g?usp=sharing");
            loadFile.WriteLine("");
            loadFile.WriteLine("WARNING: Invalid values will break the game - edit at your own risk!");

            loadFile.WriteLine("");
            loadFile.WriteLine(fileName.Replace(".txt", ""));

            SceneManager.LoadScene(1);
        }
    }
    private void ClearSaveData()
    {
        for (int i = 0; i < saveButtons.Count; i++)
        {
            Destroy(saveButtons[i]);
        }
        saveButtons.Clear();
    }
}