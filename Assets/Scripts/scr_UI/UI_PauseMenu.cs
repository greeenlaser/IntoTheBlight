using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_PauseMenu : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_PlayerSFX;
    [SerializeField] private Button btn_Save;
    [SerializeField] private Button btn_Load;

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
    [HideInInspector] public bool callPMOpenOnce;
    [HideInInspector] public bool callPMCloseOnce;

    //private variables
    private bool calledOnce;
    private readonly List<AudioSource> pausedSFX = new();

    private void Awake()
    {
        //game is paused by default when main game scene is loaded
        PauseGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) 
            && canPauseGame
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            //pauses game and opens pause menu UI
            if (!isGamePaused && !isUIOpen)
            {
                PauseGameAndOpenUI();
            }

            //closes console but keeps game paused
            else if (isGamePaused
                && par_Managers.GetComponent<Manager_Console>().consoleOpen
                && (isInventoryOpen || isTalkingToAI))
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
                && isUIOpen)
            {
                UnpauseGame();
            }

            //opens pause menu UI if player inv or container UI is open
            else if (isGamePaused
                && !isUIOpen
                && (isInventoryOpen
                || isTalkingToAI))
            {
                OpenUI();
            }

            //closes pause menu but keeps game paused
            else if (isGamePaused
                && isUIOpen
                && (isInventoryOpen
                || isTalkingToAI))
            {
                CloseUI();
            }
        }

        if (isUIOpen && isInventoryOpen && !calledOnce)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_ReturnToGame.onClick.AddListener(CloseUI);
            //Debug.Log("Added return button function to only close pause menu UI.");
            calledOnce = true;
        }

        else if (isUIOpen && !isInventoryOpen && !calledOnce)
        {
            par_Managers.GetComponent<Manager_UIReuse>().btn_ReturnToGame.onClick.AddListener(UnpauseGame);
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

            par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenu.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenuContent.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().par_KeyCommandsContent.SetActive(false);

            var savingScript = par_Managers.GetComponent<Manager_GameSaving>();

            //restarts current scene
            //if no save file directory or save file was found
            if (!Directory.Exists(savingScript.path)
                || (Directory.Exists(savingScript.path)
                && !File.Exists(savingScript.path + @"\Save0001.txt")))
            {
                btn_Load.onClick.RemoveAllListeners();
                btn_Load.interactable = false;
            }
            //loads game data if a save file was found
            else if (File.Exists(savingScript.path + @"\Save0001.txt"))
            {
                btn_Load.onClick.RemoveAllListeners();
                btn_Load.onClick.AddListener(Load);
                btn_Load.interactable = true;
            }

            btn_Save.onClick.RemoveAllListeners();
            btn_Save.onClick.AddListener(Save);

            par_Managers.GetComponent<Manager_UIReuse>().btn_ReturnToPauseMenu.gameObject.SetActive(false);

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

        par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenu.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenuContent.SetActive(false);

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

            par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenu.SetActive(false);
            par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenuContent.SetActive(false);

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
        par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenu.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenuContent.SetActive(true);

        calledOnce = false;
        isUIOpen = true;

        var savingScript = par_Managers.GetComponent<Manager_GameSaving>();

        //restarts current scene
        //if no save file directory or save file was found
        if (!Directory.Exists(savingScript.path)
            || (Directory.Exists(savingScript.path)
            && !File.Exists(savingScript.path + @"\Save0001.txt")))
        {
            btn_Load.onClick.RemoveAllListeners();
            btn_Load.interactable = false;
        }
        //loads game data if a save file was found
        else if (File.Exists(savingScript.path + @"\Save0001.txt"))
        {
            btn_Load.onClick.RemoveAllListeners();
            btn_Load.onClick.AddListener(Load);
            btn_Load.interactable = true;
        }

        btn_Save.onClick.RemoveAllListeners();
        btn_Save.onClick.AddListener(Save);

        //Debug.Log("Opened pause menu UI!");
    }

    //only closes the UI but keeps the game paused
    public void CloseUI()
    {
        par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenu.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenuContent.SetActive(false);

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
            savingScript.CreateSaveFile();
        }
    }
    public void Load()
    {
        var savingScript = par_Managers.GetComponent<Manager_GameSaving>();

        //loads game data if a save file was found
        if (File.Exists(savingScript.path + @"\Save0001.txt"))
        {
            if (!savingScript.isLoading)
            {
                savingScript.isLoading = true;
                savingScript.OpenLoadingMenuUI();
            }

            //Debug.Log("Found save file! Loading data...");
            savingScript.LoadGameData();
        }

        thePlayer.GetComponent<Player_Health>().txt_PlayerDied.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().HideExoskeletonUI();
    }

    public void OpenKeyCommands()
    {
        par_Managers.GetComponent<Manager_UIReuse>().par_KeyCommandsContent.SetActive(true);
        par_Managers.GetComponent<Manager_UIReuse>().btn_ReturnToPauseMenu.gameObject.SetActive(true);
    }
    public void CloseKeyAndConsoleCommands()
    {
        par_Managers.GetComponent<Manager_UIReuse>().par_KeyCommandsContent.SetActive(false);

        par_Managers.GetComponent<Manager_UIReuse>().btn_ReturnToPauseMenu.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().btn_ReturnToPauseMenu.gameObject.SetActive(false);
    }

    public void BackToPauseMenu()
    {
        par_Managers.GetComponent<Manager_UIReuse>().par_PauseMenuContent.SetActive(true);
    }

    public void BackToMainMenu()
    {
        //Debug.Log("Went to main menu!");
        SceneManager.LoadScene(0);
    }
}