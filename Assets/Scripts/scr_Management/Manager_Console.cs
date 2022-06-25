using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Manager_Console : MonoBehaviour
{
    [Header("Player assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject cam_Player;

    [Header("Assignables")]
    public GameObject ammoTemplate; 
    [SerializeField] private GameObject par_DebugUI;
    [SerializeField] private TMP_Text txt_PlayerPos;
    [SerializeField] private TMP_Text txt_CameraAngle;
    [SerializeField] private TMP_Text txt_PlayerSpeed;
    [SerializeField] private TMP_Text txt_SelectedTargetName;
    [SerializeField] private TMP_InputField txt_ConsoleInputField;
    public List<GameObject> allCells;
    public List<GameObject> spawnables;

    [Header("Scripts")]
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;
    
    //public but hidden variables
    [HideInInspector] public bool consoleOpen;
    [HideInInspector] public bool displayUnityLogs;
    [HideInInspector] public bool toggleAIDetection;
    [HideInInspector] public int originalMaxHealth;
    [HideInInspector] public int originalMaxStamina;
    [HideInInspector] public int originalStaminaRecharge;
    [HideInInspector] public int originalMaxRadiation;
    [HideInInspector] public int originalMaxMentalState;
    [HideInInspector] public int originalSpeed;
    [HideInInspector] public int originalJumpHeight;
    [HideInInspector] public int originalMaxInvspace;
    [HideInInspector] public List<string> cellnames;
    [HideInInspector] public List<string> itemnames;
    [HideInInspector] public List<string> playeritemnames;
    [HideInInspector] public GameObject lockpickUI;

    //private variables
    private bool foundDuplicate;
    private bool foundRepairable;
    private bool displayDebugMenu;
    private bool canContinueItemRemoval;
    private bool isSelectingTarget;
    private int invSpace;
    private int currentSelectedInsertedCommand;
    private string input;
    private readonly char[] separators = new char[] { ' ' };
    private GameObject selectedItem;
    private GameObject selectedGun;
    private GameObject target;
    private GameObject correctAmmo;
    private GameObject displayableItem;
    private GameObject duplicate;
    private readonly List<string> separatedWords = new List<string>();
    private readonly List<GameObject> createdTexts = new List<GameObject>();
    private readonly List<string> insertedCommands = new List<string>();

    //spawn and remove multiple items
    private GameObject spawnableItem;
    private GameObject removableItem;
    private int insertedValue;
    private int remainder;
    private float timer;
    private string playerPos;
    private string cameraAngle;
    private string playerSpeed;
    private Vector3 lastPos;
    private readonly List<GameObject> removables = new List<GameObject>();

    //unity log variables
    private bool startedWait;
    private string lastOutput;
    private string output;
    private string debugFilePath;
    private string debugNewFilePath;

    private void Awake()
    {
        debugFilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff\DebugFiles";
        if (!Directory.Exists(debugFilePath))
        {
            Directory.CreateDirectory(debugFilePath);
        }

        string date = DateTime.Now.ToString();
        string replaceSlash = date.Replace('/', '_');
        string replaceColon = replaceSlash.Replace(':', '_');
        string replaceEmpty = replaceColon.Replace(' ', '_');
        debugNewFilePath = debugFilePath + @"\DebugFile_" + replaceEmpty + ".txt";

        //using a text editor to write new text to new debug file in the debug file path
        using StreamWriter debugFile = File.CreateText(debugNewFilePath);

        debugFile.WriteLine("Debug information file for Lights Off Version " + par_Managers.GetComponent<GameManager>().str_GameVersion);
        debugFile.WriteLine("Read more info about the game from https://greeenlaser.itch.io/lightsoff");
        debugFile.WriteLine("Download game versions from https://drive.google.com/drive/folders/12kvUT6EEndku0nDvZVrVd4QRPt50QV7g?usp=sharing");
        debugFile.WriteLine("");

        //add user cpu
        string processorType = SystemInfo.processorType;
        int processorThreadCount = SystemInfo.processorCount;
        int processorFrequency = SystemInfo.processorFrequency;

        debugFile.WriteLine("CPU: " + processorType + "with " + processorThreadCount + " threads at " + processorFrequency + "mhz");
        //add user gpu
        string gpuName = SystemInfo.graphicsDeviceName;
        int gpuMemory = SystemInfo.graphicsMemorySize/1000;

        debugFile.WriteLine("GPU: " + gpuName + " with " + gpuMemory + "gb memory");
        //add user ram
        int ramSize = SystemInfo.systemMemorySize/1000;

        debugFile.WriteLine("RAM: " + ramSize + "gb");
        //add user OS
        string osVersion = SystemInfo.operatingSystem;

        debugFile.WriteLine("OS: " + osVersion);

        debugFile.WriteLine("");

        par_DebugUI.transform.localPosition = new Vector3(0, 300, 0);
        displayUnityLogs = true;
        toggleAIDetection = true;

        originalMaxHealth = Mathf.FloorToInt(thePlayer.GetComponent<Player_Health>().maxHealth);
        originalMaxStamina = Mathf.FloorToInt(thePlayer.GetComponent<Player_Movement>().maxStamina);
        originalStaminaRecharge = Mathf.FloorToInt(thePlayer.GetComponent<Player_Movement>().staminaRecharge);
        originalMaxMentalState = Mathf.FloorToInt(thePlayer.GetComponent<Player_Health>().maxMentalState);
        originalMaxRadiation = Mathf.FloorToInt(thePlayer.GetComponent<Player_Health>().maxRadiation);
        originalSpeed = Mathf.FloorToInt(thePlayer.GetComponent<Player_Movement>().speedIncrease);
        originalJumpHeight = Mathf.FloorToInt(thePlayer.GetComponent<Player_Movement>().jumpHeight);
        originalMaxInvspace = Mathf.FloorToInt(thePlayer.GetComponent<Inv_Player>().maxInvSpace);
    }

    private void Start()
    {
        //get all game cells and add to list
        foreach (GameObject cell in allCells)
        {
            string addableCellName = cell.GetComponent<Manager_CurrentCell>().str_CellName;
            cellnames.Add(addableCellName);
        }
        //get all template item names and add to list
        foreach (GameObject item in spawnables)
        {
            string addableItemName = item.GetComponent<Env_Item>().str_ItemName;
            itemnames.Add(addableItemName);
        }
        //get all player item names and add to list
        foreach (GameObject item in thePlayer.GetComponent<Inv_Player>().inventory)
        {
            string addableItemName = item.GetComponent<Env_Item>().str_ItemName;
            playeritemnames.Add(addableItemName);
        }

        txt_SelectedTargetName.text = "";

        //start recieving unity logs
        Application.logMessageReceived += HandleLog;
    }

    private void Update()
    {
        //open/close console when pageup key was pressed
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            ToggleConsole();
        }

        //simple way to choose newer and older inserted commands with arrow keys
        if (consoleOpen && insertedCommands.Count > 0)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentSelectedInsertedCommand--;
                if (currentSelectedInsertedCommand < 0)
                {
                    currentSelectedInsertedCommand = insertedCommands.Count -1;
                }
                txt_ConsoleInputField.text = insertedCommands[currentSelectedInsertedCommand];
                txt_ConsoleInputField.MoveToEndOfLine(false, false);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentSelectedInsertedCommand++;
                if (currentSelectedInsertedCommand == insertedCommands.Count)
                {
                    currentSelectedInsertedCommand = 0;
                }
                txt_ConsoleInputField.text = insertedCommands[currentSelectedInsertedCommand];
                txt_ConsoleInputField.MoveToEndOfLine(false, false);
            }
        }

        //updates player camera and position values on debug screen if game isnt paused and debug menu is being displayed
        if (!par_Managers.GetComponent<UI_PauseMenu>().isGamePaused && displayDebugMenu)
        {
            playerPos = new Vector3(
                Mathf.FloorToInt(thePlayer.transform.position.x),
                Mathf.FloorToInt(thePlayer.transform.position.y),
                Mathf.FloorToInt(thePlayer.transform.position.z)).ToString();

            cameraAngle = new Vector3(
                Mathf.FloorToInt(cam_Player.transform.eulerAngles.x),
                Mathf.FloorToInt(cam_Player.transform.eulerAngles.y),
                Mathf.FloorToInt(cam_Player.transform.eulerAngles.z)).ToString();

            playerSpeed = Mathf.FloorToInt((thePlayer.transform.position - lastPos).magnitude / Time.deltaTime).ToString();
            lastPos = thePlayer.transform.position;

            timer += Time.deltaTime;
            if (timer > 0.05f)
            {
                UpdateValues();
                timer = 0;
            }
        }

        if (isSelectingTarget)
        {
            par_Managers.GetComponent<UI_PauseMenu>().canPauseGame = false;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //shoots out a ray from the camera towards  the position of the cursor
                Ray ray = cam_Player.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                LayerMask ignoredLayer = LayerMask.NameToLayer("Player");
                ignoredLayer = ~ignoredLayer;

                //checks if the raycast hit anything
                if (Physics.Raycast(ray, 
                                    out RaycastHit hit, 
                                    100,
                                    ignoredLayer,
                                    QueryTriggerInteraction.Ignore))
                {
                    //if selected gameobject isnt empty
                    if (hit.transform.gameObject != null)
                    {
                        CreateNewConsoleLine("Selected " + hit.collider.name + "!", "CONSOLE_SUCCESS_MESSAGE");

                        target = hit.transform.gameObject;
                        txt_SelectedTargetName.text = "selected target: " + target.name.ToString();

                        par_Managers.GetComponent<Manager_UIReuse>().par_Console.transform.localPosition = new Vector3(0, 0, 0);

                        par_Managers.GetComponent<Manager_UIReuse>().txt_InsertedTextSlot.ActivateInputField();

                        isSelectingTarget = false;
                    }
                }
            }

        }
        if (Input.GetKeyDown(KeyCode.Escape) && isSelectingTarget)
        {
            CreateNewConsoleLine("Cancelled target selection.", "CONSOLE_SUCCESS_MESSAGE");

            txt_SelectedTargetName.text = "";
            target = null;

            par_Managers.GetComponent<Manager_UIReuse>().par_Console.transform.localPosition = new Vector3(0, 0, 0);

            par_Managers.GetComponent<UI_PauseMenu>().canPauseGame = true;
            isSelectingTarget = false;
        }
    }

    private void UpdateValues()
    {
        txt_PlayerPos.text = playerPos;
        txt_CameraAngle.text = cameraAngle;
        txt_PlayerSpeed.text = playerSpeed;
    }

    private void ToggleConsole()
    {
        if (!consoleOpen
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            if (!par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen 
                && !par_Managers.GetComponent<UI_PauseMenu>().isUIOpen 
                && !par_Managers.GetComponent<UI_PauseMenu>().isTalkingToAI
                && !par_Managers.GetComponent<UI_PauseMenu>().isWaitableUIOpen
                && lockpickUI == null)
            {
                par_Managers.GetComponent<UI_PauseMenu>().PauseGame();
            }

            par_Managers.GetComponent<Manager_UIReuse>().par_Console.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().txt_InsertedTextSlot.ActivateInputField();

            if (target != null)
            {
                txt_SelectedTargetName.text = "selected target: " + target.name.ToString();
            }

            consoleOpen = true;
        }
        else if (consoleOpen)
        {
            if (!par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen 
                && !par_Managers.GetComponent<UI_PauseMenu>().isUIOpen 
                && !par_Managers.GetComponent<UI_PauseMenu>().isTalkingToAI
                && !par_Managers.GetComponent<UI_PauseMenu>().isComputerOpen
                && lockpickUI == null)
            {
                par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();
            }

            par_Managers.GetComponent<Manager_UIReuse>().par_Console.transform.localPosition = new Vector3(0, 0, 0);

            //resets all target-related variables except last selected target
            txt_SelectedTargetName.text = "";
            isSelectingTarget = false;

            par_Managers.GetComponent<Manager_UIReuse>().par_Console.SetActive(false);

            consoleOpen = false;
        }

        separatedWords.Clear();
    }

    //reads inserted text from input field in console UI
    //disabled while player is selecting target with selecttarget
    public void ReadStringInput(string s)
    {
        input = s;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            //check inserted text
            CheckInsertedText();
            //clear inserted text
            par_Managers.GetComponent<Manager_UIReuse>().txt_InsertedTextSlot.text = "";
            //enable input field
            par_Managers.GetComponent<Manager_UIReuse>().txt_InsertedTextSlot.ActivateInputField();
        }
    }

    //check if inserted text is a valid command
    private void CheckInsertedText()
    {
        //splits each word as its own and adds to separated words list
        foreach (string word in input.Split(separators, System.StringSplitOptions.RemoveEmptyEntries))
        {
            separatedWords.Add(word);
        }

        insertedCommands.Add(input);
        currentSelectedInsertedCommand = insertedCommands.Count - 1;
        CreateNewConsoleLine("--" + input + "--", "CONSOLE_INFO_MESSAGE");

        //if inserted text was not empty and player pressed enter
        if (separatedWords.Count >= 1)
        {
            bool isInt = int.TryParse(separatedWords[0], out _);
            if (isInt)
            {
                CreateNewConsoleLine("Error: Console command cannot start with a number!", "CONSOLE_ERROR_MESSAGE");
            }
            else if (!isInt)
            {
                //if the first word is help - when the player needs help with a specific command
                if (separatedWords[0] == "help")
                {
                    Command_Help();
                }

                //toggle debug menu
                else if (separatedWords[0] == "tdm" && separatedWords.Count == 1)
                {
                    Command_ToggleDebugMenu();
                }
                //toggle unity logs
                else if (separatedWords[0] == "tul" && separatedWords.Count == 1)
                {
                    Command_ToggleUnityLogs();
                }
                else if (separatedWords[0] == "save" && separatedWords.Count == 2
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_SaveWithName();
                }
                else if (separatedWords[0] == "load" && separatedWords.Count == 2)
                {
                    Command_LoadWithName();
                }
                else if (separatedWords[0] == "sas" && separatedWords.Count == 1)
                {
                    Command_ShowAllSaves();
                }
                else if (separatedWords[0] == "restart" && separatedWords.Count == 1
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_Restart();
                }
                //delete all saves
                else if (separatedWords[0] == "das" && separatedWords.Count == 1)
                {
                    Command_DeleteAllSaves();
                }
                //player becomes unkillable
                else if (separatedWords[0] == "tgm" && separatedWords.Count == 1
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_ToggleGodMode();
                }
                //player can fly through walls and not trigger anything
                else if (separatedWords[0] == "tnc" && separatedWords.Count == 1
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_ToggleNoclip();
                }
                //toggle ai detection
                else if (separatedWords[0] == "taid" && separatedWords.Count == 1
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_ToggleAIDetection();
                }
                else if (separatedWords[0] == "gcr" && separatedWords.Count == 1
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_GlobalCellReset();
                }
                //clear the console
                else if (separatedWords[0] == "clear" && separatedWords.Count == 1)
                {
                    Command_ClearConsole();
                }
                //quit the game
                else if (separatedWords[0] == "quit" && separatedWords.Count == 1)
                {
                    Command_Quit();
                }

                //selects a target
                else if (separatedWords[0] == "st" && separatedWords.Count == 1
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_SelectTarget();
                }
                //edits a target
                else if (separatedWords[0] == "target" && separatedWords.Count >= 2
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_EditTarget();
                }
                //shows all spawnable items
                else if (separatedWords.Count == 1 && separatedWords[0] == "sasi")
                {
                    Command_ShowAllSpawnableItems();
                }
                //shows all game factions
                else if (separatedWords[0] == "showfactions" && separatedWords.Count == 1)
                {
                    Command_ShowFactions();
                }
                //sets the reputation between two factions
                else if (separatedWords[0] == "setrep" && separatedWords.Count == 4
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_SetRep();
                }
                //teleports the player to a custom vector coordinate
                else if (separatedWords[0] == "tp" && separatedWords.Count == 4
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_TeleportToVector3Location();
                }
                //shows all the games cells
                else if (separatedWords[0] == "sac" && separatedWords.Count == 1)
                {
                    Command_ShowAllCells();
                }
                //discovers all the games cells
                else if (separatedWords[0] == "dac" && separatedWords.Count == 1)
                {
                    Command_DiscoverAllCells();
                }
                //teleports the player to one of the games cells
                else if (separatedWords[0] == "tpcell" && separatedWords.Count == 2
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_TeleportToCell();
                }

                //some commands are disabled if the player is dead
                else if (!PlayerHealthScript.isPlayerAlive)
                {
                    if (separatedWords[0] == "save" && separatedWords.Count == 2
                        || separatedWords[0] == "tgm" && separatedWords.Count == 1
                        || separatedWords[0] == "tnc" && separatedWords.Count == 1
                        || separatedWords[0] == "taid" && separatedWords.Count == 1
                        || separatedWords[0] == "gcr" && separatedWords.Count == 1
                        || separatedWords[0] == "selecttarget" && separatedWords.Count == 1
                        || separatedWords[0] == "target" && separatedWords.Count >= 2
                        || separatedWords[0] == "setrep" && separatedWords.Count == 4
                        || separatedWords[0] == "tp" && separatedWords.Count == 4
                        || separatedWords[0] == "tpcell" && separatedWords.Count == 2)
                    {
                        CreateNewConsoleLine("Error: This command has been disabled because the player is dead!", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                //get or set a player value
                else if (separatedWords[0] == "player")
                {
                    //get player current coordinates
                    if (separatedWords[1] == "currcoords" && separatedWords.Count == 2)
                    {
                        Command_GetPlayerCurrentCoordinates();
                    }
                    //shows all player stats
                    else if (separatedWords.Count == 2 && separatedWords[1] == "showstats")
                    {
                        Command_ShowPlayerStats();
                    }
                    //resets all player stats
                    else if (separatedWords.Count == 2 && separatedWords[1] == "resetstats"
                             && PlayerHealthScript.isPlayerAlive)
                    {
                        Command_ResetPlayerStats();
                    }
                    //sets a player stat
                    else if (separatedWords.Count == 4 && separatedWords[1] == "setstat"
                             && PlayerHealthScript.isPlayerAlive)
                    {
                        Command_SetPlayerStat();
                    }
                    //sets the reputation between player and faction
                    else if (separatedWords.Count == 4 && separatedWords[1] == "setrep"
                             && PlayerHealthScript.isPlayerAlive)
                    {
                        Command_SetPlayerRep();
                    }
                    //shows all items in players inventory
                    else if (separatedWords.Count == 2 && separatedWords[1] == "showallitems")
                    {
                        Command_ShowAllPlayerItems();
                    }
                    //adds an item to the players inventory
                    else if (separatedWords.Count == 4 && separatedWords[1] == "additem"
                             && PlayerHealthScript.isPlayerAlive)
                    {
                        Command_AddItem();
                    }
                    //removes an item from the players inventory
                    else if (separatedWords.Count == 4 && separatedWords[1] == "removeitem"
                             && PlayerHealthScript.isPlayerAlive)
                    {
                        Command_RemoveItem();
                    }
                    //fixes all player repairable items for free
                    else if (separatedWords.Count == 2 && separatedWords[1] == "fixallitems"
                             && PlayerHealthScript.isPlayerAlive)
                    {
                        Command_FixAllItems();
                    }

                    //some commands are disabled if the player is dead
                    else if (!PlayerHealthScript.isPlayerAlive)
                    {
                        if (separatedWords.Count == 2 && separatedWords[1] == "resetstats"
                            || separatedWords.Count == 4 && separatedWords[1] == "setstat"
                            || separatedWords.Count == 4 && separatedWords[1] == "setrep"
                            || separatedWords.Count == 4 && separatedWords[1] == "additem"
                            || separatedWords.Count == 4 && separatedWords[1] == "removeitem"
                            || separatedWords.Count == 2 && separatedWords[1] == "fixallitems")
                        {
                            insertedCommands.Add(input);
                            currentSelectedInsertedCommand = insertedCommands.Count - 1;

                            CreateNewConsoleLine("Error: This command has been disabled because the player is dead!", "CONSOLE_ERROR_MESSAGE");
                        }
                    }

                    else
                    {
                        insertedCommands.Add(input);
                        currentSelectedInsertedCommand = insertedCommands.Count - 1;
                        
                        CreateNewConsoleLine("Error: Unknown or incorrect command!", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                else
                {
                    insertedCommands.Add(input);
                    currentSelectedInsertedCommand = insertedCommands.Count - 1;

                    CreateNewConsoleLine("Error: Unknown or incorrect command!", "CONSOLE_ERROR_MESSAGE");
                }
            }
        }

        else if (separatedWords.Count == 0)
        {
            insertedCommands.Add(input);
            currentSelectedInsertedCommand = insertedCommands.Count - 1;

            CreateNewConsoleLine("Error: No command was inserted! Type help to list all commands.", "CONSOLE_ERROR_MESSAGE");
        }

        separatedWords.Clear();

        input = "";
    }

    //add a new line to the console
    public void CreateNewConsoleLine(string message, string source)
    {
        if (par_Managers != null)
        {
            //create text object
            GameObject newConsoleText = Instantiate(par_Managers.GetComponent<Manager_UIReuse>().txt_InsertedTextTemplate.gameObject);
            //add created text object to list
            createdTexts.Add(newConsoleText);

            //check if createdTexts list is longer than limit
            //and remove oldest
            if (createdTexts.Count > 200)
            {
                GameObject oldestText = createdTexts[0];
                createdTexts.Remove(oldestText);
                Destroy(oldestText);
            }

            string date = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "]";
            string msg = "[" + source + "] " + date + " - " + message;

            newConsoleText.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Content.transform, false);
            newConsoleText.GetComponent<TMP_Text>().text = date + " " + message;

            //using a text editor to write new text to new debug file in the debug file path
            using StreamWriter debugFile = File.AppendText(debugNewFilePath);

            debugFile.WriteLine(msg);
        }
    }

    private void Command_Help()
    {
        //lists all console commands
        if (separatedWords.Count == 1)
        {
            CreateNewConsoleLine("tdm - Toggles the Debug menu on and off.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("tul - Toggles the Unity logs on and off.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("save - Saves current game progress.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("save saveName - Saves current game progress with custom name.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("load - Loads latest save by creation date.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("load saveName - Loads save by name.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("sas - Shows all game saves.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("restart - Loads the newest save if saves exist or restarts the game from the beginning.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("das - deletes all the game saves - WARNING: All deleted saves are lost forever!", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("tgm - toggles godmode for player.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("tnc - toggles noclip for player.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("taid - toggles ai detection for player.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("gcr - Global cell reset.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("clear - Clears the console log.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("quit - Quits the game.", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("--------", "CONSOLE_INFO_MESSAGE");

            CreateNewConsoleLine("st - select target - Hides and disables the console UI until player selects a target.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("target ... - Special command which must have more words after it - use target showinfo to show target states and commands.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("target showinfo - Shows target states and commands.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("sasi - shows all items that can be spawned.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("showfactions - Shows all the game factions.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("setrep faction1 faction2 repValue - Changes the reputation between faction1 and faction2 to repValue.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("tp xValue yValue zValue - Teleports the player to xValue, yValue and zValue coordinates.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("sac - Shows all the games cells.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("dac - Enables all the games cells.", "CONSOLE_INFO_MESSAGE");
            CreateNewConsoleLine("tpcell cellName - Teleports the player to cell cellName.", "CONSOLE_INFO_MESSAGE");
        }

        //displays specifics about each command
        else if (separatedWords.Count == 2)
        {
            //get which command player needed help with
            string helpCommand = separatedWords[1];

            //if player wants to know about player-related commands
            if (helpCommand == "player")
            {
                CreateNewConsoleLine("Gets or sets a value of the player:", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player currcoords - gets the current location of the player coordinates.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player showstats - shows all player stats.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player resetstats - resets all player stats to their original values.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player setstat statName statValue - sets statName to statValue.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player setrep faction repValue - sets the reputation between player and faction to factionValue", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player showallitems - shows all player items.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player additem itemName count - adds count of itemName to players inventory.", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player removeitem itemName count - removes count of itemName from players inventory - ITEM WILL BE DELETED!", "CONSOLE_INFO_MESSAGE");
                CreateNewConsoleLine("player fixallitems - fixes all repairable items for free.", "CONSOLE_INFO_MESSAGE");
            }
        }

        //--------

        else
        {
            CreateNewConsoleLine("Error: Unknown or incorrect command!", "CONSOLE_ERROR_MESSAGE");
        }
    }
    //toggles the debug menu on and off
    public void Command_ToggleDebugMenu()
    {
        if (!displayDebugMenu)
        {
            par_DebugUI.transform.localPosition = new Vector3(0, 0, 0);
            CreateNewConsoleLine("Showing Debug menu.", "CONSOLE_SUCCESS_MESSAGE");
            displayDebugMenu = true;
        }
        else if (displayDebugMenu)
        {
            par_DebugUI.transform.localPosition = new Vector3(0, 300, 0);
            CreateNewConsoleLine("No longer showing Debug menu.", "CONSOLE_SUCCESS_MESSAGE");
            displayDebugMenu = false;
        }
    }
    //saves the current game progress with a custom name
    private void Command_SaveWithName()
    {
        if (!par_Managers.GetComponent<Manager_GameSaving>().isSaving)
        {
            //save the potential save name
            string saveName = separatedWords[1];
            bool foundBadSymbol = false;

            //loop through all characters in saveName
            foreach (char c in saveName)
            {
                //if character is illegal
                if (!Char.IsLetterOrDigit(c))
                {
                    foundBadSymbol = true;
                    break;
                }
            }
            //if no illegal characters were found
            if (!foundBadSymbol)
            {
                par_Managers.GetComponent<Manager_GameSaving>().CreateSaveFile(saveName);
            }
            else
            {
                CreateNewConsoleLine("Error: Save name must only contain letters and digits!", "CONSOLE_ERROR_MESSAGE");
            }
        }
    }
    //loads a save with a name
    private void Command_LoadWithName()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff\GameSaves";
        if (File.Exists(path + @"\" + separatedWords[1] + ".txt"))
        {
            par_Managers.GetComponent<Manager_GameSaving>().GetLoadFile(separatedWords[1]);
        }
        else
        {
            CreateNewConsoleLine("Error: Save file " + separatedWords[1] + " does not exist!", "CONSOLE_ERROR_MESSAGE");
        }
    }
    //show all game saves
    private void Command_ShowAllSaves()
    {
        //default game saves path
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff\GameSaves";

        //if a directory was found at the path
        if (Directory.Exists(path))
        {
            //save all save files to 
            string[] saves = Directory.GetFiles(path);

            //if any saves exist at path
            if (saves.Length > 0)
            {
                //display all save names without .txt
                foreach (string save in saves)
                {
                    if (save.Contains(".txt"))
                    {
                        string saveName = Path.GetFileName(save);
                        CreateNewConsoleLine(saveName.Replace(".txt", ""), "CONSOLE_INFO_MESSAGE");
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Invalid save file extention type found at " + path + "!", "CONSOLE_ERROR_MESSAGE");
                    }
                }
            }
            else
            {
                CreateNewConsoleLine("Error: File name is invalid or save folder at path " + path + " is empty!", "CONSOLE_ERROR_MESSAGE");
            }
        }
        else
        {
            CreateNewConsoleLine("Error: Cannot find game saves folder!", "CONSOLE_ERROR_MESSAGE");
        }
    }
    //loads newest save if save was found, otherwise restarts scene
    private void Command_Restart()
    {
        par_Managers.GetComponent<Manager_GameSaving>().GetLoadFile("");
    }
    //deletes all saves
    private void Command_DeleteAllSaves()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff";
        DirectoryInfo di = new DirectoryInfo(path);

        //deletes all files and folders
        //if the folder exists and there are any files in the folder
        if (Directory.Exists(path))
        {
            if (Directory.GetFiles(path).Length > 0)
            {
                foreach (FileInfo file in di.EnumerateFiles())
                {
                    //deletes all files with Save in their name
                    if (file.Name.Contains("Save"))
                    {
                        file.Delete();
                    }
                }

                CreateNewConsoleLine("Successfully deleted all save files from " + path + "!", "CONSOLE_SUCCESS_MESSAGE");
            }
            else
            {
                CreateNewConsoleLine("Error: " + path + " has no save files to delete!", "CONSOLE_ERROR_MESSAGE");
            }
        }
    }
    //toggles the unity logs on and off
    private void Command_ToggleUnityLogs()
    {
        if (!displayUnityLogs)
        {
            CreateNewConsoleLine("Showing Unity logs.", "CONSOLE_SUCCESS_MESSAGE");
            displayUnityLogs = true;
        }

        else if (displayUnityLogs)
        {
            CreateNewConsoleLine("No longer showing Unity logs.", "CONSOLE_SUCCESS_MESSAGE");
            displayUnityLogs = false;
        }
    }
    //resets all game cells
    public void Command_GlobalCellReset()
    {
        foreach (GameObject cell in allCells)
        {
            cell.GetComponent<Manager_CurrentCell>().CellReset();
        }

        CreateNewConsoleLine("System: Global cell reset.", "CONSOLE_SUCCESS_MESSAGE");
    }
    //clears the console
    private void Command_ClearConsole()
    {
        separatedWords.Clear();

        lastOutput = "";

        foreach (GameObject createdText in createdTexts)
        {
            Destroy(createdText);
        }
        createdTexts.Clear();
    }
    //quits the game
    private void Command_Quit()
    {
        CreateNewConsoleLine("Quitting game through console.", "CONSOLE_SUCCESS_MESSAGE");
        Application.Quit();
    }

    private void Command_SelectTarget()
    {
        CreateNewConsoleLine("Selecting target - Click on any object on screen or press ESC to cancel selection and to return back to console.", "CONSOLE_SUCCESS_MESSAGE");

        txt_SelectedTargetName.text = "Click on a GameObject to select it as a target.";
        par_Managers.GetComponent<Manager_UIReuse>().par_Console.transform.localPosition = new Vector3(0, -3000, 0);

        par_Managers.GetComponent<Manager_UIReuse>().txt_InsertedTextSlot.DeactivateInputField();

        isSelectingTarget = true;
    }

    private void Command_EditTarget()
    {
        if (target != null)
        {
            //two words command
            if (separatedWords.Count == 2)
            {
                string secondCommandName = separatedWords[1];
                bool secondCommand = int.TryParse(secondCommandName, out _);

                //shows selected target states and commands
                if (secondCommandName == "showinfo"
                    && !secondCommand)
                {
                    //AI states and variables
                    if (target.GetComponent<UI_AIContent>() != null)
                    {
                        CreateNewConsoleLine("--- target states:", "CONSOLE_INFO_MESSAGE");

                        if (target.GetComponent<AI_Health>() != null)
                        {
                            //killableState
                            CreateNewConsoleLine("canBeKilled = " + target.GetComponent<AI_Health>().isKillable, "CONSOLE_INFO_MESSAGE");
                            //hostileState
                            CreateNewConsoleLine("canBeHostile = " + target.GetComponent<AI_Health>().canBeHostile, "CONSOLE_INFO_MESSAGE");
                        }
                        else if (target.GetComponent<AI_Health>() == null)
                        {
                            CreateNewConsoleLine("canBeKilled = False", "CONSOLE_INFO_MESSAGE");
                            CreateNewConsoleLine("canBeHostile = False", "CONSOLE_INFO_MESSAGE");

                        }
                        CreateNewConsoleLine("factionName = " + target.GetComponent<UI_AIContent>().faction.ToString() + "", "CONSOLE_INFO_MESSAGE");
                        CreateNewConsoleLine("--- target commands:", "CONSOLE_INFO_MESSAGE");

                        if (target.GetComponent<AI_Health>() != null)
                        {
                            CreateNewConsoleLine("target kill - kills the target if it is not protected.", "CONSOLE_INFO_MESSAGE");
                            CreateNewConsoleLine("target sethostilestate hostileStateValue - sets target hostile state to either 0 or 1", "CONSOLE_INFO_MESSAGE");
                            CreateNewConsoleLine("target setkillablestate killableStateValue - sets target killable state to either 0 or 1", "CONSOLE_INFO_MESSAGE");
                        }

                        CreateNewConsoleLine("target setfaction factionName - changes targets faction to factionName", "CONSOLE_INFO_MESSAGE");
                    }
                    //item states and commands
                    else if (target.GetComponent<Env_Item>() != null)
                    {
                        Env_Item itemScript = target.GetComponent<Env_Item>();

                        CreateNewConsoleLine("--- target states:", "CONSOLE_INFO_MESSAGE");

                        //is item protected
                        CreateNewConsoleLine("isProtected = " + itemScript.isProtected, "CONSOLE_INFO_MESSAGE");
                        //is item stackable
                        CreateNewConsoleLine("isStackable = " + itemScript.isStackable, "CONSOLE_INFO_MESSAGE");

                        CreateNewConsoleLine("itemCount = " + itemScript.int_itemCount + "", "CONSOLE_INFO_MESSAGE");
                        CreateNewConsoleLine("itemValue = " + itemScript.int_ItemValue + " (" + itemScript.int_ItemValue * itemScript.int_itemCount + ")", "CONSOLE_INFO_MESSAGE");
                        CreateNewConsoleLine("itemWeight = " + itemScript.int_ItemWeight + " (" + itemScript.int_ItemWeight * itemScript.int_itemCount + ")", "CONSOLE_INFO_MESSAGE");

                        if (target.GetComponent<Item_Gun>() != null)
                        {
                            CreateNewConsoleLine("currentDurability = " + target.GetComponent<Item_Gun>().durability + "", "CONSOLE_INFO_MESSAGE");
                            CreateNewConsoleLine("maxDurability = " + target.GetComponent<Item_Gun>().maxDurability, "CONSOLE_INFO_MESSAGE");
                        }
                        else if (target.GetComponent<Item_Melee>() != null)
                        {
                            CreateNewConsoleLine("currentdurability = " + target.GetComponent<Item_Melee>().durability + "", "CONSOLE_INFO_MESSAGE");
                            CreateNewConsoleLine("maxdurability = " + target.GetComponent<Item_Melee>().maxDurability, "CONSOLE_INFO_MESSAGE");
                        }

                        CreateNewConsoleLine("--- target commands:", "CONSOLE_INFO_MESSAGE");

                        //default modifiable variables for all gameobjects
                        CreateNewConsoleLine("target disable - disables gameobject if it isn't protected", "CONSOLE_INFO_MESSAGE");
                        CreateNewConsoleLine("target enable - enables gameobject if new gameobject hasn't been yet selected or console hasn't been yet closed", "CONSOLE_INFO_MESSAGE");

                        CreateNewConsoleLine("target setcount countValue - changes the item count to countValue if it is stackable", "CONSOLE_INFO_MESSAGE");
                        CreateNewConsoleLine("target setvalue valueValue - changes individual item value to valueValue", "CONSOLE_INFO_MESSAGE");
                        CreateNewConsoleLine("target setweight weightValue - changes individual item weight to weightValue", "CONSOLE_INFO_MESSAGE");

                        if (target.GetComponent<Item_Gun>() != null
                            || target.GetComponent<Item_Melee>() != null)
                        {
                            CreateNewConsoleLine("target setdurability durabilityValue - changes individual item durability to durabilityValue", "CONSOLE_INFO_MESSAGE");
                            CreateNewConsoleLine("target setmaxdurability maxDurabilityValue - changes individual item max durability to maxDurabilityValue", "CONSOLE_INFO_MESSAGE");
                        }
                    }
                    //door/container states and commands
                    else if (target.name == "door_interactable"
                             || target.GetComponent<Inv_Container>() != null)
                    {
                        CreateNewConsoleLine("--- target states:", "CONSOLE_INFO_MESSAGE");

                        Transform par = target.transform.parent.parent;
                        GameObject door = null;
                        GameObject container = null;

                        if (target.GetComponent<Inv_Container>() != null)
                        {
                            container = target;
                        }
                        else
                        {
                            foreach (Transform child in par)
                            {
                                if (child.GetComponent<Env_Door>() != null)
                                {
                                    door = child.gameObject;
                                    break;
                                }
                            }
                        }

                        //is door locked and protected or not
                        if (door.GetComponent<Env_Door>() != null)
                        {
                            CreateNewConsoleLine("isprotected = " + door.GetComponent<Env_Door>().isProtected + "", "CONSOLE_INFO_MESSAGE");
                            CreateNewConsoleLine("islocked = " + door.GetComponent<Env_Door>().isLocked, "CONSOLE_INFO_MESSAGE");
                        }
                        //is container locked and protected or not
                        else if (container.GetComponent<Inv_Container>() != null)
                        {
                            CreateNewConsoleLine("isprotected = " + container.GetComponent<Inv_Container>().isProtected + "", "CONSOLE_INFO_MESSAGE");
                            CreateNewConsoleLine("islocked = " + container.GetComponent<Inv_Container>().isLocked, "CONSOLE_INFO_MESSAGE");
                        }

                        CreateNewConsoleLine("--- target commands:", "CONSOLE_INFO_MESSAGE");
                        CreateNewConsoleLine("target unlock - unlocks the door/container if it isn't protected", "CONSOLE_INFO_MESSAGE");
                    }
                    else
                    {
                        CreateNewConsoleLine("No commands found for selected target.", "CONSOLE_INFO_MESSAGE");
                    }
                }

                //disable target
                else if (secondCommandName == "disable"
                    && !secondCommand)
                {
                    //selected AI
                    if (target.GetComponent<UI_AIContent>() != null
                        && target.GetComponent<AI_Health>() != null
                        && target.GetComponent<AI_Health>().isKillable)
                    {
                        CreateNewConsoleLine("Disabled " + target.GetComponent<UI_AIContent>().str_NPCName + ".", "CONSOLE_SUCCESS_MESSAGE");
                        target.SetActive(false);
                    }
                    //selected non-protected interactable item
                    else if (target.GetComponent<Env_Item>() != null
                             && !target.GetComponent<Env_Item>().isProtected)
                    {
                        CreateNewConsoleLine("Disabled " + target.GetComponent<Env_Item>().str_ItemName + ".", "CONSOLE_SUCCESS_MESSAGE");
                        target.SetActive(false);
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: " + target.name + " cannot be disabled! Please select another one.", "CONSOLE_ERROR_MESSAGE");
                    }
                }
                //enable target if same target is disabled and still selected
                else if (secondCommandName == "enable"
                         && !secondCommand)
                {
                    //selected AI
                    if (target.GetComponent<UI_AIContent>() != null)
                    {
                        CreateNewConsoleLine("Enabled " + target.GetComponent<UI_AIContent>().str_NPCName + ".", "CONSOLE_SUCCESS_MESSAGE");
                        target.SetActive(true);
                    }
                    //selected non-protected interactable item
                    else if (target.GetComponent<Env_Item>() != null)
                    {
                        CreateNewConsoleLine("Enabled " + target.GetComponent<Env_Item>().str_ItemName + ".", "CONSOLE_SUCCESS_MESSAGE");
                        target.SetActive(true);
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: " + target.name + " is already enabled!", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                //kill target
                else if (secondCommandName == "kill"
                         && !secondCommand)
                {
                    //if selected target is killable
                    if (target.GetComponent<UI_AIContent>() != null
                        && target.GetComponent<AI_Health>() != null
                        && target.GetComponent<AI_Health>().isKillable)
                    {
                        target.GetComponent<AI_Health>().Death();
                        CreateNewConsoleLine("Killed " + target.GetComponent<UI_AIContent>().str_NPCName + " through console.", "CONSOLE_SUCCESS_MESSAGE");
                    }

                    //custom kill errors
                    else if (target.GetComponent<UI_AIContent>() == null)
                    {
                        CreateNewConsoleLine("Error: Target is not a killable GameObject!", "CONSOLE_ERROR_MESSAGE");
                    }
                    else if ((target.GetComponent<UI_AIContent>() != null
                             && target.GetComponent<AI_Health>() != null
                             && !target.GetComponent<AI_Health>().isKillable)
                             || (target.GetComponent<UI_AIContent>() != null
                             && target.GetComponent<AI_Health>() == null))
                    {
                        CreateNewConsoleLine("Error: Target cannot be killed through console because it is protected!", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                //unlock door or container
                else if (secondCommandName == "unlock"
                         && !secondCommand)
                {
                    Transform par = target.transform.parent.parent;
                    GameObject door = null;
                    GameObject container = null;

                    if (target.GetComponent<Inv_Container>() != null)
                    {
                        container = target;
                    }
                    else
                    {
                        foreach (Transform child in par)
                        {
                            if (child.GetComponent<Env_Door>() != null)
                            {
                                door = child.gameObject;
                                break;
                            }
                        }
                    }

                    if (door != null)
                    {
                        if (door.GetComponent<Env_Door>().isLocked
                        && !door.GetComponent<Env_Door>().isProtected)
                        {
                            door.GetComponent<Env_Lock>().Unlock();
                            CreateNewConsoleLine("Unlocked this door.", "CONSOLE_SUCCESS_MESSAGE");
                        }

                        //custom unlock errors
                        else if (!door.GetComponent<Env_Door>().isLocked
                                 || door.GetComponent<Env_Lock>() == null)
                        {
                            CreateNewConsoleLine("Error: Target is already unlocked!", "CONSOLE_ERROR_MESSAGE");
                        }
                        else if (door.GetComponent<Env_Door>().isProtected)
                        {
                            CreateNewConsoleLine("Error: Target cannot be unlocked through console because it is protected!", "CONSOLE_ERROR_MESSAGE");
                        }
                    }
                    else if (container != null)
                    {
                        if (container.GetComponent<Inv_Container>().isLocked
                            && !container.GetComponent<Inv_Container>().isProtected)
                        {
                            container.GetComponent<Inv_Container>().isLocked = false;
                            CreateNewConsoleLine("Unlocked this container.", "CONSOLE_SUCCESS_MESSAGE");
                        }

                        //custom unlock errors
                        else if (!container.GetComponent<Inv_Container>().isLocked
                                 || container.GetComponent<Env_Lock>() == null)
                        {
                            CreateNewConsoleLine("Error: Target is already unlocked!", "CONSOLE_ERROR_MESSAGE");
                        }
                        else if (container.GetComponent<Inv_Container>().isProtected)
                        {
                            CreateNewConsoleLine("Error: Target cannot be unlocked through console because it is protected!", "CONSOLE_ERROR_MESSAGE");
                        }
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Target is not an unlockable GameObject!", "CONSOLE_ERROR_MESSAGE");
                    }
                }
                //incorrect or unknown command
                else
                {
                    CreateNewConsoleLine("Error: Incorrect or unknown command!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //three words command
            else if (separatedWords.Count == 3)
            {
                string secondCommandName = separatedWords[1];
                bool secondCommand = int.TryParse(secondCommandName, out _);
                string thirdCommandName = separatedWords[2];
                bool thirdCommand = int.TryParse(thirdCommandName, out _);
                if (thirdCommand)
                {
                    insertedValue = int.Parse(separatedWords[2]);
                }

                //selected AI
                if (target.GetComponent<UI_AIContent>() != null)
                {
                    //set hostile state for AI
                    if (secondCommandName == "sethostilestate"
                        && target.GetComponent<AI_Health>() != null
                        && !secondCommand
                        && thirdCommand
                        && insertedValue > -1 
                        && insertedValue < 2)
                    {
                        //non-hostile towards others
                        if (insertedValue == 0)
                        {
                            target.GetComponent<AI_Health>().canBeHostile = false;
                            CreateNewConsoleLine("Set " + target.GetComponent<UI_AIContent>().str_NPCName + "'s hostile state to " + thirdCommandName + ". Target is no longer hostile towards anyone.", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        //hostile towards others
                        else if (insertedValue == 1)
                        {
                            target.GetComponent<AI_Health>().canBeHostile = true;
                            CreateNewConsoleLine("Set " + target.GetComponent<UI_AIContent>().str_NPCName + "'s hostile state to " + thirdCommandName + ". Target can now be hostile towards attackers again.", "CONSOLE_SUCCESS_MESSAGE");
                        }
                    }
                    //set faction for AI
                    else if (secondCommandName == "setfaction"
                             && !secondCommand
                             && !thirdCommand)
                    {
                        //stupid way to check which role was selected but it works
                        if (thirdCommandName == UI_AIContent.Faction.scientists.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.scientists;
                        }
                        else if (thirdCommandName == UI_AIContent.Faction.geifers.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.geifers;
                        }
                        else if (thirdCommandName == UI_AIContent.Faction.annies.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.annies;
                        }
                        else if (thirdCommandName == UI_AIContent.Faction.verbannte.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.verbannte;
                        }
                        else if (thirdCommandName == UI_AIContent.Faction.raiders.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.raiders;
                        }
                        else if (thirdCommandName == UI_AIContent.Faction.military.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.military;
                        }
                        else if (thirdCommandName == UI_AIContent.Faction.verteidiger.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.verteidiger;
                        }
                        else if (thirdCommandName == UI_AIContent.Faction.others.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.others;
                        }

                        CreateNewConsoleLine("Set " + target.GetComponent<UI_AIContent>().str_NPCName + "'s faction to " + thirdCommandName + ".", "CONSOLE_SUCCESS_MESSAGE");
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Incorrect or unknown command or selected targets value cannot be edited!", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                //selected item
                else if (target.GetComponent<Env_Item>() != null
                         && !secondCommand
                         && thirdCommand)
                {
                    //is item protected
                    if (!target.GetComponent<Env_Item>().isProtected)
                    {
                        //is item in range
                        if (insertedValue > -1
                            && insertedValue < 1000001)
                        {
                            if (secondCommandName == "setcount"
                                && target.GetComponent<Env_Item>().isStackable)
                            {
                                target.GetComponent<Env_Item>().int_itemCount = insertedValue;
                                
                                CreateNewConsoleLine("Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s count to " + insertedValue + ".", "CONSOLE_SUCCESS_MESSAGE");
                            }
                            else if (secondCommandName == "setvalue")
                            {
                                target.GetComponent<Env_Item>().int_maxItemValue = insertedValue;

                               CreateNewConsoleLine("Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s value to " + insertedValue + ".", "CONSOLE_SUCCESS_MESSAGE");
                            }
                            else if (secondCommandName == "setweight")
                            {
                                target.GetComponent<Env_Item>().int_ItemWeight = insertedValue;

                                CreateNewConsoleLine("Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s weight to " + insertedValue + ".", "CONSOLE_SUCCESS_MESSAGE");
                            }
                            else if (secondCommandName == "setdurability")
                            {
                                if (target.GetComponent<Item_Gun>() != null
                                    && insertedValue <= target.GetComponent<Item_Gun>().maxDurability)
                                {
                                    target.GetComponent<Item_Gun>().durability = insertedValue;

                                    CreateNewConsoleLine("Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s durability to " + insertedValue + ".", "CONSOLE_SUCCESS_MESSAGE");
                                }
                                else if (target.GetComponent<Item_Melee>() != null
                                         && insertedValue <= target.GetComponent<Item_Melee>().maxDurability)
                                {
                                    target.GetComponent<Item_Melee>().durability = insertedValue;

                                    CreateNewConsoleLine("Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s durability to " + insertedValue + ".", "CONSOLE_SUCCESS_MESSAGE");
                                }

                                //custom error message for too high durability
                                else if ((target.GetComponent<Item_Gun>() != null
                                         && insertedValue > target.GetComponent<Item_Gun>().maxDurability)
                                         || (target.GetComponent<Item_Melee>() != null
                                         && insertedValue > target.GetComponent<Item_Melee>().maxDurability))
                                {
                                    CreateNewConsoleLine("Error: Target item durability cannot be higher than its max durability!", "CONSOLE_ERROR_MESSAGE");
                                }
                                //custom error message for weapon not found
                                else if (target.GetComponent<Item_Gun>() == null
                                         && target.GetComponent<Item_Melee>() == null)
                                {
                                    CreateNewConsoleLine("Error: Targets durability cannot be edited. Target is not an item with any durability!", "CONSOLE_ERROR_MESSAGE");
                                }
                            }
                            else if (secondCommandName == "setmaxdurability")
                            {
                                if (target.GetComponent<Item_Gun>() != null
                                    && insertedValue > target.GetComponent<Item_Gun>().durability)
                                {
                                    target.GetComponent<Item_Gun>().maxDurability = insertedValue;

                                    CreateNewConsoleLine("Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s max durability to " + insertedValue + ".", "CONSOLE_SUCCESS_MESSAGE");
                                }
                                else if (target.GetComponent<Item_Melee>() != null
                                         && insertedValue > target.GetComponent<Item_Melee>().durability)
                                {
                                    target.GetComponent<Item_Melee>().maxDurability = insertedValue;

                                    CreateNewConsoleLine("Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s max durability to " + insertedValue + ".", "CONSOLE_SUCCESS_MESSAGE");
                                }

                                //custom error message for too low max durability
                                else if ((target.GetComponent<Item_Gun>() != null
                                         && insertedValue < target.GetComponent<Item_Gun>().durability)
                                         || (target.GetComponent<Item_Melee>() != null
                                         && insertedValue < target.GetComponent<Item_Melee>().durability))
                                {
                                    CreateNewConsoleLine("Error: Target item max durability cannot be lower than its durability!", "CONSOLE_ERROR_MESSAGE");
                                }
                                //custom error message for weapon not found
                                else if (target.GetComponent<Item_Gun>() == null
                                         && target.GetComponent<Item_Melee>() == null)
                                {
                                    CreateNewConsoleLine("Error: Targets max durability cannot be edited. Target is not an item with any durability!", "CONSOLE_ERROR_MESSAGE");
                                }
                            }

                            else if (secondCommandName == "setcount"
                                     && !target.GetComponent<Env_Item>().isStackable)
                            {
                                CreateNewConsoleLine("Error: Target count cannot be edited because it is not stackable!", "CONSOLE_ERROR_MESSAGE");
                            }
                            else
                            {
                                CreateNewConsoleLine("Error: Incorrect or unknown command!", "CONSOLE_ERROR_MESSAGE");
                            }
                        }
                        else
                        {
                            CreateNewConsoleLine("Error: Selected targets value is out of range!", "CONSOLE_ERROR_MESSAGE");
                        }
                    }
                    else
                    {
                        CreateNewConsoleLine("Error: Target values cannot be edited because target is protected!", "CONSOLE_ERROR_MESSAGE");
                    }
                }

                //incorrect or unknown command
                else
                {
                    CreateNewConsoleLine("Error: Incorrect or unknown command!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //incorrect or unknown command
            else
            {
                CreateNewConsoleLine("Error: Incorrect or unknown command!", "CONSOLE_ERROR_MESSAGE");
            }
        }
        else if (target == null)
        {
            CreateNewConsoleLine("Error: No target was selected! Use the selecttarget command to select a target to edit.", "CONSOLE_ERROR_MESSAGE");
        }
    }

    //shows all game factions
    private void Command_ShowFactions()
    {
        CreateNewConsoleLine("Game factions:", "CONSOLE_INFO_MESSAGE");

        foreach (GameObject faction in par_Managers.GetComponent<GameManager>().gameFactions)
        {
            CreateNewConsoleLine(faction.GetComponent<Manager_FactionReputation>().faction.ToString(), "CONSOLE_INFO_MESSAGE");
        }
    }
    //gets the reputation values between two factions
    private void Command_SetRep()
    {
        string factionName1 = separatedWords[1];
        string factionName2 = separatedWords[2];
        bool isInt1 = int.TryParse(factionName1, out _);
        bool isInt2 = int.TryParse(factionName2, out _);
        bool isInsertedValueInt = int.TryParse(separatedWords[3], out _);
        if (isInt1 || isInt2)
        {
            CreateNewConsoleLine("Error: Faction name cannot be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInsertedValueInt)
        {
            CreateNewConsoleLine("Error: Inserted value must be a whole number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInt1 && !isInt2 && isInsertedValueInt)
        {
            int value = int.Parse(separatedWords[3]);
            if (value > -1001 && value < 1001)
            {
                foreach (GameObject faction in par_Managers.GetComponent<GameManager>().gameFactions)
                {
                    Manager_FactionReputation targetFactionScript = faction.GetComponent<Manager_FactionReputation>();
                    if (targetFactionScript.faction.ToString() == factionName1)
                    {
                        if (factionName2 == "Player")
                        {
                            targetFactionScript.vsPlayer = value;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                            CreateNewConsoleLine("Changed " + factionName1 + " and " + factionName2 + "'s reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else if (factionName2 == "Scientists")
                        {
                            targetFactionScript.vsScientists = value;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                            CreateNewConsoleLine("Changed " + factionName1 + " and " + factionName2 + "'s reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else if (factionName2 == "Geifers")
                        {
                            targetFactionScript.vsGeifers = value;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                            CreateNewConsoleLine("Changed " + factionName1 + " and " + factionName2 + "'s reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else if (factionName2 == "Annies")
                        {
                            targetFactionScript.vsAnnies = value;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                            CreateNewConsoleLine("Changed " + factionName1 + " and " + factionName2 + "'s reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else if (factionName2 == "Verbannte")
                        {
                            targetFactionScript.vsVerbannte = value;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                            CreateNewConsoleLine("Changed " + factionName1 + " and " + factionName2 + "'s reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else if (factionName2 == "Raiders")
                        {
                            targetFactionScript.vsRaiders = value;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                            CreateNewConsoleLine("Changed " + factionName1 + " and " + factionName2 + "'s reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else if (factionName2 == "Military")
                        {
                            targetFactionScript.vsMilitary = value;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                            CreateNewConsoleLine("Changed " + factionName1 + " and " + factionName2 + "'s reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else if (factionName2 == "Verteidiger")
                        {
                            targetFactionScript.vsVerteidiger = value;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                            CreateNewConsoleLine("Changed " + factionName1 + " and " + factionName2 + "'s reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }
                        else if (factionName2 == "Others")
                        {
                            targetFactionScript.vsOthers = value;
                            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                            CreateNewConsoleLine("Changed " + factionName1 + " and " + factionName2 + "'s reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                        }

                        break;
                    }
                }
            }
            else if (value <= -1001)
            {
                Debug.LogError("Error: Inserted reputation value must be higher than -1001!");
            }
            else if (value >= 1001)
            {
                Debug.LogError("Error: Inserted reputation value must be lower than 1001!");
            }
        }
    }

    //allows to teleport to custom location with coordinates
    private void Command_TeleportToVector3Location()
    {
        string secondWord = separatedWords[1];
        string thirdWord = separatedWords[2];
        string fourthWord = separatedWords[3];

        bool firstFloatCorrect = float.TryParse(secondWord, out float firstVec);
        bool secondFloatCorrect = float.TryParse(thirdWord, out float secondVec);
        bool thirdFloatCorrect = float.TryParse(fourthWord, out float thirdVec);
        if (!firstFloatCorrect)
        {
            CreateNewConsoleLine("Error: Teleport coordinate first input must be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        if (!secondFloatCorrect)
        {
            CreateNewConsoleLine("Error: Teleport coordinate second input must be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        if (!thirdFloatCorrect)
        {
            CreateNewConsoleLine("Error: Teleport coordinate third input must be a number!", "CONSOLE_ERROR_MESSAGE");
        }

        //if all 3 are numbers then assign them as
        //teleport position coordinates and move player to target
        if (firstFloatCorrect && secondFloatCorrect && thirdFloatCorrect)
        {
            if (firstVec >= 1000001 || firstVec <= -1000001)
            {
                CreateNewConsoleLine("Error: Teleport coordinate first input cannot be higher than 1000000 and lower than -1000000!", "CONSOLE_ERROR_MESSAGE");
            }
            if (secondVec >= 1000001 || secondVec <= -1000001)
            {
                CreateNewConsoleLine("Error: Teleport coordinate second input cannot be higher than 1000000 and lower than -1000000!", "CONSOLE_ERROR_MESSAGE");
            }
            if (thirdVec >= 1000001 || thirdVec <= -1000001)
            {
                CreateNewConsoleLine("Error: Teleport coordinate third input cannot be higher than 1000000 and lower than -1000000!", "CONSOLE_ERROR_MESSAGE");
            }

            else
            {
                //set teleportPos;
                Vector3 teleportPos = new Vector3(firstVec, secondVec, thirdVec);

                CreateNewConsoleLine("--tp " + firstVec + " " + secondVec + " " + thirdVec + "--", "CONSOLE_SUCCESS_MESSAGE");
                CreateNewConsoleLine("Success: Teleported player to " + teleportPos + "!", "CONSOLE_SUCCESS_MESSAGE");

                thePlayer.transform.position = teleportPos;
                ToggleConsole();
            }
        }
    }
    //list all valid game cell names
    private void Command_ShowAllCells()
    {
        CreateNewConsoleLine("All game cells:", "CONSOLE_INFO_MESSAGE");

        foreach (string cellname in cellnames)
        {
            CreateNewConsoleLine(cellname, "CONSOLE_INFO_MESSAGE");
        }
    }
    //discover all game cell names
    private void Command_DiscoverAllCells()
    {
        foreach (GameObject cell in allCells)
        {
            if (!cell.GetComponent<Manager_CurrentCell>().discoveredCell)
            {
                cell.GetComponent<Manager_CurrentCell>().discoveredCell = true;
                cell.GetComponent<Manager_CurrentCell>().EnableCellTeleportButtonOnMap();
            }
        }

        CreateNewConsoleLine("All cells are now discovered!", "CONSOLE_SUCCESS_MESSAGE");
    }
    //teleport to specific cell spawn point
    private void Command_TeleportToCell()
    {
        string cellName = separatedWords[1];
        bool isInt = int.TryParse(cellName, out _);
        if (isInt)
        {
            CreateNewConsoleLine("Error: Cell name cannot be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInt)
        {
            bool foundCell = false;

            for (int i = 0; i < allCells.Count; i++)
            {
                GameObject cell = allCells[i];

                if (cellName == cell.GetComponent<Manager_CurrentCell>().str_CellName)
                {
                    CreateNewConsoleLine("Teleported to cell " + cellName + "!", "CONSOLE_SUCCESS_MESSAGE");
                    //get vector3 from valid cell
                    Transform teleportLoc = cell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint;
                    //move player to cell
                    thePlayer.transform.position = teleportLoc.position;
                    //load teleported cell items
                    cell.GetComponent<Manager_CurrentCell>().LoadCell();
                    ToggleConsole();

                    foundCell = true;

                    break;
                }
            }

            if (!foundCell)
            {
                CreateNewConsoleLine("Error: Cell name not found! Use sac to list all valid game cells.", "CONSOLE_ERROR_MESSAGE");
            }
        }
    }

    //get player current coordinates
    private void Command_GetPlayerCurrentCoordinates()
    {
        if (separatedWords.Count == 2)
        {
            Vector3 currCoords = thePlayer.transform.position;

            CreateNewConsoleLine("Player current coordinates are " + currCoords + ".", "CONSOLE_INFO_MESSAGE");
        }
    }
    //toggles godmode on and off
    private void Command_ToggleGodMode()
    {
        if (!thePlayer.GetComponent<Player_Health>().canTakeDamage)
        {
            thePlayer.GetComponent<Player_Health>().canTakeDamage = true;
            CreateNewConsoleLine("Disabled godmode.", "CONSOLE_SUCCESS_MESSAGE");
        }

        else if (thePlayer.GetComponent<Player_Health>().canTakeDamage)
        {
            thePlayer.GetComponent<Player_Health>().canTakeDamage = false;
            CreateNewConsoleLine("Enabled godmode.", "CONSOLE_SUCCESS_MESSAGE");
        }
    }
    //toggles noclip on and off
    private void Command_ToggleNoclip()
    {
        if (!thePlayer.GetComponent<Player_Movement>().isNoclipping)
        {
            CreateNewConsoleLine("Enabled noclip.", "CONSOLE_SUCCESS_MESSAGE");

            thePlayer.GetComponent<Player_Movement>().isClimbingLadder = false;
            thePlayer.GetComponent<Player_Movement>().isNoclipping = true;
        }
        else if (thePlayer.GetComponent<Player_Movement>().isNoclipping)
        {
            CreateNewConsoleLine("Disabled noclip.", "CONSOLE_SUCCESS_MESSAGE");

            thePlayer.GetComponent<Player_Movement>().isNoclipping = false;
        }
    }
    //toggles AI detection on and off
    private void Command_ToggleAIDetection()
    {
        if (!toggleAIDetection)
        {
            toggleAIDetection = true;
            CreateNewConsoleLine("Enabled AI detection.", "CONSOLE_SUCCESS_MESSAGE");
        }

        else if (toggleAIDetection)
        {
            toggleAIDetection = false;
            CreateNewConsoleLine("Disabled AI detection.", "CONSOLE_SUCCESS_MESSAGE");
        }
    }

    //shows all player stats
    private void Command_ShowPlayerStats()
    {
        CreateNewConsoleLine("Player stats:", "CONSOLE_INFO_MESSAGE");

        //get current player health
        string health = thePlayer.GetComponent<Player_Health>().health.ToString();
        CreateNewConsoleLine("health: " + health, "CONSOLE_INFO_MESSAGE");
        //get players current max health
        string maxHealth = thePlayer.GetComponent<Player_Health>().maxHealth.ToString();
        CreateNewConsoleLine("maxhealth: " + maxHealth, "CONSOLE_INFO_MESSAGE");
        //get player current stamina
        float stamina = thePlayer.GetComponent<Player_Movement>().currentStamina;
        CreateNewConsoleLine("stamina: " + stamina, "CONSOLE_INFO_MESSAGE");
        //get player max stamina
        float maxstamina = thePlayer.GetComponent<Player_Movement>().maxStamina;
        CreateNewConsoleLine("maxstamina: " + maxstamina, "CONSOLE_INFO_MESSAGE");
        //get player current mental state
        float mentalstate = thePlayer.GetComponent<Player_Health>().mentalState;
        CreateNewConsoleLine("mentalstate: " + mentalstate, "CONSOLE_INFO_MESSAGE");
        //get player max mental state
        float maxmentalstate = thePlayer.GetComponent<Player_Health>().maxMentalState;
        CreateNewConsoleLine("maxmentalstate: " + maxmentalstate, "CONSOLE_INFO_MESSAGE");
        //get player current radiation
        float radiation = thePlayer.GetComponent<Player_Health>().radiation;
        CreateNewConsoleLine("radiation: " + radiation, "CONSOLE_INFO_MESSAGE");
        //get player max radiation
        float maxradiation = thePlayer.GetComponent<Player_Health>().maxRadiation;
        CreateNewConsoleLine("maxradiation: " + maxradiation, "CONSOLE_INFO_MESSAGE");
        //get player stamina recharge speed
        float staminarecharge = thePlayer.GetComponent<Player_Movement>().staminaRecharge;
        CreateNewConsoleLine("staminarecharge: " + staminarecharge, "CONSOLE_INFO_MESSAGE");
        //get player speed
        float speed = thePlayer.GetComponent<Player_Movement>().speedIncrease;
        CreateNewConsoleLine("speed: " + speed, "CONSOLE_INFO_MESSAGE");
        //get player jump height
        float jumpheight = thePlayer.GetComponent<Player_Movement>().jumpHeight;
        CreateNewConsoleLine("jumpheight: " + jumpheight, "CONSOLE_INFO_MESSAGE");
        //get player current inventory space
        int currentSpace = thePlayer.GetComponent<Inv_Player>().invSpace;
        CreateNewConsoleLine("current used inv space: " + currentSpace + " <not editable>", "CONSOLE_INFO_MESSAGE");
        //get player max inventory space
        int maxInvSpace = thePlayer.GetComponent<Inv_Player>().maxInvSpace;
        CreateNewConsoleLine("maxinvspace: " + maxInvSpace, "CONSOLE_INFO_MESSAGE");
        //get player current money
        int money = thePlayer.GetComponent<Inv_Player>().money;
        CreateNewConsoleLine("money: " + money, "CONSOLE_INFO_MESSAGE");
        //get player flashlight battery if player has equipped flashlight with battery
        if (thePlayer.GetComponent<Inv_Player>().equippedFlashlight != null
            && thePlayer.GetComponent<Inv_Player>().equippedFlashlight.GetComponent<Item_Flashlight>().battery != null)
        {
            GameObject equippedFlashlight = thePlayer.GetComponent<Inv_Player>().equippedFlashlight;
            GameObject flashlightBattery = equippedFlashlight.GetComponent<Item_Flashlight>().battery;

            CreateNewConsoleLine("flashlight battery: " + flashlightBattery.GetComponent<Item_Battery>().currentBattery.ToString(), "CONSOLE_INFO_MESSAGE");
        }
    }
    private void Command_ResetPlayerStats()
    {
        if (thePlayer.GetComponent<Player_Health>().health == originalMaxHealth
            && thePlayer.GetComponent<Player_Health>().maxHealth == originalMaxHealth
            && thePlayer.GetComponent<Player_Movement>().currentStamina == originalMaxStamina
            && thePlayer.GetComponent<Player_Movement>().maxStamina == originalMaxStamina
            && thePlayer.GetComponent<Player_Movement>().staminaRecharge == originalStaminaRecharge
            && thePlayer.GetComponent<Player_Health>().maxMentalState == originalMaxMentalState
            && thePlayer.GetComponent<Player_Health>().maxRadiation == originalMaxRadiation
            && thePlayer.GetComponent<Player_Movement>().speedIncrease == originalSpeed
            && thePlayer.GetComponent<Player_Movement>().jumpHeight == originalJumpHeight + 0.75f
            && thePlayer.GetComponent<Inv_Player>().maxInvSpace == originalMaxInvspace)
        {
            CreateNewConsoleLine("Error: All player stats already are at their original value.", "CONSOLE_ERROR_MESSAGE");
        }
        else
        {
            thePlayer.GetComponent<Player_Health>().health = originalMaxHealth;
            thePlayer.GetComponent<Player_Health>().maxHealth = originalMaxHealth;
            thePlayer.GetComponent<Player_Movement>().currentStamina = originalMaxStamina;
            thePlayer.GetComponent<Player_Movement>().maxStamina = originalMaxStamina;
            thePlayer.GetComponent<Player_Movement>().staminaRecharge = originalStaminaRecharge;
            thePlayer.GetComponent<Player_Health>().mentalState = 100;
            thePlayer.GetComponent<Player_Health>().maxMentalState = originalMaxMentalState;
            thePlayer.GetComponent<Player_Health>().radiation = 0;
            thePlayer.GetComponent<Player_Health>().maxRadiation = originalMaxRadiation;
            thePlayer.GetComponent<Player_Movement>().speedIncrease = originalSpeed;
            thePlayer.GetComponent<Player_Movement>().jumpHeight = originalJumpHeight + 0.75f; //increasing the jump height to the real original jump height
            thePlayer.GetComponent<Inv_Player>().maxInvSpace = originalMaxInvspace;

            //fill flashlight battery
            if (thePlayer.GetComponent<Inv_Player>().equippedFlashlight != null
                && thePlayer.GetComponent<Inv_Player>().equippedFlashlight.GetComponent<Item_Flashlight>().battery != null)
            {
                GameObject equippedFlashlight = thePlayer.GetComponent<Inv_Player>().equippedFlashlight;
                GameObject flashlightBattery = equippedFlashlight.GetComponent<Item_Flashlight>().battery;

                float maxBattery = flashlightBattery.GetComponent<Item_Battery>().maxBattery;
                flashlightBattery.GetComponent<Item_Battery>().currentBattery = maxBattery;

                par_Managers.GetComponent<Manager_UIReuse>().flashlightBattery = maxBattery;
                par_Managers.GetComponent<Manager_UIReuse>().flashlightMaxBattery = maxBattery;
                par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFlashlight();
            }

            par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();

            par_Managers.GetComponent<Manager_UIReuse>().health = originalMaxHealth;
            par_Managers.GetComponent<Manager_UIReuse>().maxHealth = originalMaxHealth;
            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerHealth();

            par_Managers.GetComponent<Manager_UIReuse>().stamina = originalMaxStamina;
            par_Managers.GetComponent<Manager_UIReuse>().maxStamina = originalMaxStamina;
            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerStamina();

            par_Managers.GetComponent<Manager_UIReuse>().radiation = 0;
            par_Managers.GetComponent<Manager_UIReuse>().maxRadiation = originalMaxRadiation;
            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerRadiation();

            par_Managers.GetComponent<Manager_UIReuse>().mentalState = originalMaxMentalState;
            par_Managers.GetComponent<Manager_UIReuse>().maxMentalState = originalMaxMentalState;
            par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerMentalState();

            CreateNewConsoleLine("All modifiable player stats were reset to their original values.", "CONSOLE_SUCCESS_MESSAGE");
        }
    }
    //set a player stat
    private void Command_SetPlayerStat()
    {
        string statName = separatedWords[2];

        bool isInt = int.TryParse(statName, out _);
        bool isInsertedValueInt = int.TryParse(separatedWords[3], out _);
        if (isInt)
        {
            CreateNewConsoleLine("Error: Player stat name cannot be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInsertedValueInt)
        {
            CreateNewConsoleLine("Error: Inserted value must be a whole number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInt && isInsertedValueInt)
        {
            insertedValue = int.Parse(separatedWords[3]);
            if (statName == "health")
            {
                if (insertedValue <= thePlayer.GetComponent<Player_Health>().maxHealth
                    && insertedValue > -1)
                {
                    thePlayer.GetComponent<Player_Health>().health = insertedValue;
                    string health = thePlayer.GetComponent<Player_Health>().health.ToString();
                    CreateNewConsoleLine("Changed player current health to " + health + ".", "CONSOLE_SUCCESS_MESSAGE");

                    par_Managers.GetComponent<Manager_UIReuse>().health = thePlayer.GetComponent<Player_Health>().health;
                    par_Managers.GetComponent<Manager_UIReuse>().maxHealth = thePlayer.GetComponent<Player_Health>().maxHealth;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerHealth();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue > thePlayer.GetComponent<Player_Health>().maxHealth)
                {
                    CreateNewConsoleLine("Error: Player current health cannot be set over player max health!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue < 0)
                {
                    CreateNewConsoleLine("Error: Player current health must be set over -1!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player max health
            else if (statName == "maxhealth")
            {
                if (insertedValue >= thePlayer.GetComponent<Player_Health>().health
                    && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Health>().maxHealth = insertedValue;
                    string maxhealth = thePlayer.GetComponent<Player_Health>().maxHealth.ToString();
                    CreateNewConsoleLine("Changed player max health to " + maxhealth + ".", "CONSOLE_SUCCESS_MESSAGE");

                    par_Managers.GetComponent<Manager_UIReuse>().health = thePlayer.GetComponent<Player_Health>().health;
                    par_Managers.GetComponent<Manager_UIReuse>().maxHealth = thePlayer.GetComponent<Player_Health>().maxHealth;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerHealth();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue < thePlayer.GetComponent<Player_Health>().health)
                {
                    CreateNewConsoleLine("Error: Player max health cannot be set under player current health!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue >= 1000001)
                {
                    CreateNewConsoleLine("Error: Player max health must be set below 1000001!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player stamina
            else if (statName == "stamina")
            {
                if (insertedValue <= thePlayer.GetComponent<Player_Movement>().maxStamina 
                    && insertedValue > -1)
                {
                    thePlayer.GetComponent<Player_Movement>().currentStamina = insertedValue;
                    string stamina = thePlayer.GetComponent<Player_Movement>().currentStamina.ToString();
                    CreateNewConsoleLine("Changed player current stamina to " + stamina + ".", "CONSOLE_SUCCESS_MESSAGE");

                    par_Managers.GetComponent<Manager_UIReuse>().stamina = thePlayer.GetComponent<Player_Movement>().currentStamina;
                    par_Managers.GetComponent<Manager_UIReuse>().maxStamina = thePlayer.GetComponent<Player_Movement>().maxStamina;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerStamina();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue < 0)
                {
                    CreateNewConsoleLine("Error: Player current stamina must be set over -1!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue > thePlayer.GetComponent<Player_Movement>().maxStamina)
                {
                    CreateNewConsoleLine("Error: Player current stamina cannot be set over player max stamina!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player max stamina
            else if (statName == "maxstamina")
            {
                if (insertedValue >= thePlayer.GetComponent<Player_Movement>().currentStamina 
                    && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Movement>().maxStamina = insertedValue;
                    string maxstamina = thePlayer.GetComponent<Player_Movement>().maxStamina.ToString();
                    CreateNewConsoleLine("Changed player max stamina to " + maxstamina + ".", "CONSOLE_SUCCESS_MESSAGE");

                    par_Managers.GetComponent<Manager_UIReuse>().stamina = thePlayer.GetComponent<Player_Movement>().currentStamina;
                    par_Managers.GetComponent<Manager_UIReuse>().maxStamina = thePlayer.GetComponent<Player_Movement>().maxStamina;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerStamina();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue >= 1000001)
                {
                    CreateNewConsoleLine("Error: Player max stamina must be set below 1000001!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue < thePlayer.GetComponent<Player_Movement>().currentStamina)
                {
                    CreateNewConsoleLine("Error: Player max stamina cannot be set under player current stamina!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player mental state
            else if (statName == "mentalstate")
            {
                if (insertedValue <= thePlayer.GetComponent<Player_Health>().maxMentalState
                    && insertedValue > -1)
                {
                    thePlayer.GetComponent<Player_Health>().mentalState = insertedValue;
                    string mentalstate = thePlayer.GetComponent<Player_Health>().mentalState.ToString();
                    CreateNewConsoleLine("Changed player current mental state to " + mentalstate + ".", "CONSOLE_SUCCESS_MESSAGE");

                    par_Managers.GetComponent<Manager_UIReuse>().mentalState = thePlayer.GetComponent<Player_Health>().mentalState;
                    par_Managers.GetComponent<Manager_UIReuse>().maxMentalState = thePlayer.GetComponent<Player_Health>().maxMentalState;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerMentalState();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue < 0)
                {
                    CreateNewConsoleLine("Error: Player current mental stamina must be set over -1!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue > thePlayer.GetComponent<Player_Health>().maxMentalState)
                {
                    CreateNewConsoleLine("Error: Player current mental state cannot be set over player max mental state!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player max mental state
            else if (statName == "maxmentalstate")
            {
                if (insertedValue >= thePlayer.GetComponent<Player_Health>().mentalState
                    && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Health>().maxMentalState = insertedValue;
                    string maxmentalstate = thePlayer.GetComponent<Player_Health>().maxMentalState.ToString();
                    CreateNewConsoleLine("Changed player max mental state to " + maxmentalstate + ".", "CONSOLE_SUCCESS_MESSAGE");

                    par_Managers.GetComponent<Manager_UIReuse>().mentalState = thePlayer.GetComponent<Player_Health>().mentalState;
                    par_Managers.GetComponent<Manager_UIReuse>().maxMentalState = thePlayer.GetComponent<Player_Health>().maxMentalState;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerMentalState();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue >= 1000001)
                {
                    CreateNewConsoleLine("Error: Player max mental state must be set below 1000001!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue < thePlayer.GetComponent<Player_Health>().mentalState)
                {
                    CreateNewConsoleLine("Error: Player max mental state cannot be set under mental state!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player radiation
            else if (statName == "radiation")
            {
                if (insertedValue <= thePlayer.GetComponent<Player_Health>().maxRadiation
                    && insertedValue > -1)
                {
                    thePlayer.GetComponent<Player_Health>().radiation = insertedValue;
                    string radiation = thePlayer.GetComponent<Player_Health>().radiation.ToString();
                    CreateNewConsoleLine("Changed player current radiation to " + radiation + ".", "CONSOLE_SUCCESS_MESSAGE");

                    par_Managers.GetComponent<Manager_UIReuse>().radiation = thePlayer.GetComponent<Player_Health>().radiation;
                    par_Managers.GetComponent<Manager_UIReuse>().maxRadiation = thePlayer.GetComponent<Player_Health>().maxRadiation;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerRadiation();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue < 0)
                {
                    CreateNewConsoleLine("Error: Player current radiation must be set over -1!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue > thePlayer.GetComponent<Player_Health>().maxRadiation)
                {
                    CreateNewConsoleLine("Error: Player current radiation cannot be set over player max radiation!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player max radiation
            else if (statName == "maxradiation")
            {
                if (insertedValue >= thePlayer.GetComponent<Player_Health>().radiation
                    && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Health>().maxRadiation = insertedValue;
                    string maxradiation = thePlayer.GetComponent<Player_Health>().maxRadiation.ToString();
                    CreateNewConsoleLine("Changed player max radiation to " + maxradiation + ".", "CONSOLE_SUCCESS_MESSAGE");

                    par_Managers.GetComponent<Manager_UIReuse>().radiation = thePlayer.GetComponent<Player_Health>().radiation;
                    par_Managers.GetComponent<Manager_UIReuse>().maxRadiation = thePlayer.GetComponent<Player_Health>().maxRadiation;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerRadiation();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue >= 1000001)
                {
                    CreateNewConsoleLine("Error: Player max radiation must be set below 1000001!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue < thePlayer.GetComponent<Player_Health>().radiation)
                {
                    CreateNewConsoleLine("Error: Player max radiation cannot be set under radiation!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player stamina recharge
            else if (statName == "staminarecharge")
            {
                if (insertedValue > -1 && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Movement>().staminaRecharge = insertedValue;
                    string staminarecharge = thePlayer.GetComponent<Player_Movement>().staminaRecharge.ToString();
                    CreateNewConsoleLine("Changed player stamina recharge speed to " + staminarecharge + ".", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (insertedValue >= 1000001)
                {
                    CreateNewConsoleLine("Error: Player stamina recharge must be set below 1000001!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue < 0)
                {
                    CreateNewConsoleLine("Error: Player stamina recharge speed must be set over -1!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player speed
            else if (statName == "speed")
            {
                if (insertedValue > -1 && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Movement>().speedIncrease = insertedValue;
                    string speed = thePlayer.GetComponent<Player_Movement>().speedIncrease.ToString();
                    CreateNewConsoleLine("Changed player speed to " + speed + ".", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (insertedValue >= 1000001)
                {
                    CreateNewConsoleLine("Error: Player speed must be set below 1000001!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue < 0)
                {
                    CreateNewConsoleLine("Error: Player speed must be set over -1!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player jump height
            else if (statName == "jumpheight")
            {
                if (insertedValue > -1 && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Movement>().jumpHeight = insertedValue;
                    string jumpheight = thePlayer.GetComponent<Player_Movement>().jumpHeight.ToString();
                    CreateNewConsoleLine("Changed player jumpheight to " + jumpheight + ".", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (insertedValue >= 1000001)
                {
                    CreateNewConsoleLine("Error: Player jump height must be set below 1000001!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue < 0)
                {
                    CreateNewConsoleLine("Error: Player jump height must be set over -1!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player max inv space
            else if (statName == "maxinvspace")
            {
                foreach (GameObject item in thePlayer.GetComponent<Inv_Player>().inventory)
                {
                    invSpace += item.GetComponent<Env_Item>().int_ItemWeight;
                }

                if (insertedValue > invSpace && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Inv_Player>().maxInvSpace = insertedValue;
                    string maxinvspace = thePlayer.GetComponent<Inv_Player>().maxInvSpace.ToString();
                    //update player invspace
                    thePlayer.GetComponent<Inv_Player>().invSpace = insertedValue;
                    //remove each items weight from player invspace
                    foreach (GameObject item in thePlayer.GetComponent<Inv_Player>().inventory)
                    {
                        int itemWeight = item.GetComponent<Env_Item>().int_ItemWeight;
                        thePlayer.GetComponent<Inv_Player>().invSpace -= itemWeight;
                    }
                    invSpace = 0;
                    CreateNewConsoleLine("Changed player max inv space to " + maxinvspace + ".", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (insertedValue == invSpace)
                {
                    thePlayer.GetComponent<Inv_Player>().maxInvSpace = insertedValue;
                    string maxinvspace = thePlayer.GetComponent<Inv_Player>().maxInvSpace.ToString();
                    thePlayer.GetComponent<Inv_Player>().invSpace = 0;
                    invSpace = 0;
                    CreateNewConsoleLine("Changed player max inv space to " + maxinvspace + ".", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (insertedValue < invSpace)
                {
                    invSpace = 0;
                    CreateNewConsoleLine("Error: Player max inv space cannot be set under current inventory space!", "CONSOLE_ERROR_MESSAGE");
                }
                else if (insertedValue >= 1000001)
                {
                    CreateNewConsoleLine("Error: Player max inv space must be set below 1000001!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            //when changing player money
            else if (statName == "money")
            {
                if (insertedValue > -1)
                {
                    thePlayer.GetComponent<Inv_Player>().money = insertedValue;
                    string newMoney = thePlayer.GetComponent<Inv_Player>().money.ToString();
                    CreateNewConsoleLine("Changed player money to " + newMoney + ".", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (insertedValue < 0)
                {
                    CreateNewConsoleLine("Error: Player money must be set over -1!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else
            {
                CreateNewConsoleLine("Error: Player stat not found! Type player showstats to list all player stats.", "CONSOLE_ERROR_MESSAGE");
            }
        }
    }

    //list all spawnable game items
    private void Command_ShowAllSpawnableItems()
    {
        for (int i = 0; i < itemnames.Count; i++)
        {
            string consoleLine;
            string itemName = itemnames[i];

            foreach (GameObject spawnable in spawnables)
            {
                if (spawnable.name == itemName)
                {
                    selectedItem = spawnable;
                    spawnableItem = spawnable;
                    break;
                }
            }

            int itemWeight = selectedItem.GetComponent<Env_Item>().int_ItemWeight;
            int playerInvSpace = PlayerInventoryScript.invSpace;

            if (itemWeight == 0)
            {
                consoleLine = itemnames[i] + " (" + itemWeight + ") " + "(" + 999 + ")";

                //extra tag behind item name if it cant be stacked
                if (!spawnableItem.GetComponent<Env_Item>().isStackable)
                {
                    consoleLine += " <not stackable>";
                }

                CreateNewConsoleLine(consoleLine, "CONSOLE_INFO_MESSAGE");
            }
            else if (itemWeight > 0)
            {
                int spawnCount = playerInvSpace / itemWeight;
                consoleLine = itemnames[i] + " (" + itemWeight + ") " + "(" + spawnCount + ")";

                //extra tag behind item name if it cant be stacked
                if (!spawnableItem.GetComponent<Env_Item>().isStackable)
                {
                    consoleLine += " <not stackable>";
                }

                CreateNewConsoleLine(consoleLine, "CONSOLE_INFO_MESSAGE");
            }

            spawnableItem = null;
        }
    }
    //list all player inventory items
    private void Command_ShowAllPlayerItems()
    {
        CreateNewConsoleLine("All player inventory items:", "CONSOLE_INFO_MESSAGE");

        for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
        {
            displayableItem = PlayerInventoryScript.inventory[i];
            int itemCount = displayableItem.GetComponent<Env_Item>().int_itemCount;

            string consoleLine = displayableItem.name + " x" + itemCount;

            //shows equippable item current and max durability
            if (displayableItem.GetComponent<Item_Gun>() != null)
            {
                float currentRemainder = displayableItem.GetComponent<Item_Gun>().durability;
                float maxRemainder = displayableItem.GetComponent<Item_Gun>().maxDurability;

                consoleLine += " <" + Mathf.Floor(currentRemainder * 10) / 10 + "/" + maxRemainder + ">";
            }

            //shows reusable consumable current and max remainder
            if (displayableItem.GetComponent<Item_Consumable>() != null)
            {
                float currentRemainder = displayableItem.GetComponent<Item_Consumable>().currentConsumableAmount;
                float maxRemainder = displayableItem.GetComponent<Item_Consumable>().maxConsumableAmount;

                consoleLine += " <"+ Mathf.Floor(currentRemainder * 10) / 10 + "/" + maxRemainder +">";
            }

            //shows extra text that the item isnt stackable
            if (!displayableItem.GetComponent<Env_Item>().isStackable)
            {
                consoleLine += " <not stackable>";
            }
            //shows extra text that the item is protected
            if (displayableItem.GetComponent<Env_Item>().isProtected)
            {
                consoleLine += " <protected>";
            }

            CreateNewConsoleLine(consoleLine, "CONSOLE_INFO_MESSAGE");
        }
    }
    //add item/items to players inventory
    private void Command_AddItem()
    {
        foundDuplicate = false;
        duplicate = null;

        string itemName = separatedWords[2];

        bool isInt = int.TryParse(itemName, out _);
        bool isinsertedValueInt = int.TryParse(separatedWords[3], out _);
        if (isInt)
        {
            CreateNewConsoleLine("Error: Added item name cannot be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isinsertedValueInt)
        {
            CreateNewConsoleLine("Error: Inserted addable item count must be a whole number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInt && isinsertedValueInt)
        {
            insertedValue = int.Parse(separatedWords[3]);

            //if item count is an integer and itemnames contains the name of selected item name
            if (itemnames.Contains(itemName) && insertedValue > 0 && insertedValue < 1000001)
            {
                foreach (GameObject spawnable in spawnables)
                {
                    if (spawnable.name == itemName)
                    {
                        selectedItem = spawnable;
                        spawnableItem = spawnable;
                        break;
                    }
                }

                int spawnableItemWeight = selectedItem.GetComponent<Env_Item>().int_ItemWeight;
                int currentPlayerInvFreeSpace = PlayerInventoryScript.invSpace;
                //enough space to add item to player inventory
                if (spawnableItemWeight * insertedValue <= currentPlayerInvFreeSpace)
                {
                    //check if player inventory has gameobject with same name
                    for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
                    {
                        string itemname = PlayerInventoryScript.inventory[i].name;
                        if (itemname == selectedItem.name
                            && selectedItem.GetComponent<Env_Item>().isStackable)
                        {
                            foundDuplicate = true;
                            duplicate = PlayerInventoryScript.inventory[i];
                            break;
                        }
                    }
                    //if the spawnable item is a duplicate and its a stackable or its not a stackable and the spawnable amount is only 1
                    if (foundDuplicate
                        && (spawnableItem.GetComponent<Env_Item>().isStackable
                        || (!spawnableItem.GetComponent<Env_Item>().isStackable
                        && insertedValue == 1)))
                    {
                        duplicate.GetComponent<Env_Item>().int_itemCount += insertedValue;
                        PlayerInventoryScript.invSpace -= (duplicate.GetComponent<Env_Item>().int_ItemWeight * insertedValue);

                        //assigns new ammo to currently equipped gun if ammo type is same as equipped gun
                        if (duplicate.GetComponent<Item_Ammo>() != null
                            && PlayerInventoryScript.equippedGun != null
                            && duplicate.GetComponent<Item_Ammo>().caseType.ToString()
                            == PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().caseType.ToString())
                        {
                            PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().AssignAmmoType();
                        }

                        RebuildInventoryUI();
                        if (duplicate.GetComponent<Item_Ammo>() != null
                            || duplicate.GetComponent<Item_Grenade>() != null)
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().txt_ammoForGun.text = duplicate.GetComponent<Env_Item>().int_itemCount.ToString();
                        }

                        CreateNewConsoleLine("Successfully added " + insertedValue + " " + selectedItem.name + "(s) to players inventory! Removed " + spawnableItemWeight * insertedValue + " space from players inventory.", "CONSOLE_SUCCESS_MESSAGE");
                    }
                    //if the spawnable item isnt a duplicate and its a stackable or its not a stackable and the spawnable amount is only 1
                    else if (!foundDuplicate 
                             && (spawnableItem.GetComponent<Env_Item>().isStackable
                             || (!spawnableItem.GetComponent<Env_Item>().isStackable
                             && insertedValue == 1)))
                    {
                        GameObject newDuplicate = Instantiate(selectedItem, 
                                                              PlayerInventoryScript.par_PlayerItems.transform.position, 
                                                              Quaternion.identity, 
                                                              PlayerInventoryScript.par_PlayerItems.transform);

                        newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;

                        PlayerInventoryScript.inventory.Add(newDuplicate);
                        PlayerInventoryScript.invSpace -= (newDuplicate.GetComponent<Env_Item>().int_ItemWeight * insertedValue);
                        newDuplicate.GetComponent<Env_Item>().int_itemCount = insertedValue;
                        newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = true;
                        playeritemnames.Add(newDuplicate.GetComponent<Env_Item>().str_ItemName);

                        //assigns new ammo to currently equipped gun if ammo type is same as equipped gun
                        if (PlayerInventoryScript.equippedGun != null
                            && PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>() != null
                            && newDuplicate.GetComponent<Item_Ammo>() != null
                            && newDuplicate.GetComponent<Item_Ammo>().caseType.ToString()
                            == PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().caseType.ToString())
                        {
                            PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().AssignAmmoType();
                        }

                        RebuildInventoryUI();
                        if (newDuplicate.GetComponent<Item_Ammo>() != null
                            || newDuplicate.GetComponent<Item_Grenade>() != null)
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().txt_ammoForGun.text = newDuplicate.GetComponent<Env_Item>().int_itemCount.ToString();
                        }

                        newDuplicate.GetComponent<MeshRenderer>().enabled = false;
                        if (newDuplicate.GetComponent<Rigidbody>() != null)
                        {
                            newDuplicate.GetComponent<Rigidbody>().isKinematic = true;
                        }

                        newDuplicate.GetComponent<Env_Item>().DeactivateItem();

                        CreateNewConsoleLine("Successfully added " + insertedValue + " " + selectedItem.name + "(s) to players inventory! Removed " + spawnableItemWeight * insertedValue + " space from players inventory.", "CONSOLE_SUCCESS_MESSAGE");
                    }
                    //if the spawnable item isnt stackable and the player wants to spawn more than one of it
                    else if (!spawnableItem.GetComponent<Env_Item>().isStackable && insertedValue > 1)
                    {
                        remainder = insertedValue;

                        StartCoroutine(SpawnMultipleNonStackables());

                        CreateNewConsoleLine("Successfully added " + insertedValue + " " + selectedItem.name + "(s) to players inventory! Removed " + spawnableItemWeight * insertedValue + " space from players inventory.", "CONSOLE_SUCCESS_MESSAGE");
                    }

                    //update upgrade ui if upgrade cell was added
                    //and if upgrade ui is open
                    if (itemName == "Upgrade_cell"
                        && par_Managers.GetComponent<UI_PlayerMenu>().openedUpgradeUI)
                    {
                        par_Managers.GetComponent<UI_AbilityManager>().LoadUI();
                        par_Managers.GetComponent<UI_AbilityManager>().ShowUpgradeButtonPositions();
                    }
                }
                else if (spawnableItemWeight * insertedValue > currentPlayerInvFreeSpace)
                {
                    CreateNewConsoleLine("Error: Not enough inventory space to add " + selectedItem.name + " to players inventory!", "CONSOLE_ERROR_MESSAGE");
                }
            }
            else if (!itemnames.Contains(itemName))
            {
                CreateNewConsoleLine("Error: Item not found! Type sasi to display all spawnable items.", "CONSOLE_ERROR_MESSAGE");
            }
            else if (insertedValue <= 0)
            {
                CreateNewConsoleLine("Error: Item count must be over 0!", "CONSOLE_ERROR_MESSAGE");
            }
            else if (insertedValue >= 1000001)
            {
                CreateNewConsoleLine("Error: Item count must be less than 1000001!", "CONSOLE_ERROR_MESSAGE");
            }
        }

        selectedItem = null;
    }
    //remove item/items from players inventory
    private void Command_RemoveItem()
    {
        string itemName = separatedWords[2];

        bool isInt = int.TryParse(itemName, out _);
        bool isInsertedValueInt = int.TryParse(separatedWords[3], out _);
        if (isInt)
        {
            CreateNewConsoleLine("Error: Removed item name cannot be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInsertedValueInt)
        {
            CreateNewConsoleLine("Error: Inserted removable item count must be a whole number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInt && isInsertedValueInt)
        {
            insertedValue = int.Parse(separatedWords[3]);

            //find the correct item
            foreach (GameObject removable in PlayerInventoryScript.inventory)
            {
                if (removable.name == separatedWords[2])
                {
                    selectedItem = removable;
                    removableItem = removable;
                    break;
                }
            }
            //if item count is an integer and itemnames contains the name of selected item name
            if (playeritemnames.Contains(separatedWords[2])
                && !selectedItem.GetComponent<Env_Item>().isProtected
                && insertedValue > 0
                && insertedValue < 1000001)
            {
                //cannot delete the equipped gun while it is reloading
                if (selectedItem.GetComponent<Item_Gun>() != null
                    && PlayerInventoryScript.equippedGun == selectedItem
                    && selectedItem.GetComponent<Item_Gun>().isReloading)
                {
                    CreateNewConsoleLine("Error: Can't delete " + selectedItem.GetComponent<Env_Item>().str_ItemName + " through console while it is reloading!", "CONSOLE_ERROR_MESSAGE");
                    canContinueItemRemoval = false;
                }
                else if (selectedItem.GetComponent<Item_Gun>() == null
                         || PlayerInventoryScript.equippedGun != selectedItem
                         || !selectedItem.GetComponent<Item_Gun>().isReloading)
                {
                    canContinueItemRemoval = true;
                }

                if (canContinueItemRemoval)
                {
                    //finds the equipped gun
                    foreach (GameObject item in PlayerInventoryScript.inventory)
                    {
                        if (item.GetComponent<Item_Gun>() != null
                            && PlayerInventoryScript.equippedGun == item)
                        {
                            selectedGun = item;
                            break;
                        }
                    }

                    //cannot delete the equipped guns ammo while the gun is reloading
                    if (selectedGun != null
                        && selectedItem.GetComponent<Item_Ammo>() != null
                        && selectedGun.GetComponent<Item_Gun>().caseType.ToString()
                        == selectedItem.GetComponent<Item_Ammo>().caseType.ToString()
                        && selectedGun.GetComponent<Item_Gun>().isReloading)
                    {
                        CreateNewConsoleLine("Error: Can't delete " + selectedItem.GetComponent<Env_Item>().str_ItemName + " through console while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!", "CONSOLE_ERROR_MESSAGE");
                        canContinueItemRemoval = false;
                    }
                    else if (selectedGun == null
                             || selectedItem.GetComponent<Item_Ammo>() == null
                             || selectedGun.GetComponent<Item_Gun>().caseType.ToString()
                             != selectedItem.GetComponent<Item_Ammo>().caseType.ToString()
                             || !selectedGun.GetComponent<Item_Gun>().isReloading)
                    {
                        canContinueItemRemoval = true;
                    }
                }
                if (canContinueItemRemoval)
                {
                    //if you want to remove all of the selected stackable item or only one of a stackable item
                    if ((insertedValue == selectedItem.GetComponent<Env_Item>().int_itemCount 
                        && removableItem.GetComponent<Env_Item>().isStackable)
                        || (insertedValue == 1
                        && !removableItem.GetComponent<Env_Item>().isStackable))
                    {
                        PlayerInventoryScript.inventory.Remove(selectedItem);
                        PlayerInventoryScript.invSpace += selectedItem.GetComponent<Env_Item>().int_ItemWeight * insertedValue;
                        selectedItem.GetComponent<Env_Item>().isInPlayerInventory = false;

                        if (selectedItem.GetComponent<Item_Battery>() != null
                            && PlayerInventoryScript.equippedFlashlight != null
                            && PlayerInventoryScript.equippedFlashlight.GetComponent<Item_Flashlight>().battery == selectedItem)
                        {
                            PlayerInventoryScript.equippedFlashlight.GetComponent<Item_Flashlight>().RemoveBattery();
                        }

                        ResetAmmoAndGun();

                        RebuildInventoryUI();

                        playeritemnames.Remove(selectedItem.GetComponent<Env_Item>().str_ItemName);

                        int spawnableItemWeight = selectedItem.GetComponent<Env_Item>().int_ItemWeight;
                        CreateNewConsoleLine("Successfully removed " + insertedValue + " " + selectedItem.name + "(s) from players inventory! Added " + spawnableItemWeight * insertedValue + " space back to players inventory.", "CONSOLE_SUCCESS_MESSAGE");

                        Destroy(selectedItem);
                    }
                    //if you want to remove less than all of the selected stackable item
                    else if (insertedValue < selectedItem.GetComponent<Env_Item>().int_itemCount 
                             && removableItem.GetComponent<Env_Item>().isStackable)
                    {
                        PlayerInventoryScript.invSpace += selectedItem.GetComponent<Env_Item>().int_ItemWeight * insertedValue;
                        selectedItem.GetComponent<Env_Item>().int_itemCount -= insertedValue;
                        RebuildInventoryUI();

                        if (selectedItem.GetComponent<Item_Ammo>() != null
                            || selectedItem.GetComponent<Item_Grenade>() != null)
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().txt_ammoForGun.text = selectedItem.GetComponent<Env_Item>().int_itemCount.ToString();
                        }

                        int spawnableItemWeight = selectedItem.GetComponent<Env_Item>().int_ItemWeight;
                        CreateNewConsoleLine("Successfully removed " + insertedValue + " " + selectedItem.name + "(s) from players inventory! Added " + spawnableItemWeight * insertedValue + " space back to players inventory.", "CONSOLE_SUCCESS_MESSAGE");
                    }
                    //if you want to remove more than one of a non-stackable item with the same name
                    else if (!removableItem.GetComponent<Env_Item>().isStackable
                             && insertedValue > 1)
                    {
                        int removableCount = insertedValue;
                        //gets the removableCount of the first removables in the players inventory
                        foreach (GameObject removable in PlayerInventoryScript.inventory)
                        {
                            if (removable.name == removableItem.name && removableCount > 0)
                            {
                                if (removableItem.GetComponent<Item_Battery>() != null
                                    && PlayerInventoryScript.equippedFlashlight != null
                                    && PlayerInventoryScript.equippedFlashlight.GetComponent<Item_Flashlight>().battery == removableItem)
                                {
                                    PlayerInventoryScript.equippedFlashlight.GetComponent<Item_Flashlight>().RemoveBattery();
                                }

                                removables.Add(removable);
                                removableCount--;
                            }
                        }
                        remainder = removables.Count + 1;
                        StartCoroutine(RemoveMultipleNonStackables());

                        int spawnableItemWeight = selectedItem.GetComponent<Env_Item>().int_ItemWeight;
                        CreateNewConsoleLine("Successfully removed " + insertedValue + " " + selectedItem.name + "(s) from players inventory! Added " + spawnableItemWeight * insertedValue + " space back to players inventory.", "CONSOLE_SUCCESS_MESSAGE");
                    }

                    //update upgrade ui if upgrade cell was added
                    //and if upgrade ui is open
                    if (itemName == "Upgrade_cell"
                        && par_Managers.GetComponent<UI_PlayerMenu>().openedUpgradeUI)
                    {
                        par_Managers.GetComponent<UI_AbilityManager>().LoadUI();
                        par_Managers.GetComponent<UI_AbilityManager>().ShowUpgradeButtonPositions();
                    }
                }
                canContinueItemRemoval = true;
            }
            else if (!playeritemnames.Contains(separatedWords[2]))
            {
                CreateNewConsoleLine("Error: Item not found! Type player showallitems to display all spawnable items.", "CONSOLE_ERROR_MESSAGE");
            }
            else if (selectedItem.GetComponent<Env_Item>().isProtected)
            {
                CreateNewConsoleLine("Error: This item is protected and can't be removed from the players inventory!", "CONSOLE_ERROR_MESSAGE");
            }
            else if (insertedValue <= 0)
            {
                CreateNewConsoleLine("Error: Item count must be over 0!", "CONSOLE_ERROR_MESSAGE");
            }
            else if (insertedValue > selectedItem.GetComponent<Env_Item>().int_itemCount)
            {
                CreateNewConsoleLine("Error: Trying to remove too many of this item!", "CONSOLE_ERROR_MESSAGE");
            }
            else if (insertedValue >= 1000001)
            {
                CreateNewConsoleLine("Error: Item count must be less than 1000001!", "CONSOLE_ERROR_MESSAGE");
            }
        }

        selectedItem = null;
    }

    //fixes all repairable items for free
    private void Command_FixAllItems()
    {
        foreach (GameObject repairable in PlayerInventoryScript.inventory)
        {
            if (repairable.GetComponent<Item_Gun>() != null
                && repairable.GetComponent<Item_Gun>().durability 
                < repairable.GetComponent<Item_Gun>().maxDurability)
            {
                foundRepairable = true;
            }
        }

        if (foundRepairable)
        {
            for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
            {
                displayableItem = PlayerInventoryScript.inventory[i];

                if (displayableItem.GetComponent<Item_Gun>() != null)
                {
                    displayableItem.GetComponent<Item_Gun>().durability = displayableItem.GetComponent<Item_Gun>().maxDurability; 

                    if (displayableItem.GetComponent<Item_Gun>().hasEquippedGun)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().durability = displayableItem.GetComponent<Item_Gun>().durability;
                        par_Managers.GetComponent<Manager_UIReuse>().maxDurability = displayableItem.GetComponent<Item_Gun>().maxDurability;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdateWeaponQuality();
                    }

                    CreateNewConsoleLine("Fully repaired " + displayableItem.GetComponent<Env_Item>().str_ItemName + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
            }
        }
        else if (!foundRepairable)
        {
            CreateNewConsoleLine("Error: No repairable items were found in the players inventory or all items are already fully repaired!", "CONSOLE_ERROR_MESSAGE");
        }
        foundRepairable = false;
    }

    //set player reputation with a faction
    private void Command_SetPlayerRep()
    {
        string factionName = separatedWords[2];
        bool isInt = int.TryParse(factionName, out _);
        bool isInsertedValueInt = int.TryParse(separatedWords[3], out _);

        if (isInt)
        {
            CreateNewConsoleLine("Error: Faction name cannot be a number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInsertedValueInt)
        {
            CreateNewConsoleLine("Error: Inserted value must be a whole number!", "CONSOLE_ERROR_MESSAGE");
        }
        else if (!isInt && isInsertedValueInt)
        {
            int value = int.Parse(separatedWords[3]);
            if (value > -1001 && value < 1001)
            {
                if (factionName == "Scientists")
                {
                    par_Managers.GetComponent<Manager_FactionReputation>().vsScientists = value;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                    CreateNewConsoleLine("Changed player and " + factionName + " reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (factionName == "Geifers")
                {
                    par_Managers.GetComponent<Manager_FactionReputation>().vsGeifers = value;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                    CreateNewConsoleLine("Changed player and " + factionName + " reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (factionName == "Annies")
                {
                    par_Managers.GetComponent<Manager_FactionReputation>().vsAnnies = value;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                    CreateNewConsoleLine("Changed player and " + factionName + " reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (factionName == "Verbannte")
                {
                    par_Managers.GetComponent<Manager_FactionReputation>().vsVerbannte = value;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                    CreateNewConsoleLine("Changed player and " + factionName + " reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (factionName == "Raiders")
                {
                    par_Managers.GetComponent<Manager_FactionReputation>().vsRaiders = value;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                    CreateNewConsoleLine("Changed player and " + factionName + " reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (factionName == "Military")
                {
                    par_Managers.GetComponent<Manager_FactionReputation>().vsMilitary = value;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                    CreateNewConsoleLine("Changed player and " + factionName + " reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (factionName == "Verteidiger")
                {
                    par_Managers.GetComponent<Manager_FactionReputation>().vsVerteidiger = value;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                    CreateNewConsoleLine("Changed player and " + factionName + " reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
                else if (factionName == "Others")
                {
                    par_Managers.GetComponent<Manager_FactionReputation>().vsOthers = value;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                    CreateNewConsoleLine("Changed player and " + factionName + " reputation to " + value + "!", "CONSOLE_SUCCESS_MESSAGE");
                }
            }
            else if (value <= -1001)
            {
                CreateNewConsoleLine("Error: Inserted reputation value must be higher than -1001!", "CONSOLE_ERROR_MESSAGE");
            }
            else if (value >= 1001)
            {
                CreateNewConsoleLine("Error: Inserted reputation value must be lower than 1001!", "CONSOLE_ERROR_MESSAGE");
            }
        }
    }

    private void RebuildInventoryUI()
    {
        if (PlayerInventoryScript.isPlayerInventoryOpen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
        }
        else if (PlayerInventoryScript.Container != null && PlayerInventoryScript.Container.GetComponent<Inv_Container>().isContainerInventoryOpen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildContainerInventory();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Container.GetComponent<Inv_Container>().str_ContainerName + " inventory";
        }
        else if (PlayerInventoryScript.Trader != null && PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().isShopOpen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildShopInventory();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().str_ShopName;
        }
        else if (PlayerInventoryScript.Trader != null && PlayerInventoryScript.Trader.GetComponent<UI_RepairContent>().isNPCRepairUIOpen)
        {
            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Trader.GetComponent<UI_AIContent>().str_NPCName + "'s repair shop";
        }
        else if (PlayerInventoryScript.Workbench != null)
        {
            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().str_workbenchName;
        }
        par_Managers.GetComponent<Manager_UIReuse>().UpdateWeaponQuality();
    }

    private void ResetAmmoAndGun()
    {
        //updates player equpped gun and removes ammo
        if (PlayerInventoryScript.equippedGun != null
            && PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>() != null
            && selectedItem.GetComponent<Item_Ammo>().caseType.ToString()
            == PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().caseType.ToString())
        {
            PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().ammoClip = null;
            par_Managers.GetComponent<Manager_UIReuse>().txt_ammoForGun.text = "0";
        }
        //unequips this item if it is a gun
        if (selectedItem.GetComponent<Item_Gun>() != null
            && selectedItem.GetComponent<Item_Gun>().hasEquippedGun)
        {
            selectedItem.GetComponent<Item_Gun>().UnequipGun();

            CreateNewConsoleLine("Unequipped this gun because player removed it from their inventory.", "CONSOLE_INFO_MESSAGE");
        }
        //if the player is removing a gun from their inventory with a clip that isnt empty
        if (selectedItem.GetComponent<Item_Gun>() != null
            && selectedItem.GetComponent<Item_Gun>().currentClipSize > 0)
        {
            int removedAmmo = selectedItem.GetComponent<Item_Gun>().currentClipSize;
            correctAmmo = null;

            //looks for the same ammo from the players inventory as this gun
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (selectedItem.GetComponent<Item_Gun>().caseType.ToString()
                    == item.GetComponent<Item_Ammo>().caseType.ToString())
                {
                    correctAmmo = item;
                    break;
                }
            }

            //finds the guns ammo type and assigns the guns ammo to the ammotype
            if (correctAmmo != null)
            {
                foreach (GameObject item in PlayerInventoryScript.inventory)
                {
                    if (selectedItem.GetComponent<Item_Gun>().caseType.ToString()
                        == item.GetComponent<Item_Ammo>().caseType.ToString())
                    {
                        item.GetComponent<Env_Item>().int_itemCount += removedAmmo;
                        selectedItem.GetComponent<Item_Gun>().currentClipSize = 0;
                        break;
                    }
                }

                CreateNewConsoleLine("Unloaded this gun and added " + removedAmmo + " ammo to existing ammo clip in players inventory.", "CONSOLE_INFO_MESSAGE");
            }
            //if no ammo clip for this gun was found in the players inventory then a new clip is created
            else if (correctAmmo == null)
            {
                GameObject ammo = Instantiate(ammoTemplate, 
                                              PlayerInventoryScript.pos_EquippedItem.position, 
                                              Quaternion.identity);

                ammo.GetComponent<Env_Item>().int_itemCount = removedAmmo;
                ammo.name = ammo.GetComponent<Env_Item>().str_ItemName;
                PlayerInventoryScript.inventory.Add(ammo);
                playeritemnames.Add(ammo.GetComponent<Env_Item>().str_ItemName);

                RebuildInventoryUI();

                CreateNewConsoleLine("Unloaded this gun and added " + removedAmmo + " ammo to new ammo clip in players inventory.", "CONSOLE_INFO_MESSAGE");
            }
        }
        par_Managers.GetComponent<Manager_UIReuse>().UpdateWeaponQuality();
    }

    private IEnumerator SpawnMultipleNonStackables()
    {
        while (remainder > 0)
        {
            GameObject newDuplicate = Instantiate(spawnableItem, 
                                                  PlayerInventoryScript.par_PlayerItems.transform.position, 
                                                  Quaternion.identity, 
                                                  PlayerInventoryScript.par_PlayerItems.transform);

            newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;

            PlayerInventoryScript.inventory.Add(newDuplicate);
            PlayerInventoryScript.invSpace -= (newDuplicate.GetComponent<Env_Item>().int_ItemWeight);
            newDuplicate.GetComponent<Env_Item>().int_itemCount = 1;
            newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = true;
            playeritemnames.Add(newDuplicate.GetComponent<Env_Item>().str_ItemName);

            newDuplicate.GetComponent<MeshRenderer>().enabled = false;
            if (newDuplicate.GetComponent<Rigidbody>() != null)
            {
                newDuplicate.GetComponent<Rigidbody>().isKinematic = true;
            }

            newDuplicate.GetComponent<Env_Item>().DeactivateItem();
            remainder--;
        }
        if (remainder == 0)
        {
            RebuildInventoryUI();
        }
        yield return null;
    }
    private IEnumerator RemoveMultipleNonStackables()
    {
        for (int i = 0; i < removables.Count; i++)
        {
            ResetAmmoAndGun();

            GameObject removable = removables[i];
            removable.name = removable.GetComponent<Env_Item>().str_ItemName;
            PlayerInventoryScript.inventory.Remove(removable);
            PlayerInventoryScript.invSpace += (removable.GetComponent<Env_Item>().int_ItemWeight);
            playeritemnames.Remove(removable.GetComponent<Env_Item>().str_ItemName);
            Destroy(removable);
            remainder--;
        }
        if (remainder == 0)
        {
            removables.Clear();
            RebuildInventoryUI();
        }
        yield return null;
    }

    private void HandleLog(string logString, string unusedStackString, LogType type)
    {
        if (par_Managers != null)
        {
            output = logString;

            if (displayUnityLogs && !startedWait)
            {
                if (lastOutput == output)
                {
                    startedWait = true;
                    StartCoroutine(Wait());
                }

                NewUnitylogMessage();
            }
        }
    }

    private void NewUnitylogMessage()
    {
        CreateNewConsoleLine(output, "UNITY_LOG_MESSAGE");
        lastOutput = output;
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        startedWait = false;
    }
}