using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UI_PauseMenu : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_PlayerSFX;
    [SerializeField] private Button btn_Save;
    [SerializeField] private Button btn_Load;

    [Header("Save list")]
    [SerializeField] private GameObject par_GameSaves;
    [SerializeField] private GameObject saveContent;
    [SerializeField] private TMP_Text txt_SaveTitle;
    [SerializeField] private TMP_Text txt_SaveDate;
    [SerializeField] private TMP_Text txt_LocationName;
    [SerializeField] private Button btn_SaveButtonTemplate;
    [SerializeField] private Button btn_LoadGame;

    [Header("Scripts")]
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Player_Camera PlayerCameraScript;
    [SerializeField] private GameObject par_Managers;

    //hidden but public variables
    [HideInInspector] public bool canPauseGame;
    [HideInInspector] public bool isGamePaused;
    [HideInInspector] public bool isUIOpen;
    [HideInInspector] public bool isInventoryOpen;
    [HideInInspector] public bool isWaitableUIOpen;
    [HideInInspector] public bool isTalkingToAI;
    [HideInInspector] public bool isComputerOpen;
    [HideInInspector] public bool callPMOpenOnce;
    [HideInInspector] public bool callPMCloseOnce;

    //private variables
    private bool calledOnce;
    private readonly List<AudioSource> pausedSFX = new List<AudioSource>();
    private string path;
    private readonly List<Button> saveButtons = new List<Button>();
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) 
            && canPauseGame
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            //pauses game and opens pause menu UI
            if (!isGamePaused 
                && !isUIOpen)
            {
                PauseGameAndOpenUI();
            }

            //closes console but keeps game paused
            else if (isGamePaused
                     && par_Managers.GetComponent<Manager_Console>().consoleOpen
                     && (isInventoryOpen 
                     || isTalkingToAI))
            {
                PauseGameAndCloseUIAndResetBools();
            }

            //unpauses game
            else if (isGamePaused
                     && !par_Managers.GetComponent<Manager_Console>().consoleOpen
                     && !thePlayer.GetComponent<Inv_Player>().isPlayerInventoryOpen
                     && !thePlayer.GetComponent<Inv_Player>().isPlayerAndContainerOpen
                     && !isWaitableUIOpen
                     && !isInventoryOpen
                     && !isTalkingToAI
                     && !isComputerOpen
                     && isUIOpen)
            {
                UnpauseGame();
            }

            //opens pause menu UI but doesn't pause the game again
            else if (isGamePaused
                     && !isUIOpen
                     && (isInventoryOpen
                     || isTalkingToAI
                     || isComputerOpen))
            {
                OpenUI();
            }

            //closes pause menu but keeps game paused
            else if (isGamePaused
                     && isUIOpen
                     && (isInventoryOpen
                     || isTalkingToAI
                     || isComputerOpen))
            {
                CloseUI();
            }
        }

        if (isUIOpen && isInventoryOpen && !calledOnce)
        {
            UIReuseScript.btn_ReturnToGame.onClick.AddListener(CloseUI);
            //Debug.Log("Added return button function to only close pause menu UI.");
            calledOnce = true;
        }

        else if (isUIOpen && !isInventoryOpen && !calledOnce)
        {
            UIReuseScript.btn_ReturnToGame.onClick.AddListener(UnpauseGame);
            //Debug.Log("Added return button function to unpause the game.");
            calledOnce = true;
        }
    }

    //pauses the game but does not open the UI
    public void PauseGame()
    {
        if (!callPMOpenOnce)
        {
            Time.timeScale = 0;

            PlayerMovementScript.canMove = false;
            PlayerMovementScript.canCrouch = false;
            PlayerCameraScript.isCamEnabled = false;

            PauseSFX();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            calledOnce = false;
            isGamePaused = true;
            callPMCloseOnce = false;

            //Debug.Log("Paused game!");
            callPMOpenOnce = true;
        }
    }

    //pauses the game and opens the UI
    public void PauseGameAndOpenUI()
    {
        if (!callPMOpenOnce)
        {
            Time.timeScale = 0;

            PlayerMovementScript.canMove = false;
            PlayerMovementScript.canCrouch = false;
            PlayerCameraScript.isCamEnabled = false;

            UIReuseScript.par_PauseMenu.SetActive(true);
            UIReuseScript.par_PauseMenuContent.SetActive(true);

            par_GameSaves.SetActive(false);
            UIReuseScript.par_KeyCommandsContent.SetActive(false);
            UIReuseScript.par_GraphicsContent.SetActive(false);

            UIReuseScript.btn_ReturnToPauseMenu.gameObject.SetActive(false);

            var savingScript = par_Managers.GetComponent<Manager_GameSaving>();

            par_GameSaves.SetActive(false);
            btn_Load.onClick.RemoveAllListeners();
            btn_Load.onClick.AddListener(RebuildSaveList);

            btn_Save.onClick.RemoveAllListeners();
            btn_Save.onClick.AddListener(Save);

            UIReuseScript.btn_ReturnToPauseMenu.gameObject.SetActive(false);

            PauseSFX();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            calledOnce = false;
            isGamePaused = true;
            isUIOpen = true;
            callPMCloseOnce = false;

            //Debug.Log("Paused game and opened pause menu UI!");
            callPMOpenOnce = true;
        }
    }

    //keeps the game paused, closes UI and fixes bools to be able to re-open pause menu again
    public void PauseGameAndCloseUIAndResetBools()
    {
        Time.timeScale = 0;

        PlayerMovementScript.canMove = false;
        PlayerMovementScript.canCrouch = false;
        PlayerCameraScript.isCamEnabled = false;

        UIReuseScript.btn_ReturnToPauseMenu.gameObject.SetActive(false);

        UIReuseScript.par_PauseMenu.SetActive(false);
        UIReuseScript.par_PauseMenuContent.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        calledOnce = false;
        canPauseGame = true;
        isUIOpen = false;
        callPMOpenOnce = false;
        callPMCloseOnce = true;
        isGamePaused = true;

        //Debug.Log("Paused game and reset bools!");
    }

    //unpauses the game, closes the pause menu UI
    public void UnpauseGame()
    {
        if (!callPMCloseOnce)
        {
            Time.timeScale = 1;

            PlayerMovementScript.canMove = true;
            PlayerMovementScript.canCrouch = true;
            PlayerCameraScript.isCamEnabled = true;

            UIReuseScript.btn_ReturnToPauseMenu.gameObject.SetActive(false);

            UIReuseScript.par_PauseMenu.SetActive(false);
            UIReuseScript.par_PauseMenuContent.SetActive(false);

            //unpauses all paused player SFX audiosources
            for (int i = 0; i < pausedSFX.Count; i++)
            {
                pausedSFX[i].Play();
                //Debug.Log("Game was unpaused! Removed " + pausedSFX[i].transform.parent.name + " from pausedSFX and playing it!");
                pausedSFX.Remove(pausedSFX[i]);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            calledOnce = false;
            canPauseGame = true;
            isGamePaused = false;
            isInventoryOpen = false;
            isUIOpen = false;
            callPMOpenOnce = false;

            //Debug.Log("Unpaused game and closed pause menu UI!");
            callPMCloseOnce = true;
        }
    }

    //doesnt pause the game but allows free cursor movement
    public void LockCamera()
    {
        PlayerCameraScript.isCamEnabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void UnlockCamera()
    {
        PlayerCameraScript.isCamEnabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void PauseSFX()
    {
        //pauses all playing player SFX audiosources
        foreach (Transform child in par_PlayerSFX.transform)
        {
            //all child SFX
            if (child.gameObject.GetComponent<AudioSource>() != null)
            {
                AudioSource sfx = child.gameObject.GetComponent<AudioSource>();
                if (sfx.isPlaying && !pausedSFX.Contains(sfx))
                {
                    pausedSFX.Add(sfx);
                    //Debug.Log("Paused " + child.name + " and added to pausedSFX list. Waiting for game to un-pause to continue playing.");
                    sfx.Pause();
                }
            }
            //all SFX of childrens children
            else if (child.gameObject.GetComponent<AudioSource>() == null)
            {
                foreach (Transform child2 in child)
                {
                    if (child2.gameObject.GetComponent<AudioSource>() != null)
                    {
                        AudioSource sfx = child2.gameObject.GetComponent<AudioSource>();
                        if (sfx.isPlaying && !pausedSFX.Contains(sfx))
                        {
                            pausedSFX.Add(sfx);
                            //Debug.Log("Paused " + child2.name + " and added to pausedSFX list. Waiting for game to un-pause to continue playing.");
                            sfx.Pause();
                        }
                    }
                }
            }
        }
    }

    //only opens the UI
    public void OpenUI()
    {
        UIReuseScript.par_PauseMenu.SetActive(true);
        UIReuseScript.par_PauseMenuContent.SetActive(true);

        calledOnce = false;
        isUIOpen = true;

        var savingScript = par_Managers.GetComponent<Manager_GameSaving>();

        UIReuseScript.btn_ReturnToPauseMenu.gameObject.SetActive(false);

        par_GameSaves.SetActive(false);
        btn_Load.onClick.RemoveAllListeners();
        btn_Load.onClick.AddListener(RebuildSaveList);

        btn_Save.onClick.RemoveAllListeners();
        btn_Save.onClick.AddListener(Save);

        //Debug.Log("Opened pause menu UI!");
    }

    //only closes the UI but keeps the game paused
    public void CloseUI()
    {
        UIReuseScript.par_PauseMenu.SetActive(false);
        UIReuseScript.par_PauseMenuContent.SetActive(false);

        calledOnce = false;
        isUIOpen = false;
        //Debug.Log("Closed pause menu UI!");
    }

    public void Save()
    {
        var savingScript = par_Managers.GetComponent<Manager_GameSaving>();

        //game can only be saved if game isnt currently being saved
        //and if player is alive
        if (!savingScript.isSaving
            && thePlayer.GetComponent<Player_Health>().isPlayerAlive)
        {
            savingScript.CreateSaveFile("");
        }
    }

    private void RebuildSaveList()
    {
        par_GameSaves.SetActive(true);

        UIReuseScript.btn_ReturnToPauseMenu.gameObject.SetActive(true);
        UIReuseScript.btn_ReturnToPauseMenu.onClick.RemoveAllListeners();
        UIReuseScript.btn_ReturnToPauseMenu.onClick.AddListener(BackToPauseMenu);

        btn_LoadGame.interactable = false;

        path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff\GameSaves";
        string[] files = Directory.GetFiles(path);

        if (files.Length > 0)
        {
            ClearSaveData();

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

        if (File.Exists(path + @"\" + fileName))
        {
            //using a text editor to write text to the game save file in the saved file path
            using StreamWriter loadFile = File.CreateText(loadFilePath);

            loadFile.WriteLine("false");
            loadFile.WriteLine(fileName.Replace(".txt", ""));

            par_Managers.GetComponent<Manager_GameSaving>().par_LoadingMenu.SetActive(true);
            par_Managers.GetComponent<Manager_GameSaving>().img_loadingLogo.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_GameSaving>().txt_LoadingText.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_GameSaving>().btn_Continue.gameObject.SetActive(false);
            SceneManager.LoadScene(1);
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

    public void OpenKeyCommands()
    {
        UIReuseScript.par_KeyCommandsContent.SetActive(true);

        UIReuseScript.btn_ReturnToPauseMenu.gameObject.SetActive(true);
        UIReuseScript.btn_ReturnToPauseMenu.onClick.RemoveAllListeners();
        UIReuseScript.btn_ReturnToPauseMenu.onClick.AddListener(BackToPauseMenu);
    }
    public void OpenGraphics()
    {
        UIReuseScript.par_GraphicsContent.SetActive(true);

        UIReuseScript.btn_ReturnToPauseMenu.gameObject.SetActive(true);
        UIReuseScript.btn_ReturnToPauseMenu.onClick.RemoveAllListeners();
        UIReuseScript.btn_ReturnToPauseMenu.onClick.AddListener(BackToPauseMenu);
    }

    public void BackToPauseMenu()
    {
        UIReuseScript.par_PauseMenuContent.SetActive(true);

        par_GameSaves.SetActive(false);
        UIReuseScript.par_KeyCommandsContent.SetActive(false);
        UIReuseScript.par_GraphicsContent.SetActive(false);

        UIReuseScript.btn_ReturnToPauseMenu.gameObject.SetActive(false);
    }

    public void BackToMainMenu()
    {
        //Debug.Log("Went to main menu!");
        SceneManager.LoadScene(0);
    }
}