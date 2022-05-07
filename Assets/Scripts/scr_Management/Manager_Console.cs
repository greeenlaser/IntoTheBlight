using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public List<GameObject> allCells = new List<GameObject>();
    public List<GameObject> spawnables = new List<GameObject>();

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
    [HideInInspector] private readonly List<string> cellnames = new List<string>();
    [HideInInspector] private readonly List<string> itemnames = new List<string>();
    [HideInInspector] public List<string> playeritemnames = new List<string>();
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
    private string consoleText;
    private readonly char[] separators = new char[] { ' ' };
    private GameObject selectedItem;
    private GameObject selectedGun;
    private GameObject target;
    private GameObject correctAmmo;
    private GameObject displayableItem;
    private GameObject duplicate;
    private readonly List<string> separatedWords = new List<string>();
    private readonly List<GameObject> createdTexts = new List<GameObject>();
    private List<string> insertedCommands = new List<string>();

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
    private List<GameObject> removables = new List<GameObject>();
    private List<string> factionNames = new List<string>();

    //unity log variables
    private bool startedWait;
    private string lastOutput;
    private string output;
    private string stack;

    private void Awake()
    {
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

        factionNames.Add("scientists");
        factionNames.Add("geifers");
        factionNames.Add("annies");
        factionNames.Add("verbannte");
        factionNames.Add("raiders");
        factionNames.Add("military");
        factionNames.Add("verteidiger");
        factionNames.Add("others");

        txt_SelectedTargetName.text = "";

        //start recieving unity logs
        Application.logMessageReceived += HandleLog;

        consoleText = "---GAME VERSION: " + par_Managers.GetComponent<GameManager>().str_GameVersion + "---";
        CreateNewConsoleLine();
        consoleText = "";
        CreateNewConsoleLine();
        consoleText = "---Type help to list all game commands---";
        CreateNewConsoleLine();
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
                        consoleText = "Selected " + hit.collider.name + "!";
                        CreateNewConsoleLine();
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
            consoleText = "Cancelled target selection.";
            CreateNewConsoleLine();
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
        if (!consoleOpen)
        {
            if (!par_Managers.GetComponent<UI_PauseMenu>().isInventoryOpen 
                && !par_Managers.GetComponent<UI_PauseMenu>().isUIOpen 
                && !par_Managers.GetComponent<UI_PauseMenu>().isTalkingToAI
                && !par_Managers.GetComponent<UI_PauseMenu>().isWaitableUIOpen
                && !par_Managers.GetComponent<Manager_GameSaving>().isLoading
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

    //reads inserted text
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

        //if inserted text was not empty and player pressed enter
        if (separatedWords.Count >= 1)
        {
            bool isInt = int.TryParse(separatedWords[0], out _);
            if (isInt)
            {
                insertedCommands.Add(input);
                currentSelectedInsertedCommand = insertedCommands.Count - 1;
                consoleText = "--" + input + "--";
                CreateNewConsoleLine();

                consoleText = "Error: Console command cannot start with a number!";
                CreateNewConsoleLine();
            }
            else if (!isInt)
            {
                //if the first word is help - when the player needs help with a specific command
                if (separatedWords[0] == "help")
                {
                    Command_Help();
                }

                //show the intro message
                else if (separatedWords[0] == "intromessage" && separatedWords.Count == 1)
                {
                    Command_ShowIntroMessage();
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
                else if (separatedWords[0] == "save" && separatedWords.Count == 1
                         && PlayerHealthScript.isPlayerAlive)
                {
                    Command_Save();
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
                    if (separatedWords[0] == "save" && separatedWords.Count == 1
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
                        insertedCommands.Add(input);
                        currentSelectedInsertedCommand = insertedCommands.Count - 1;
                        consoleText = "--" + input + "--";
                        CreateNewConsoleLine();

                        consoleText = "Error: This command has been disabled because the player is dead!";
                        CreateNewConsoleLine();
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
                    //gets all quest ids of the players quests
                    else if (separatedWords.Count == 2 && separatedWords[1] == "getallquestids")
                    {
                        Command_GetAllPlayerQuestIDs();
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
                            consoleText = "--" + input + "--";
                            CreateNewConsoleLine();

                            consoleText = "Error: This command has been disabled because the player is dead!";
                            CreateNewConsoleLine();
                        }
                    }

                    else
                    {
                        insertedCommands.Add(input);
                        currentSelectedInsertedCommand = insertedCommands.Count - 1;
                        consoleText = "--" + input + "--";
                        CreateNewConsoleLine();

                        consoleText = "Error: Unknown or incorrect command!";
                        CreateNewConsoleLine();
                    }
                }

                //get or set a quest value
                else if (separatedWords[0] == "quest")
                {
                    insertedCommands.Add(input);
                    currentSelectedInsertedCommand = insertedCommands.Count - 1;
                    consoleText = "--" + input + "--";
                    CreateNewConsoleLine();

                    consoleText = "Error: Unknown or incorrect command!";
                    CreateNewConsoleLine();
                }

                //get or set a graphics value
                else if (separatedWords[0] == "graphics")
                {
                    insertedCommands.Add(input);
                    currentSelectedInsertedCommand = insertedCommands.Count - 1;
                    consoleText = "--" + input + "--";
                    CreateNewConsoleLine();

                    consoleText = "Error: Unknown or incorrect command!";
                    CreateNewConsoleLine();
                }

                else
                {
                    insertedCommands.Add(input);
                    currentSelectedInsertedCommand = insertedCommands.Count - 1;
                    consoleText = "--" + input + "--";
                    CreateNewConsoleLine();

                    consoleText = "Error: Unknown or incorrect command!";
                    CreateNewConsoleLine();
                }
            }
        }

        else if (separatedWords.Count == 0)
        {
            insertedCommands.Add(input);
            currentSelectedInsertedCommand = insertedCommands.Count - 1;
            consoleText = "--" + input + "--";
            CreateNewConsoleLine();

            consoleText = "Error: No command was inserted! Type help to list all commands.";
            CreateNewConsoleLine();
        }

        else
        {
            insertedCommands.Add(input);
            currentSelectedInsertedCommand = insertedCommands.Count - 1;
            consoleText = "--" + input + "--";
            CreateNewConsoleLine();

            consoleText = "Error: Unknown or incorrect command!";
            CreateNewConsoleLine();
        }

        separatedWords.Clear();

        input = "";
    }

    //create a new text object
    private void CreateNewConsoleLine()
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

            newConsoleText.transform.SetParent(par_Managers.GetComponent<Manager_UIReuse>().par_Content.transform, false);
            newConsoleText.GetComponent<TMP_Text>().text = consoleText;
        }
    }

    private void Command_Help()
    {
        insertedCommands.Add(input);
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        //lists all console commands
        if (separatedWords.Count == 1)
        {
            consoleText = "intromessage - Displays the default intro message that pops up whenever the player first opens up the console.";
            CreateNewConsoleLine();
            consoleText = "tdm - Toggles the Debug menu on and off.";
            CreateNewConsoleLine();
            consoleText = "tul - Toggles the Unity logs on and off.";
            CreateNewConsoleLine();
            consoleText = "save - Saves current game progress.";
            CreateNewConsoleLine();
            consoleText = "restart - Loads the newest save if saves exist or restarts the game from the beginning.";
            CreateNewConsoleLine();
            consoleText = "das - deletes all the game saves - WARNING: All deleted saves are lost forever!";
            CreateNewConsoleLine();
            consoleText = "tgm - toggles godmode for player.";
            CreateNewConsoleLine();
            consoleText = "tnc - toggles noclip for player.";
            CreateNewConsoleLine();
            consoleText = "taid - toggles ai detection for player.";
            CreateNewConsoleLine();
            consoleText = "gcr - Global cell reset.";
            CreateNewConsoleLine();
            consoleText = "clear - Clears the console log.";
            CreateNewConsoleLine();
            consoleText = "quit - Quits the game.";
            CreateNewConsoleLine();

            consoleText = "--------";
            CreateNewConsoleLine();

            consoleText = "st - select target - Hides and disables the console UI until player selects a target.";
            CreateNewConsoleLine();
            consoleText = "target ... - Special command which must have more words after it - use target showinfo to show target states and commands.";
            CreateNewConsoleLine();
            consoleText = "target showinfo - Shows target states and commands.";
            CreateNewConsoleLine();
            consoleText = "sasi - shows all items that can be spawned.";
            CreateNewConsoleLine();
            consoleText = "showfactions - Shows all the game factions.";
            CreateNewConsoleLine();
            consoleText = "setrep faction1 faction2 repValue - Changes the reputation between faction1 and faction2 to repValue.";
            CreateNewConsoleLine();
            consoleText = "tp xValue yValue zValue - Teleports the player to xValue, yValue and zValue coordinates.";
            CreateNewConsoleLine();
            consoleText = "sac - Shows all the games cells.";
            CreateNewConsoleLine();
            consoleText = "dac - Enables all the games cells.";
            CreateNewConsoleLine();
            consoleText = "tpcell cellName - Teleports the player to cell cellName.";
            CreateNewConsoleLine();

            consoleText = "--------";
            CreateNewConsoleLine();

            consoleText = "help player - more commands related to the player";
            CreateNewConsoleLine();
            consoleText = "help quest - more commands related to quests";
            CreateNewConsoleLine();
            consoleText = "help graphics - more commands related to graphics";
            CreateNewConsoleLine();
        }

        //displays specifics about each command
        else if (separatedWords.Count == 2)
        {
            //get which command player needed help with
            string helpCommand = separatedWords[1];

            //if player wants to know about player-related commands
            if (helpCommand == "player")
            {
                consoleText = "Gets or sets a value of the player:";
                CreateNewConsoleLine();
                consoleText = "player currcoords - gets the current location of the player coordinates.";
                CreateNewConsoleLine();
                consoleText = "player showstats - shows all player stats.";
                CreateNewConsoleLine();
                consoleText = "player resetstats - resets all player stats to their original values.";
                CreateNewConsoleLine();
                consoleText = "player setstat statName statValue - sets statName to statValue.";
                CreateNewConsoleLine();
                consoleText = "player setrep faction repValue - sets the reputation between player and faction to factionValue";
                CreateNewConsoleLine();
                consoleText = "player showallitems - shows all player items.";
                CreateNewConsoleLine();
                consoleText = "player additem itemName count - adds count of itemName to players inventory.";
                CreateNewConsoleLine();
                consoleText = "player removeitem itemName count - removes count of itemName from players inventory - ITEM WILL BE DELETED!";
                CreateNewConsoleLine();
                consoleText = "player fixallitems - fixes all repairable items for free.";
                CreateNewConsoleLine();
                consoleText = "player getallquestids - gets all players quest IDs <NOT YET FUNCTIONAL>.";
            }

            //--------

            //if player wants to know about quest-related commands
            else if (helpCommand == "quest")
            {
                consoleText = "quest getquestid questName - gets the ID of questName <NOT YET FUNCTIONAL>.";
                CreateNewConsoleLine();
                consoleText = "quest questID getallstages - gets all the stages of questID <NOT YET FUNCTIONAL>.";
                CreateNewConsoleLine();
                consoleText = "quest questID setstage stageValue - sets the stage of questID to stageValue <NOT YET FUNCTIONAL>.";
                CreateNewConsoleLine();
            }

            //--------

            //if player wants to know about quest-related commands
            else if (helpCommand == "graphics")
            {
                consoleText = "<GRAPHICS COMMANDS COMING IN VERSION 0.6>";
                CreateNewConsoleLine();
            }
        }

        //--------

        else
        {
            consoleText = "Error: Unknown or incorrect command!";
            CreateNewConsoleLine();
        }

        separatedWords.Clear();
    }

    //shows the console intro message
    private void Command_ShowIntroMessage()
    {
        insertedCommands.Add("intromessage");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        consoleText = "---GAME VERSION: " + par_Managers.GetComponent<GameManager>().str_GameVersion + "---";
        CreateNewConsoleLine();

        consoleText = "";
        CreateNewConsoleLine();
        consoleText = "---TYPE HELP FOR COMMANDS---";
        CreateNewConsoleLine();

        separatedWords.Clear();
    }
    //toggles the debug menu on and off
    private void Command_ToggleDebugMenu()
    {
        insertedCommands.Add("tdm"); 
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        if (!displayDebugMenu)
        {
            par_DebugUI.transform.localPosition = new Vector3(0, 0, 0);
            consoleText = "Showing Debug menu.";
            CreateNewConsoleLine();
            displayDebugMenu = true;
        }
        else if (displayDebugMenu)
        {
            par_DebugUI.transform.localPosition = new Vector3(0, 300, 0);
            consoleText = "No longer showing Debug menu.";
            CreateNewConsoleLine();
            displayDebugMenu = false;
        }
        separatedWords.Clear();
    }
    //saves the current game progress
    private void Command_Save()
    {
        insertedCommands.Add("save");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--save--";
        CreateNewConsoleLine();

        par_Managers.GetComponent<UI_PauseMenu>().Save();

        separatedWords.Clear();
    }
    //loads newest save if save was found, otherwise restarts scene
    private void Command_Restart()
    {
        insertedCommands.Add("restart");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--restart--";
        CreateNewConsoleLine();

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
        else
        {
            SceneManager.LoadScene(1);
        }

        separatedWords.Clear();
    }
    //deletes all saves
    private void Command_DeleteAllSaves()
    {
        insertedCommands.Add("das");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

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

                consoleText = "Successfully deleted all save files from " + path + "!";
                CreateNewConsoleLine();
            }
            else
            {
                consoleText = "Error: " + path + " has no save files to delete!";
                CreateNewConsoleLine();
            }
        }

        separatedWords.Clear();
    }
    //toggles the unity logs on and off
    private void Command_ToggleUnityLogs()
    {
        insertedCommands.Add("tul");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        if (!displayUnityLogs)
        {
            consoleText = "Showing Unity logs.";
            CreateNewConsoleLine();
            displayUnityLogs = true;
        }

        else if (displayUnityLogs)
        {
            consoleText = "No longer showing Unity logs.";
            CreateNewConsoleLine();
            displayUnityLogs = false;
        }
        separatedWords.Clear();
    }
    //adds time in hours
    private void Command_GlobalCellReset()
    {
        insertedCommands.Add("gcr");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--gcr--";
        CreateNewConsoleLine();

        par_Managers.GetComponent<GameManager>().GlobalCellReset();

        separatedWords.Clear();
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
        consoleText = "Bye.";
        CreateNewConsoleLine();
        Application.Quit();
    }

    private void Command_SelectTarget()
    {
        insertedCommands.Add("selecttarget");
        currentSelectedInsertedCommand = insertedCommands.Count - 1;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();
        consoleText = "Selecting target - Click on any object on screen or press ESC to cancel selection and to return back to console.";
        CreateNewConsoleLine();

        txt_SelectedTargetName.text = "Click on a GameObject to select it as a target.";
        par_Managers.GetComponent<Manager_UIReuse>().par_Console.transform.localPosition = new Vector3(0, -3000, 0);

        par_Managers.GetComponent<Manager_UIReuse>().txt_InsertedTextSlot.DeactivateInputField();

        isSelectingTarget = true;

        separatedWords.Clear();
    }

    private void Command_EditTarget()
    {
        insertedCommands.Add(input);
        currentSelectedInsertedCommand = insertedCommands.Count - 1;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

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
                        consoleText = "--- target states:";
                        CreateNewConsoleLine();

                        if (target.GetComponent<AI_Health>() != null)
                        {
                            //killableState
                            if (target.GetComponent<AI_Health>().isKillable)
                            {
                                consoleText = "canBeKilled = true";
                                CreateNewConsoleLine();
                            }
                            else if (!target.GetComponent<AI_Health>().isKillable)
                            {
                                consoleText = "canBeKilled = false";
                                CreateNewConsoleLine();
                            }
                            //hostileState
                            if (target.GetComponent<AI_Health>().canBeHostile)
                            {
                                consoleText = "canBeHostile = true";
                                CreateNewConsoleLine();
                            }
                            else if (!target.GetComponent<AI_Health>().canBeHostile)
                            {
                                consoleText = "canBeHostile = false";
                                CreateNewConsoleLine();
                            }
                        }
                        else if (target.GetComponent<AI_Health>() == null)
                        {
                            consoleText = "canBeKilled = false";
                            CreateNewConsoleLine();
                            consoleText = "canBeHostile = false";
                            CreateNewConsoleLine();
                        }
                        consoleText = "factionName = " + target.GetComponent<UI_AIContent>().faction.ToString();
                        CreateNewConsoleLine();

                        consoleText = "--- target commands:";
                        CreateNewConsoleLine();

                        if (target.GetComponent<AI_Health>() != null)
                        {
                            consoleText = "target kill - kills the target if it is not protected.";
                            CreateNewConsoleLine();
                            consoleText = "target sethostilestate hostileStateValue - sets target hostile state to either 0 or 1. 0 means target is no longer hostile towards anyone who attacks it, 1 means targets original battle rules have been enabled";
                            CreateNewConsoleLine();
                            consoleText = "target setkillablestate killableStateValue - sets target killable state to either 0 or 1. 0 means target is no longer killable, 1 means target is killable again. protected npc/monsters are permanently unkillable and cannot be set to killable";
                            CreateNewConsoleLine();
                        }

                        consoleText = "target setfaction factionName - changes targets faction to factionName";
                        CreateNewConsoleLine();
                    }
                    //item states and commands
                    else if (target.GetComponent<Env_Item>() != null)
                    {
                        consoleText = "--- target states:";
                        CreateNewConsoleLine();

                        //is item protected
                        if (target.GetComponent<Env_Item>().isProtected)
                        {
                            consoleText = "isProtected = true";
                            CreateNewConsoleLine();
                        }
                        else if (!target.GetComponent<Env_Item>().isProtected)
                        {
                            consoleText = "isProtected = false";
                            CreateNewConsoleLine();
                        }
                        //is item stackable
                        if (target.GetComponent<Env_Item>().isStackable)
                        {
                            consoleText = "isStackable = true";
                            CreateNewConsoleLine();
                        }
                        else if (!target.GetComponent<Env_Item>().isStackable)
                        {
                            consoleText = "isStackable = false";
                            CreateNewConsoleLine();
                        }

                        consoleText = "itemCount = " + target.GetComponent<Env_Item>().int_itemCount;
                        CreateNewConsoleLine();
                        consoleText = "itemValue = " + target.GetComponent<Env_Item>().int_ItemValue + " (" + target.GetComponent<Env_Item>().int_ItemValue * target.GetComponent<Env_Item>().int_itemCount + ")";
                        CreateNewConsoleLine();
                        consoleText = "itemWeight = " + target.GetComponent<Env_Item>().int_ItemWeight  + " (" + target.GetComponent<Env_Item>().int_ItemWeight * target.GetComponent<Env_Item>().int_itemCount + ")";
                        CreateNewConsoleLine();
                        if (target.GetComponent<Item_Gun>() != null)
                        {
                            consoleText = "currentDurability = " + target.GetComponent<Item_Gun>().durability;
                            CreateNewConsoleLine();
                            consoleText = "maxDurability = " + target.GetComponent<Item_Gun>().maxDurability;
                            CreateNewConsoleLine();
                        }
                        else if (target.GetComponent<Item_Melee>() != null)
                        {
                            consoleText = "currentdurability = " + target.GetComponent<Item_Melee>().durability;
                            CreateNewConsoleLine();
                            consoleText = "maxdurability = " + target.GetComponent<Item_Melee>().maxDurability;
                            CreateNewConsoleLine();
                        }

                        consoleText = "--- target commands:";
                        CreateNewConsoleLine();

                        //default modifiable variables for all gameobjects
                        consoleText = "target disable - disables gameobject if it isn't protected";
                        CreateNewConsoleLine();
                        consoleText = "target enable - enables gameobject if new gameobject hasn't been yet selected or console hasn't been yet closed";
                        CreateNewConsoleLine();

                        consoleText = "target setcount countValue - changes the item count to countValue if it is stackable";
                        CreateNewConsoleLine();
                        consoleText = "target setvalue valueValue - changes individual item value to valueValue";
                        CreateNewConsoleLine();
                        consoleText = "target setweight weightValue - changes individual item weight to weightValue";
                        CreateNewConsoleLine();
                        if (target.GetComponent<Item_Gun>() != null
                            || target.GetComponent<Item_Melee>() != null)
                        {
                            consoleText = "target setdurability durabilityValue - changes individual item durability to durabilityValue";
                            CreateNewConsoleLine();
                            consoleText = "target setmaxdurability maxDurabilityValue - changes individual item max durability to maxDurabilityValue";
                            CreateNewConsoleLine();
                        }
                    }
                    //door/container states and commands
                    else if (target.name == "door_interactable"
                             || target.GetComponent<Inv_Container>() != null)
                    {
                        consoleText = "--- target states:";
                        CreateNewConsoleLine();

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
                            if (door.GetComponent<Env_Door>().isProtected)
                            {
                                consoleText = "isprotected = true";
                                CreateNewConsoleLine();
                            }
                            else if (!door.GetComponent<Env_Door>().isProtected)
                            {
                                consoleText = "isprotected = false";
                                CreateNewConsoleLine();
                            }

                            if (door.GetComponent<Env_Door>().isLocked)
                            {
                                consoleText = "islocked = true";
                                CreateNewConsoleLine();
                            }
                            else if (!door.GetComponent<Env_Door>().isLocked)
                            {
                                consoleText = "islocked = false";
                                CreateNewConsoleLine();
                            }
                        }
                        //is container locked and protected or not
                        else if (container.GetComponent<Inv_Container>() != null)
                        {
                            if (container.GetComponent<Inv_Container>().isProtected)
                            {
                                consoleText = "isprotected = true";
                                CreateNewConsoleLine();
                            }
                            else if (!container.GetComponent<Inv_Container>().isProtected)
                            {
                                consoleText = "isprotected = false";
                                CreateNewConsoleLine();
                            }

                            if (container.GetComponent<Inv_Container>().isLocked)
                            {
                                consoleText = "islocked = true";
                                CreateNewConsoleLine();
                            }
                            else if (!container.GetComponent<Inv_Container>().isLocked)
                            {
                                consoleText = "islocked = false";
                                CreateNewConsoleLine();
                            }
                        }

                        consoleText = "--- target commands:";
                        CreateNewConsoleLine();

                        consoleText = "target unlock - unlocks the door/container if it isn't protected";
                        CreateNewConsoleLine();
                    }
                    else
                    {
                        consoleText = "No commands found for selected target.";
                        CreateNewConsoleLine();
                    }
                }

                //disable target
                else if (secondCommandName == "disable"
                    && !secondCommand)
                {
                    //selected AI
                    if (target.GetComponent<UI_AIContent>() != null)
                    {
                        consoleText = "Disabled " + target.GetComponent<UI_AIContent>().str_NPCName + ".";
                        CreateNewConsoleLine();
                        target.SetActive(false);
                    }
                    //selected non-protected interactable item
                    else if (target.GetComponent<Env_Item>() != null
                             && !target.GetComponent<Env_Item>().isProtected)
                    {
                        consoleText = "Disabled " + target.GetComponent<Env_Item>().str_ItemName + ".";
                        CreateNewConsoleLine();
                        target.SetActive(false);
                    }
                    else
                    {
                        consoleText = "Error: " + target.name + " cannot be disabled! Please select another one.";
                        CreateNewConsoleLine();
                    }
                }
                //enable target if same target is disabled and still selected
                else if (secondCommandName == "enable"
                         && !secondCommand)
                {
                    //selected AI
                    if (target.GetComponent<UI_AIContent>() != null)
                    {
                        consoleText = "Enabled " + target.GetComponent<UI_AIContent>().str_NPCName + ".";
                        CreateNewConsoleLine();
                        target.SetActive(true);
                    }
                    else
                    {
                        consoleText = "Error: " + target.name + " is already enabled!";
                        CreateNewConsoleLine();
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
                        consoleText = "Killed " + target.GetComponent<UI_AIContent>().str_NPCName + " through console.";
                        CreateNewConsoleLine();
                    }

                    //custom kill errors
                    else if (target.GetComponent<UI_AIContent>() == null)
                    {
                        consoleText = "Error: Target is not a killable GameObject!";
                        CreateNewConsoleLine();
                    }
                    else if ((target.GetComponent<UI_AIContent>() != null
                             && target.GetComponent<AI_Health>() != null
                             && !target.GetComponent<AI_Health>().isKillable)
                             || (target.GetComponent<UI_AIContent>() != null
                             && target.GetComponent<AI_Health>() == null))
                    {
                        consoleText = "Error: Target cannot be killed through console because it is protected!";
                        CreateNewConsoleLine();
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
                            consoleText = "Unlocked this door.";
                            CreateNewConsoleLine();
                        }

                        //custom unlock errors
                        else if (!door.GetComponent<Env_Door>().isLocked
                                 || door.GetComponent<Env_Lock>() == null)
                        {
                            consoleText = "Error: Target is already unlocked!";
                            CreateNewConsoleLine();
                        }
                        else if (door.GetComponent<Env_Door>().isProtected)
                        {
                            consoleText = "Error: Target cannot be unlocked through console because it is protected!";
                            CreateNewConsoleLine();
                        }
                    }
                    else if (container != null)
                    {
                        if (container.GetComponent<Inv_Container>().isLocked
                            && !container.GetComponent<Inv_Container>().isProtected)
                        {
                            container.GetComponent<Inv_Container>().isLocked = false;
                            consoleText = "Unlocked this container.";
                            CreateNewConsoleLine();
                        }

                        //custom unlock errors
                        else if (!container.GetComponent<Inv_Container>().isLocked
                                 || container.GetComponent<Env_Lock>() == null)
                        {
                            consoleText = "Error: Target is already unlocked!";
                            CreateNewConsoleLine();
                        }
                        else if (container.GetComponent<Inv_Container>().isProtected)
                        {
                            consoleText = "Error: Target cannot be unlocked through console because it is protected!";
                            CreateNewConsoleLine();
                        }
                    }
                    else
                    {
                        consoleText = "Error: Target is not an unlockable GameObject!";
                        CreateNewConsoleLine();
                    }
                }
                //incorrect or unknown command
                else
                {
                    consoleText = "Error: Incorrect or unknown command!";
                    CreateNewConsoleLine();
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

                string factionName = "none";
                foreach (string realFactionName in factionNames)
                {
                    if (thirdCommandName == realFactionName)
                    {
                        factionName = realFactionName;
                    }
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
                            consoleText = "Set " + target.GetComponent<UI_AIContent>().str_NPCName + "'s hostile state to " + thirdCommandName + ". Target is no longer hostile towards anyone.";
                            CreateNewConsoleLine();
                        }
                        //hostile towards others
                        else if (insertedValue == 1)
                        {
                            target.GetComponent<AI_Health>().canBeHostile = true;
                            consoleText = "Set " + target.GetComponent<UI_AIContent>().str_NPCName + "'s hostile state to " + thirdCommandName + ". Target can now be hostile towards attackers again.";
                            CreateNewConsoleLine();
                        }
                    }
                    //set faction for AI
                    else if (secondCommandName == "setfaction"
                             && !secondCommand
                             && !thirdCommand
                             && factionName != "none")
                    {
                        //stupid way to check which role was selected but it works
                        if (factionName == UI_AIContent.Faction.scientists.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.scientists;
                        }
                        else if (factionName == UI_AIContent.Faction.geifers.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.geifers;
                        }
                        else if (factionName == UI_AIContent.Faction.annies.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.annies;
                        }
                        else if (factionName == UI_AIContent.Faction.verbannte.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.verbannte;
                        }
                        else if (factionName == UI_AIContent.Faction.raiders.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.raiders;
                        }
                        else if (factionName == UI_AIContent.Faction.military.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.military;
                        }
                        else if (factionName == UI_AIContent.Faction.verteidiger.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.verteidiger;
                        }
                        else if (factionName == UI_AIContent.Faction.others.ToString())
                        {
                            target.GetComponent<UI_AIContent>().faction = UI_AIContent.Faction.others;
                        }

                        consoleText = "Set " + target.GetComponent<UI_AIContent>().str_NPCName + "'s faction to " + thirdCommandName + ".";
                        CreateNewConsoleLine();
                    }
                    else
                    {
                        consoleText = "Error: Incorrect or unknown command or selected targets value cannot be edited!";
                        CreateNewConsoleLine();
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
                                
                                consoleText = "Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s count to " + insertedValue + ".";
                                CreateNewConsoleLine();
                            }
                            else if (secondCommandName == "setvalue")
                            {
                                target.GetComponent<Env_Item>().int_maxItemValue = insertedValue;

                                consoleText = "Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s value to " + insertedValue + ".";
                                CreateNewConsoleLine();
                            }
                            else if (secondCommandName == "setweight")
                            {
                                target.GetComponent<Env_Item>().int_ItemWeight = insertedValue;

                                consoleText = "Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s weight to " + insertedValue + ".";
                                CreateNewConsoleLine();
                            }
                            else if (secondCommandName == "setdurability")
                            {
                                if (target.GetComponent<Item_Gun>() != null
                                    && insertedValue <= target.GetComponent<Item_Gun>().maxDurability)
                                {
                                    target.GetComponent<Item_Gun>().durability = insertedValue;

                                    consoleText = "Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s durability to " + insertedValue + ".";
                                    CreateNewConsoleLine();
                                }
                                else if (target.GetComponent<Item_Melee>() != null
                                         && insertedValue <= target.GetComponent<Item_Melee>().maxDurability)
                                {
                                    target.GetComponent<Item_Melee>().durability = insertedValue;

                                    consoleText = "Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s durability to " + insertedValue + ".";
                                    CreateNewConsoleLine();
                                }

                                //custom error message for too high durability
                                else if ((target.GetComponent<Item_Gun>() != null
                                         && insertedValue > target.GetComponent<Item_Gun>().maxDurability)
                                         || (target.GetComponent<Item_Melee>() != null
                                         && insertedValue > target.GetComponent<Item_Melee>().maxDurability))
                                {
                                    consoleText = "Error: Target item durability cannot be higher than its max durability!";
                                    CreateNewConsoleLine();
                                }
                                //custom error message for weapon not found
                                else if (target.GetComponent<Item_Gun>() == null
                                         && target.GetComponent<Item_Melee>() == null)
                                {
                                    consoleText = "Error: Targets durability cannot be edited. Target is not an item with any durability!";
                                    CreateNewConsoleLine();
                                }
                            }
                            else if (secondCommandName == "setmaxdurability")
                            {
                                if (target.GetComponent<Item_Gun>() != null
                                    && insertedValue > target.GetComponent<Item_Gun>().durability)
                                {
                                    target.GetComponent<Item_Gun>().maxDurability = insertedValue;

                                    consoleText = "Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s max durability to " + insertedValue + ".";
                                    CreateNewConsoleLine();
                                }
                                else if (target.GetComponent<Item_Melee>() != null
                                         && insertedValue > target.GetComponent<Item_Melee>().durability)
                                {
                                    target.GetComponent<Item_Melee>().maxDurability = insertedValue;

                                    consoleText = "Changed " + target.GetComponent<Env_Item>().str_ItemName + "'s max durability to " + insertedValue + ".";
                                    CreateNewConsoleLine();
                                }

                                //custom error message for too low max durability
                                else if ((target.GetComponent<Item_Gun>() != null
                                         && insertedValue < target.GetComponent<Item_Gun>().durability)
                                         || (target.GetComponent<Item_Melee>() != null
                                         && insertedValue < target.GetComponent<Item_Melee>().durability))
                                {
                                    consoleText = "Error: Target item max durability cannot be lower than its durability!";
                                    CreateNewConsoleLine();
                                }
                                //custom error message for weapon not found
                                else if (target.GetComponent<Item_Gun>() == null
                                         && target.GetComponent<Item_Melee>() == null)
                                {
                                    consoleText = "Error: Targets max durability cannot be edited. Target is not an item with any durability!";
                                    CreateNewConsoleLine();
                                }
                            }

                            else if (secondCommandName == "setcount"
                                     && !target.GetComponent<Env_Item>().isStackable)
                            {
                                consoleText = "Error: Target count cannot be edited because it is not stackable!";
                                CreateNewConsoleLine();
                            }
                            else
                            {
                                consoleText = "Error: Incorrect or unknown command!";
                                CreateNewConsoleLine();
                            }
                        }
                        else
                        {
                            consoleText = "Error: Selected targets value is out of range!";
                            CreateNewConsoleLine();
                        }
                    }
                    else
                    {
                        consoleText = "Error: Target values cannot be edited because target is protected!";
                        CreateNewConsoleLine();
                    }
                }

                //incorrect or unknown command
                else
                {
                    consoleText = "Error: Incorrect or unknown command!";
                    CreateNewConsoleLine();
                }
            }
            /*
            else if (separatedWords.Count == 4)
            {
                string secondCommandName = separatedWords[1];
                bool secondCommand = int.TryParse(secondCommandName, out _);
                string thirdCommandName = separatedWords[2];
                bool thirdCommand = int.TryParse(thirdCommandName, out _);
                string fourthCommandName = separatedWords[3];
                bool fourthCommand = int.TryParse(fourthCommandName, out _);
            }
            */
            //incorrect or unknown command
            else
            {
                consoleText = "Error: Incorrect or unknown command!";
                CreateNewConsoleLine();
            }
        }
        else if (target == null)
        {
            consoleText = "Error: No target was selected! Use the selecttarget command to select a target to edit.";
            CreateNewConsoleLine();
        }

        separatedWords.Clear();
    }

    //shows all game factions
    private void Command_ShowFactions()
    {
        insertedCommands.Add("showfactions");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();
        consoleText = "Game factions:";
        CreateNewConsoleLine();

        foreach (string factionName in factionNames)
        {
            consoleText = factionName;
            CreateNewConsoleLine();
        }

        separatedWords.Clear();
    }
    //sets reputation value between two factions
    private void Command_SetRep()
    {
        insertedCommands.Add("setrep " + separatedWords[1] + " " + separatedWords[2] + " " + separatedWords[3]);
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        string factionName1 = separatedWords[1];
        string factionName2 = separatedWords[2];
        bool isInt1 = int.TryParse(factionName1, out _);
        bool isInt2 = int.TryParse(factionName2, out _);
        bool isInsertedValueInt = int.TryParse(separatedWords[3], out _);
        if (isInt1 || isInt2)
        {
            consoleText = "Error: Faction name cannot be a number!";
            CreateNewConsoleLine();
        }
        else if (!isInsertedValueInt)
        {
            consoleText = "Error: Inserted value must be a whole number!";
            CreateNewConsoleLine();
        }
        else if (!isInt1 && !isInt2 && isInsertedValueInt)
        {
            int value = int.Parse(separatedWords[3]);
            if (value > -1001 && value < 1001)
            {
                string confirmedFactionName1 = "none";
                string confirmedFactionName2 = "none";

                foreach (string possibleFactionName1 in factionNames)
                {
                    if (factionName1 == possibleFactionName1)
                    {
                        confirmedFactionName1 = factionName1;

                        foreach (string possibleFactionName2 in factionNames)
                        {
                            if (factionName2 == possibleFactionName2)
                            {
                                confirmedFactionName2 = factionName2;
                                break;
                            }
                        }
                    }
                }
                if (confirmedFactionName1 != "none" && confirmedFactionName2 != "none")
                {
                    //scientists vs others
                    if (confirmedFactionName1 == "scientists" && confirmedFactionName2 == "scientists")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v1 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "scientists" && confirmedFactionName2 == "geifers")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v2 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v1 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "scientists" && confirmedFactionName2 == "annies")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v3 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v1 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "scientists" && confirmedFactionName2 == "verbannte")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v4 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v1 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "scientists" && confirmedFactionName2 == "raiders")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v5 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v1 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "scientists" && confirmedFactionName2 == "military")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v6 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v1 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "scientists" && confirmedFactionName2 == "verteidiger")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v7 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v1 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "scientists" && confirmedFactionName2 == "others")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v8 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v1 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    //geifers vs others
                    else if (confirmedFactionName1 == "geifers" && confirmedFactionName2 == "scientists")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v1 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v2 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "geifers" && confirmedFactionName2 == "geifers")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v2 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "geifers" && confirmedFactionName2 == "annies")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v3 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v2 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "geifers" && confirmedFactionName2 == "verbannte")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v4 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v2 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "geifers" && confirmedFactionName2 == "raiders")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v5 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v2 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "geifers" && confirmedFactionName2 == "military")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v6 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v2 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "geifers" && confirmedFactionName2 == "verteidiger")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v7 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v2 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "geifers" && confirmedFactionName2 == "others")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v8 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v2 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    //annies vs others
                    else if (confirmedFactionName1 == "annies" && confirmedFactionName2 == "scientists")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v1 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v3 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "annies" && confirmedFactionName2 == "geifers")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v2 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v3 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "annies" && confirmedFactionName2 == "annies")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v3 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "annies" && confirmedFactionName2 == "verbannte")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v4 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v3 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "annies" && confirmedFactionName2 == "raiders")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v5 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v3 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "annies" && confirmedFactionName2 == "military")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v6 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v3 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "annies" && confirmedFactionName2 == "verteidiger")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v7 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v3 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "annies" && confirmedFactionName2 == "others")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v8 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v3 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    //verbannte vs others
                    else if (confirmedFactionName1 == "verbannte" && confirmedFactionName2 == "scientists")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v1 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v4 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verbannte" && confirmedFactionName2 == "geifers")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v2 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v4 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verbannte" && confirmedFactionName2 == "annies")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v3 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v4 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verbannte" && confirmedFactionName2 == "verbannte")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v4 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verbannte" && confirmedFactionName2 == "raiders")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v5 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v4 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verbannte" && confirmedFactionName2 == "military")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v6 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v4 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verbannte" && confirmedFactionName2 == "verteidiger")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v7 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v4 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verbannte" && confirmedFactionName2 == "others")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v8 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v4 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    //raiders vs others
                    else if (confirmedFactionName1 == "raiders" && confirmedFactionName2 == "scientists")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v1 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v5 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "raiders" && confirmedFactionName2 == "geifers")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v2 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v5 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "raiders" && confirmedFactionName2 == "annies")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v3 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v5 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "raiders" && confirmedFactionName2 == "verbannte")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v4 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v5 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "raiders" && confirmedFactionName2 == "raiders")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v5 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "raiders" && confirmedFactionName2 == "military")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v6 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v5 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "raiders" && confirmedFactionName2 == "verteidiger")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v7 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v5 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "raiders" && confirmedFactionName2 == "others")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v8 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v5 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    //military vs others
                    else if (confirmedFactionName1 == "military" && confirmedFactionName2 == "scientists")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v1 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v6 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "military" && confirmedFactionName2 == "geifers")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v2 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v6 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "military" && confirmedFactionName2 == "annies")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v3 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v6 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "military" && confirmedFactionName2 == "verbannte")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v4 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v6 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "military" && confirmedFactionName2 == "raiders")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v5 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v6 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "military" && confirmedFactionName2 == "military")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v6 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "military" && confirmedFactionName2 == "verteidiger")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v7 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v6 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "military" && confirmedFactionName2 == "others")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v8 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v6 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    //verteidiger vs others
                    else if (confirmedFactionName1 == "verteidiger" && confirmedFactionName2 == "scientists")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v1 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v7 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verteidiger" && confirmedFactionName2 == "geifers")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v2 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v7 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verteidiger" && confirmedFactionName2 == "annies")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v3 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v7 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verteidiger" && confirmedFactionName2 == "verbannte")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v4 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v7 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verteidiger" && confirmedFactionName2 == "raiders")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v5 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v7 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verteidiger" && confirmedFactionName2 == "military")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v6 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v7 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verteidiger" && confirmedFactionName2 == "verteidiger")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v7 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "verteidiger" && confirmedFactionName2 == "others")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v8 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v7 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    //others vs others
                    else if (confirmedFactionName1 == "others" && confirmedFactionName2 == "scientists")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v1 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v8 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "others" && confirmedFactionName2 == "geifers")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v2 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v8 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "others" && confirmedFactionName2 == "annies")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v3 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v8 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "others" && confirmedFactionName2 == "verbannte")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v4 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v8 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "others" && confirmedFactionName2 == "raiders")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v5 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v8 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "others" && confirmedFactionName2 == "military")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v6 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v8 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "others" && confirmedFactionName2 == "verteidiger")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v7 = value;
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v8 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName1 == "others" && confirmedFactionName2 == "others")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v8 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed " + confirmedFactionName1 + " and " + confirmedFactionName2 + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                }
                else if (confirmedFactionName1 == "none")
                {
                    Debug.LogError("Error: First inserted faction name was not found!");
                }
                else if (confirmedFactionName2 == "none")
                {
                    Debug.LogError("Error: Second inserted faction name was not found!");
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
        separatedWords.Clear();
    }
    //allows to teleport to custom location with coordinates
    private void Command_TeleportToVector3Location()
    {
        insertedCommands.Add("tp " + separatedWords[1] + " " + separatedWords[2] + " " + separatedWords[3]);
        currentSelectedInsertedCommand = insertedCommands.Count;
        string secondWord = separatedWords[1];
        string thirdWord = separatedWords[2];
        string fourthWord = separatedWords[3];

        bool firstFloatCorrect = float.TryParse(secondWord, out float firstVec);
        bool secondFloatCorrect = float.TryParse(thirdWord, out float secondVec);
        bool thirdFloatCorrect = float.TryParse(fourthWord, out float thirdVec);
        if (!firstFloatCorrect)
        {
            consoleText = "Error: Teleport coordinate first input must be a number!";
            CreateNewConsoleLine();
        }
        if (!secondFloatCorrect)
        {
            consoleText = "Error: Teleport coordinate second input must be a number!";
            CreateNewConsoleLine();
        }
        if (!thirdFloatCorrect)
        {
            consoleText = "Error: Teleport coordinate third input must be a number!";
            CreateNewConsoleLine();
        }

        //if all 3 are numbers then assign them as
        //teleport position coordinates and move player to target
        if (firstFloatCorrect && secondFloatCorrect && thirdFloatCorrect)
        {
            if (firstVec >= 1000001 || firstVec <= -1000001)
            {
                consoleText = "Error: Teleport coordinate first input cannot be higher than 1000000 and lower than -1000000!";
                CreateNewConsoleLine();
            }
            if (secondVec >= 1000001 || secondVec <= -1000001)
            {
                consoleText = "Error: Teleport coordinate second input cannot be higher than 1000000 and lower than -1000000!";
                CreateNewConsoleLine();
            }
            if (thirdVec >= 1000001 || thirdVec <= -1000001)
            {
                consoleText = "Error: Teleport coordinate third input cannot be higher than 1000000 and lower than -1000000!";
                CreateNewConsoleLine();
            }

            else
            {
                //set teleportPos;
                Vector3 teleportPos = new Vector3(firstVec, secondVec, thirdVec);

                consoleText = "--tp " + firstVec + " " + secondVec + " " + thirdVec + "--";
                CreateNewConsoleLine();

                consoleText = "Success: Teleported player to " + teleportPos + "!";
                CreateNewConsoleLine();
                thePlayer.transform.position = teleportPos;
                ToggleConsole();

                separatedWords.Clear();
            }
        }
    }
    //list all valid game cell names
    private void Command_ShowAllCells()
    {
        insertedCommands.Add("sac");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();
        consoleText = "All game cells:";
        CreateNewConsoleLine();

        foreach (string cellname in cellnames)
        {
            consoleText = cellname;
            CreateNewConsoleLine();
        }

        separatedWords.Clear();
    }
    //discover all game cell names
    private void Command_DiscoverAllCells()
    {
        insertedCommands.Add("dac");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        foreach (GameObject cell in allCells)
        {
            if (!cell.GetComponent<Manager_CurrentCell>().discoveredCell)
            {
                cell.GetComponent<Manager_CurrentCell>().discoveredCell = true;
                cell.GetComponent<Manager_CurrentCell>().EnableCellTeleportButtonOnMap();
            }
        }

        consoleText = "All cells are now discovered!";
        CreateNewConsoleLine();

        separatedWords.Clear();
    }
    //teleport to specific cell spawn point
    private void Command_TeleportToCell()
    {
        insertedCommands.Add("tpcell " + separatedWords[1]);
        currentSelectedInsertedCommand = insertedCommands.Count;
        string cellName = separatedWords[1];
        consoleText = "--tpcell " + cellName + "--";
        CreateNewConsoleLine();
        bool isInt = int.TryParse(cellName, out _);
        if (isInt)
        {
            consoleText = "Error: Cell name cannot be a number!";
            CreateNewConsoleLine();
        }
        else if (!isInt)
        {
            for (int i = 0; i < allCells.Count; i++)
            {
                GameObject cell = allCells[i];
                GameObject lastCell = allCells[allCells.Count - 1];
                if (cellName == cell.GetComponent<Manager_CurrentCell>().str_CellName)
                {
                    consoleText = "Teleported to cell " + cellName + "!";
                    CreateNewConsoleLine();
                    //get vector3 from valid cell
                    Transform teleportLoc = cell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint;
                    //move player to cell
                    thePlayer.transform.position = teleportLoc.position;
                    //load teleported cell items
                    cell.GetComponent<Manager_CurrentCell>().LoadCell();
                    ToggleConsole();
                    break;
                }
                else if (i == allCells.Count -1
                        && cellName != lastCell.GetComponent<Manager_CurrentCell>().str_CellName)
                {
                    consoleText = "Error: Cell name not found! Use sac to list all valid game cells.";
                    CreateNewConsoleLine();
                }
            }
        }
        separatedWords.Clear();
    }

    //get player current coordinates
    private void Command_GetPlayerCurrentCoordinates()
    {
        if (separatedWords.Count == 2)
        {
            insertedCommands.Add("player currcoords");
            currentSelectedInsertedCommand = insertedCommands.Count;
            Vector3 currCoords = thePlayer.transform.position;

            consoleText = "--" + input + "--";
            CreateNewConsoleLine();
            consoleText = "Player current coordinates are " + currCoords + ".";
            CreateNewConsoleLine();
            separatedWords.Clear();
        }
    }
    //toggles godmode on and off
    private void Command_ToggleGodMode()
    {
        insertedCommands.Add("tgm");
        currentSelectedInsertedCommand = insertedCommands.Count - 1;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();
        if (!thePlayer.GetComponent<Player_Health>().canTakeDamage)
        {
            thePlayer.GetComponent<Player_Health>().canTakeDamage = true;
            consoleText = "Disabled godmode.";
            CreateNewConsoleLine();
        }

        else if (thePlayer.GetComponent<Player_Health>().canTakeDamage)
        {
            thePlayer.GetComponent<Player_Health>().canTakeDamage = false;
            consoleText = "Enabled godmode.";
            CreateNewConsoleLine();
        }
        separatedWords.Clear();
    }
    //toggles noclip on and off
    private void Command_ToggleNoclip()
    {
        insertedCommands.Add("tnc");
        currentSelectedInsertedCommand = insertedCommands.Count - 1;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        if (!thePlayer.GetComponent<Player_Movement>().isNoclipping)
        {
            consoleText = "Enabled noclip.";
            CreateNewConsoleLine();

            thePlayer.GetComponent<Player_Movement>().isClimbingLadder = false;
            thePlayer.GetComponent<Player_Movement>().isNoclipping = true;
        }
        else if (thePlayer.GetComponent<Player_Movement>().isNoclipping)
        {
            consoleText = "Disabled noclip.";
            CreateNewConsoleLine();

            thePlayer.GetComponent<Player_Movement>().isNoclipping = false;
        }

        separatedWords.Clear();
    }
    //toggles AI detection on and off
    private void Command_ToggleAIDetection()
    {
        insertedCommands.Add("taid");
        currentSelectedInsertedCommand = insertedCommands.Count - 1;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();
        if (!toggleAIDetection)
        {
            toggleAIDetection = true;
            consoleText = "Enabled AI detection.";
            CreateNewConsoleLine();
        }

        else if (toggleAIDetection)
        {
            toggleAIDetection = false;
            consoleText = "Disabled AI detection.";
            CreateNewConsoleLine();
        }
        separatedWords.Clear();
    }

    //shows all player stats
    private void Command_ShowPlayerStats()
    {
        insertedCommands.Add("player showstats");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();
        consoleText = "Player stats:";
        CreateNewConsoleLine();

        //get current player health
        string health = thePlayer.GetComponent<Player_Health>().health.ToString();
        consoleText = "health: " + health;
        CreateNewConsoleLine();
        //get players current max health
        string maxHealth = thePlayer.GetComponent<Player_Health>().maxHealth.ToString();
        consoleText = "maxhealth: " + maxHealth;
        CreateNewConsoleLine();
        //get player current stamina
        float stamina = thePlayer.GetComponent<Player_Movement>().currentStamina;
        consoleText = "stamina: " + stamina;
        CreateNewConsoleLine();
        //get player max stamina
        float maxstamina = thePlayer.GetComponent<Player_Movement>().maxStamina;
        consoleText = "maxstamina: " + maxstamina;
        CreateNewConsoleLine();
        //get player current mental state
        float mentalstate = thePlayer.GetComponent<Player_Health>().mentalState;
        consoleText = "mentalstate: " + mentalstate;
        CreateNewConsoleLine();
        //get player max mental state
        float maxmentalstate = thePlayer.GetComponent<Player_Health>().maxMentalState;
        consoleText = "maxmentalstate: " + maxmentalstate;
        CreateNewConsoleLine();
        //get player current radiation
        float radiation = thePlayer.GetComponent<Player_Health>().radiation;
        consoleText = "radiation: " + radiation;
        CreateNewConsoleLine();
        //get player max radiation
        float maxradiation = thePlayer.GetComponent<Player_Health>().maxRadiation;
        consoleText = "maxradiation: " + maxradiation;
        CreateNewConsoleLine();
        //get player stamina recharge speed
        float staminarecharge = thePlayer.GetComponent<Player_Movement>().staminaRecharge;
        consoleText = "staminarecharge: " + staminarecharge;
        CreateNewConsoleLine();
        //get player speed
        float speed = thePlayer.GetComponent<Player_Movement>().speedIncrease;
        consoleText = "speed: " + speed;
        CreateNewConsoleLine();
        //get player jump height
        float jumpheight = thePlayer.GetComponent<Player_Movement>().jumpHeight;
        consoleText = "jumpheight: " + jumpheight;
        CreateNewConsoleLine();
        //get player current inventory space
        int currentSpace = thePlayer.GetComponent<Inv_Player>().invSpace;
        consoleText = "current used inv space: " + currentSpace + " <not editable>";
        CreateNewConsoleLine();
        //get player max inventory space
        int maxInvSpace = thePlayer.GetComponent<Inv_Player>().maxInvSpace;
        consoleText = "maxinvspace: " + maxInvSpace;
        CreateNewConsoleLine();
        //get player current money
        int money = thePlayer.GetComponent<Inv_Player>().money;
        consoleText = "money: " + money;
        CreateNewConsoleLine();

        separatedWords.Clear();
    }
    private void Command_ResetPlayerStats()
    {
        insertedCommands.Add("player resetstats");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

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
            consoleText = "Error: All player stats already are at their original value.";
            CreateNewConsoleLine();
        }
        else
        {
            consoleText = "All modifiable player stats were reset to their original values.";
            CreateNewConsoleLine();

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
        }

        separatedWords.Clear();
    }
    //set a player stat
    private void Command_SetPlayerStat()
    {
        insertedCommands.Add("player setstat " + separatedWords[2] + " " + separatedWords[3]);
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        string statName = separatedWords[2];

        bool isInt = int.TryParse(statName, out _);
        bool isInsertedValueInt = int.TryParse(separatedWords[3], out _);
        if (isInt)
        {
            consoleText = "Error: Player stat name cannot be a number!";
            CreateNewConsoleLine();
        }
        else if (!isInsertedValueInt)
        {
            consoleText = "Error: Inserted value must be a whole number!";
            CreateNewConsoleLine();
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
                    consoleText = "Changed player current health to " + health + ".";
                    CreateNewConsoleLine();

                    par_Managers.GetComponent<Manager_UIReuse>().health = thePlayer.GetComponent<Player_Health>().health;
                    par_Managers.GetComponent<Manager_UIReuse>().maxHealth = thePlayer.GetComponent<Player_Health>().maxHealth;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerHealth();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue > thePlayer.GetComponent<Player_Health>().maxHealth)
                {
                    consoleText = "Error: Player current health cannot be set over player max health!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue < 0)
                {
                    consoleText = "Error: Player current health must be set over -1!";
                    CreateNewConsoleLine();
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
                    consoleText = "Changed player max health to " + maxhealth + ".";
                    CreateNewConsoleLine();

                    par_Managers.GetComponent<Manager_UIReuse>().health = thePlayer.GetComponent<Player_Health>().health;
                    par_Managers.GetComponent<Manager_UIReuse>().maxHealth = thePlayer.GetComponent<Player_Health>().maxHealth;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerHealth();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue < thePlayer.GetComponent<Player_Health>().health)
                {
                    consoleText = "Error: Player max health cannot be set under player current health!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue >= 1000001)
                {
                    consoleText = "Error: Player max health must be set below 1000001!";
                    CreateNewConsoleLine();
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
                    consoleText = "Changed player current stamina to " + stamina + ".";
                    CreateNewConsoleLine();

                    par_Managers.GetComponent<Manager_UIReuse>().stamina = thePlayer.GetComponent<Player_Movement>().currentStamina;
                    par_Managers.GetComponent<Manager_UIReuse>().maxStamina = thePlayer.GetComponent<Player_Movement>().maxStamina;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerStamina();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue < 0)
                {
                    consoleText = "Error: Player current stamina must be set over -1!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue > thePlayer.GetComponent<Player_Movement>().maxStamina)
                {
                    consoleText = "Error: Player current stamina cannot be set over player max stamina!";
                    CreateNewConsoleLine();
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
                    consoleText = "Changed player max stamina to " + maxstamina + ".";
                    CreateNewConsoleLine();

                    par_Managers.GetComponent<Manager_UIReuse>().stamina = thePlayer.GetComponent<Player_Movement>().currentStamina;
                    par_Managers.GetComponent<Manager_UIReuse>().maxStamina = thePlayer.GetComponent<Player_Movement>().maxStamina;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerStamina();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue >= 1000001)
                {
                    consoleText = "Error: Player max stamina must be set below 1000001!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue < thePlayer.GetComponent<Player_Movement>().currentStamina)
                {
                    consoleText = "Error: Player max stamina cannot be set under player current stamina!";
                    CreateNewConsoleLine();
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
                    consoleText = "Changed player current mental state to " + mentalstate + ".";
                    CreateNewConsoleLine();

                    par_Managers.GetComponent<Manager_UIReuse>().mentalState = thePlayer.GetComponent<Player_Health>().mentalState;
                    par_Managers.GetComponent<Manager_UIReuse>().maxMentalState = thePlayer.GetComponent<Player_Health>().maxMentalState;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerMentalState();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue < 0)
                {
                    consoleText = "Error: Player current mental stamina must be set over -1!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue > thePlayer.GetComponent<Player_Health>().maxMentalState)
                {
                    consoleText = "Error: Player current mental state cannot be set over player max mental state!";
                    CreateNewConsoleLine();
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
                    consoleText = "Changed player max mental state to " + maxmentalstate + ".";
                    CreateNewConsoleLine();

                    par_Managers.GetComponent<Manager_UIReuse>().mentalState = thePlayer.GetComponent<Player_Health>().mentalState;
                    par_Managers.GetComponent<Manager_UIReuse>().maxMentalState = thePlayer.GetComponent<Player_Health>().maxMentalState;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerMentalState();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue >= 1000001)
                {
                    consoleText = "Error: Player max mental state must be set below 1000001!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue < thePlayer.GetComponent<Player_Health>().mentalState)
                {
                    consoleText = "Error: Player max mental state cannot be set under mental state!";
                    CreateNewConsoleLine();
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
                    consoleText = "Changed player current radiation to " + radiation + ".";
                    CreateNewConsoleLine();

                    par_Managers.GetComponent<Manager_UIReuse>().radiation = thePlayer.GetComponent<Player_Health>().radiation;
                    par_Managers.GetComponent<Manager_UIReuse>().maxRadiation = thePlayer.GetComponent<Player_Health>().maxRadiation;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerRadiation();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue < 0)
                {
                    consoleText = "Error: Player current radiation must be set over -1!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue > thePlayer.GetComponent<Player_Health>().maxRadiation)
                {
                    consoleText = "Error: Player current radiation cannot be set over player max radiation!";
                    CreateNewConsoleLine();
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
                    consoleText = "Changed player max radiation to " + maxradiation + ".";
                    CreateNewConsoleLine();

                    par_Managers.GetComponent<Manager_UIReuse>().radiation = thePlayer.GetComponent<Player_Health>().radiation;
                    par_Managers.GetComponent<Manager_UIReuse>().maxRadiation = thePlayer.GetComponent<Player_Health>().maxRadiation;
                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerRadiation();

                    par_Managers.GetComponent<UI_PlayerMenuStats>().GetStats();
                }
                else if (insertedValue >= 1000001)
                {
                    consoleText = "Error: Player max radiation must be set below 1000001!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue < thePlayer.GetComponent<Player_Health>().radiation)
                {
                    consoleText = "Error: Player max radiation cannot be set under radiation!";
                    CreateNewConsoleLine();
                }
            }
            //when changing player stamina recharge
            else if (statName == "staminarecharge")
            {
                if (insertedValue > -1 && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Movement>().staminaRecharge = insertedValue;
                    string staminarecharge = thePlayer.GetComponent<Player_Movement>().staminaRecharge.ToString();
                    consoleText = "Changed player stamina recharge speed to " + staminarecharge + ".";
                    CreateNewConsoleLine();
                }
                else if (insertedValue >= 1000001)
                {
                    consoleText = "Error: Player stamina recharge must be set below 1000001!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue < 0)
                {
                    consoleText = "Error: Player stamina recharge speed must be set over -1!";
                    CreateNewConsoleLine();
                }
            }
            //when changing player speed
            else if (statName == "speed")
            {
                if (insertedValue > -1 && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Movement>().speedIncrease = insertedValue;
                    string speed = thePlayer.GetComponent<Player_Movement>().speedIncrease.ToString();
                    consoleText = "Changed player speed to " + speed + ".";
                    CreateNewConsoleLine();
                }
                else if (insertedValue >= 1000001)
                {
                    consoleText = "Error: Player speed must be set below 1000001!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue < 0)
                {
                    consoleText = "Error: Player speed must be set over -1!";
                    CreateNewConsoleLine();
                }
            }
            //when changing player jump height
            else if (statName == "jumpheight")
            {
                if (insertedValue > -1 && insertedValue < 1000001)
                {
                    thePlayer.GetComponent<Player_Movement>().jumpHeight = insertedValue;
                    string jumpheight = thePlayer.GetComponent<Player_Movement>().jumpHeight.ToString();
                    consoleText = "Changed player jumpheight to " + jumpheight + ".";
                    CreateNewConsoleLine();
                }
                else if (insertedValue >= 1000001)
                {
                    consoleText = "Error: Player jump height must be set below 1000001!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue < 0)
                {
                    consoleText = "Error: Player jump height must be set over -1!";
                    CreateNewConsoleLine();
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
                    consoleText = "Changed player max inv space to " + maxinvspace + ".";
                    CreateNewConsoleLine();
                }
                else if (insertedValue == invSpace)
                {
                    thePlayer.GetComponent<Inv_Player>().maxInvSpace = insertedValue;
                    string maxinvspace = thePlayer.GetComponent<Inv_Player>().maxInvSpace.ToString();
                    thePlayer.GetComponent<Inv_Player>().invSpace = 0;
                    invSpace = 0;
                    consoleText = "Changed player max inv space to " + maxinvspace + ".";
                    CreateNewConsoleLine();
                }
                else if (insertedValue < invSpace)
                {
                    invSpace = 0;
                    consoleText = "Error: Player max inv space cannot be set under current inventory space!";
                    CreateNewConsoleLine();
                }
                else if (insertedValue >= 1000001)
                {
                    consoleText = "Error: Player max inv space must be set below 1000001!";
                    CreateNewConsoleLine();
                }
            }
            //when changing player money
            else if (statName == "money")
            {
                if (insertedValue > -1)
                {
                    thePlayer.GetComponent<Inv_Player>().money = insertedValue;
                    string newMoney = thePlayer.GetComponent<Inv_Player>().money.ToString();
                    consoleText = "Changed player money to " + newMoney + ".";
                    CreateNewConsoleLine();
                }
                else if (insertedValue < 0)
                {
                    consoleText = "Error: Player money must be set over -1!";
                    CreateNewConsoleLine();
                }
            }
            else
            {
                consoleText = "Error: Player stat not found! Type player showstats to list all player stats.";
                CreateNewConsoleLine();
            }
        }

        separatedWords.Clear();
    }

    //list all spawnable game items
    private void Command_ShowAllSpawnableItems()
    {
        insertedCommands.Add("sasi");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

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

                consoleText = consoleLine;
                CreateNewConsoleLine();
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

                consoleText = consoleLine;
                CreateNewConsoleLine();
            }

            spawnableItem = null;
        }
        separatedWords.Clear();
    }
    //list all player inventory items
    private void Command_ShowAllPlayerItems()
    {
        insertedCommands.Add("player showallitems");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        consoleText = "All player inventory items:";
        CreateNewConsoleLine();

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

            consoleText = consoleLine;
            CreateNewConsoleLine();
        }
        separatedWords.Clear();
    }
    //add item/items to players inventory
    private void Command_AddItem()
    {
        insertedCommands.Add("player additem " + separatedWords[2] + " " + separatedWords[3]);
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--player additem " + separatedWords[2] + " " + separatedWords[3] +"--";
        CreateNewConsoleLine();
        foundDuplicate = false;
        duplicate = null;

        string itemName = separatedWords[2];

        bool isInt = int.TryParse(itemName, out _);
        bool isinsertedValueInt = int.TryParse(separatedWords[3], out _);
        if (isInt)
        {
            consoleText = "Error: Added item name cannot be a number!";
            CreateNewConsoleLine();
        }
        else if (!isinsertedValueInt)
        {
            consoleText = "Error: Inserted addable item count must be a whole number!";
            CreateNewConsoleLine();
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

                        consoleText = "Successfully added " + insertedValue + " " + selectedItem.name + "(s) to players inventory! Removed " + spawnableItemWeight * insertedValue + " space from players inventory.";
                        CreateNewConsoleLine();
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
                        if (newDuplicate.GetComponent<Item_Ammo>() != null
                            && PlayerInventoryScript.equippedGun != null
                            && newDuplicate.GetComponent<Item_Ammo>().caseType.ToString()
                            == PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().caseType.ToString())
                        {
                            PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().AssignAmmoType();
                        }

                        RebuildInventoryUI();

                        newDuplicate.GetComponent<MeshRenderer>().enabled = false;
                        if (newDuplicate.GetComponent<Rigidbody>() != null)
                        {
                            newDuplicate.GetComponent<Rigidbody>().isKinematic = true;
                        }

                        newDuplicate.GetComponent<Env_Item>().DeactivateItem();

                        consoleText = "Successfully added " + insertedValue + " " + selectedItem.name + "(s) to players inventory! Removed " + spawnableItemWeight * insertedValue + " space from players inventory.";
                        CreateNewConsoleLine();
                    }
                    //if the spawnable item isnt stackable and the player wants to spawn more than one of it
                    else if (!spawnableItem.GetComponent<Env_Item>().isStackable && insertedValue > 1)
                    {
                        remainder = insertedValue;

                        StartCoroutine(SpawnMultipleNonStackables());

                        consoleText = "Successfully added " + insertedValue + " " + selectedItem.name + "(s) to players inventory! Removed " + spawnableItemWeight * insertedValue + " space from players inventory.";
                        CreateNewConsoleLine();
                    }
                }
                else if (spawnableItemWeight * insertedValue > currentPlayerInvFreeSpace)
                {
                    consoleText = "Error: Not enough inventory space to add " + selectedItem.name + " to players inventory!";
                    CreateNewConsoleLine();
                }
            }
            else if (!itemnames.Contains(itemName))
            {
                consoleText = "Error: Item not found! Type sasi to display all spawnable items.";
                CreateNewConsoleLine();
            }
            else if (insertedValue <= 0)
            {
                consoleText = "Error: Item count must be over 0!";
                CreateNewConsoleLine();
            }
            else if (insertedValue >= 1000001)
            {
                consoleText = "Error: Item count must be less than 1000001!";
                CreateNewConsoleLine();
            }
        }

        selectedItem = null;
        separatedWords.Clear();
    }
    //remove item/items from players inventory
    private void Command_RemoveItem()
    {
        insertedCommands.Add("player removeitem " + separatedWords[2] + " " + separatedWords[3]);
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--player removeitem " + separatedWords[2] + " " + separatedWords[3] + "--";
        CreateNewConsoleLine();
        string itemName = separatedWords[2];

        bool isInt = int.TryParse(itemName, out _);
        bool isInsertedValueInt = int.TryParse(separatedWords[3], out _);
        if (isInt)
        {
            consoleText = "Error: Removed item name cannot be a number!";
            CreateNewConsoleLine();
        }
        else if (!isInsertedValueInt)
        {
            consoleText = "Error: Inserted removable item count must be a whole number!";
            CreateNewConsoleLine();
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
                    consoleText = "Error: Can't delete " + selectedItem.GetComponent<Env_Item>().str_ItemName + " through console while it is reloading!";
                    CreateNewConsoleLine();
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
                        consoleText = "Error: Can't delete " + selectedItem.GetComponent<Env_Item>().str_ItemName + " through console while " + selectedGun.GetComponent<Env_Item>().str_ItemName + " is reloading!";
                        CreateNewConsoleLine();
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

                        ResetAmmoAndGun();

                        RebuildInventoryUI();

                        playeritemnames.Remove(selectedItem.GetComponent<Env_Item>().str_ItemName);

                        int spawnableItemWeight = selectedItem.GetComponent<Env_Item>().int_ItemWeight;
                        consoleText = "Successfully removed " + insertedValue + " " + selectedItem.name + "(s) from players inventory! Added " + spawnableItemWeight * insertedValue + " space back to players inventory.";
                        CreateNewConsoleLine();

                        Destroy(selectedItem);
                    }
                    //if you want to remove less than all of the selected stackable item
                    else if (insertedValue < selectedItem.GetComponent<Env_Item>().int_itemCount 
                             && removableItem.GetComponent<Env_Item>().isStackable)
                    {
                        PlayerInventoryScript.invSpace += selectedItem.GetComponent<Env_Item>().int_ItemWeight * insertedValue;
                        selectedItem.GetComponent<Env_Item>().int_itemCount -= insertedValue;
                        RebuildInventoryUI();

                        int spawnableItemWeight = selectedItem.GetComponent<Env_Item>().int_ItemWeight;
                        consoleText = "Successfully removed " + insertedValue + " " + selectedItem.name + "(s) from players inventory! Added " + spawnableItemWeight * insertedValue + " space back to players inventory.";
                        CreateNewConsoleLine();
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
                                removables.Add(removable);
                                removableCount--;
                            }
                        }
                        remainder = removables.Count + 1;
                        StartCoroutine(RemoveMultipleNonStackables());

                        int spawnableItemWeight = selectedItem.GetComponent<Env_Item>().int_ItemWeight;
                        consoleText = "Successfully removed " + insertedValue + " " + selectedItem.name + "(s) from players inventory! Added " + spawnableItemWeight * insertedValue + " space back to players inventory.";
                        CreateNewConsoleLine();
                    }
                }
                canContinueItemRemoval = true;
            }
            else if (!playeritemnames.Contains(separatedWords[2]))
            {
                consoleText = "Error: Item not found! Type player showallitems to display all spawnable items.";
                CreateNewConsoleLine();
            }
            else if (selectedItem.GetComponent<Env_Item>().isProtected)
            {
                consoleText = "Error: This item is protected and can't be removed from the players inventory!";
                CreateNewConsoleLine();
            }
            else if (insertedValue <= 0)
            {
                consoleText = "Error: Item count must be over 0!";
                CreateNewConsoleLine();
            }
            else if (insertedValue > selectedItem.GetComponent<Env_Item>().int_itemCount)
            {
                consoleText = "Error: Trying to remove too many of this item!";
                CreateNewConsoleLine();
            }
            else if (insertedValue >= 1000001)
            {
                consoleText = "Error: Item count must be less than 1000001!";
                CreateNewConsoleLine();
            }
        }

        selectedItem = null;
        separatedWords.Clear();
    }

    //shows all player repairable items and their durability
    private void Command_ShowAllRepairableItems()
    {
        insertedCommands.Add("player showallrepairableitems");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        consoleText = "All player inventory repairable items: <item name> <condition> <percentage from maximum durability> <breakable status - breakable/not breakable>";
        CreateNewConsoleLine();

        for (int i = 0; i < PlayerInventoryScript.inventory.Count; i++)
        {
            displayableItem = PlayerInventoryScript.inventory[i];

            if (displayableItem.GetComponent<Item_Gun>() != null)
            {
                float itemDurability = 
                    displayableItem.GetComponent<Item_Gun>().durability 
                    / displayableItem.GetComponent<Item_Gun>().maxDurability * 100;
                string consoleLine = displayableItem.name + " " +
                    "(" + displayableItem.GetComponent<Item_Gun>().durability + ") " +
                    "(" + itemDurability.ToString() + "%)";

                //extra tag behind item name if its breakable or not
                if (displayableItem.GetComponent<Env_Item>().isProtected)
                {
                    consoleLine += " <not breakable>";
                }
                else if (!displayableItem.GetComponent<Env_Item>().isProtected)
                {
                    consoleLine += " <breakable>";
                }

                consoleText = consoleLine;
                CreateNewConsoleLine();
            }
        }
        separatedWords.Clear();
    }
    //fixes all repairable items for free
    private void Command_FixAllItems()
    {
        insertedCommands.Add("player fixallitems");
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

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

                    consoleText = "Fully repaired " + displayableItem.GetComponent<Env_Item>().str_ItemName + "!";
                    CreateNewConsoleLine();
                }
            }
        }
        else if (!foundRepairable)
        {
            consoleText = "Error: No repairable items were found in the players inventory or all items are already fully repaired!";
            CreateNewConsoleLine();
        }
        foundRepairable = false;
        separatedWords.Clear();
    }
    //lists all players quest IDs
    private void Command_GetAllPlayerQuestIDs()
    {
        insertedCommands.Add("player getallquestids");
        currentSelectedInsertedCommand = insertedCommands.Count - 1;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        consoleText = "Error: This command has no functions yet!";
        CreateNewConsoleLine();

        separatedWords.Clear();
    }

    //set player reputation with a faction
    private void Command_SetPlayerRep()
    {
        insertedCommands.Add("player setrep " + separatedWords[2] + " " + separatedWords[3]);
        currentSelectedInsertedCommand = insertedCommands.Count;
        consoleText = "--" + input + "--";
        CreateNewConsoleLine();

        string factionName = separatedWords[2];
        bool isInt = int.TryParse(factionName, out _);
        bool isInsertedValueInt = int.TryParse(separatedWords[3], out _);

        if (isInt)
        {
            consoleText = "Error: Faction name cannot be a number!";
            CreateNewConsoleLine();
        }
        else if (!isInsertedValueInt)
        {
            consoleText = "Error: Inserted value must be a whole number!";
            CreateNewConsoleLine();
        }
        else if (!isInt && isInsertedValueInt)
        {
            int value = int.Parse(separatedWords[3]);
            if (value > -1001 && value < 1001)
            {
                string confirmedFactionName = "none";

                foreach (string possibleFactionName in factionNames)
                {
                    if (factionName == possibleFactionName)
                    {
                        confirmedFactionName = factionName;
                        break;
                    }
                }
                if (confirmedFactionName != "none")
                {
                    if (confirmedFactionName == "scientists")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv1 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed player and " + confirmedFactionName + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName == "geifers")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv2 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed player and " + confirmedFactionName + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName == "annies")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv3 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed player and " + confirmedFactionName + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName == "verbannte")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv4 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed player and " + confirmedFactionName + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName == "raiders")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv5 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed player and " + confirmedFactionName + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName == "military")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv6 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed player and " + confirmedFactionName + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName == "verteidiger")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv7 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed player and " + confirmedFactionName + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                    else if (confirmedFactionName == "others")
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv8 = value;
                        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                        consoleText = "Changed player and " + confirmedFactionName + " reputation to " + value + "!";
                        CreateNewConsoleLine();
                    }
                }
                else if (confirmedFactionName == "none")
                {
                    consoleText = "Error: Faction name was not found!";
                    CreateNewConsoleLine();
                }
            }
            else if (value <= -1001)
            {
                consoleText = "Error: Inserted reputation value must be higher than -1001!";
                CreateNewConsoleLine();
            }
            else if (value >= 1001)
            {
                consoleText = "Error: Inserted reputation value must be lower than 1001!";
                CreateNewConsoleLine();
            }
        }
        separatedWords.Clear();
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
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.GetComponent<UI_AIContent>().str_NPCName + "'s repair shop";
        }
        else if (PlayerInventoryScript.Workbench != null)
        {
            par_Managers.GetComponent<Manager_UIReuse>().ClearStatsUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildRepairMenu();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = PlayerInventoryScript.GetComponent<Env_Workbench>().str_workbenchName;
        }
        par_Managers.GetComponent<Manager_UIReuse>().UpdateWeaponQuality();
    }

    private void ResetAmmoAndGun()
    {
        //updates player equpped gun and removes ammo
        if (PlayerInventoryScript.equippedGun != null
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

            consoleText = "Unequipped this gun because player removed it from their inventory.";
            CreateNewConsoleLine();
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

                consoleText = "Unloaded this gun and added " + removedAmmo + " ammo to existing ammo clip in players inventory.";
                CreateNewConsoleLine();
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

                consoleText = "Unloaded this gun and added " + removedAmmo + " ammo to new ammo clip in players inventory.";
                CreateNewConsoleLine();
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

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (par_Managers != null)
        {
            output = logString;
            stack = stackTrace;

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
        consoleText = "[" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + "]" + " - " + output;
        CreateNewConsoleLine();
        lastOutput = output;
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.5f);
        startedWait = false;
    }
}