using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Manager_GameSaving : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject light_Flashlight;

    [Header("Special items")]
    [SerializeField] private GameObject Exoskeleton;

    [Header("Loading menu content")]
    [SerializeField] private GameObject par_LoadingMenu;
    [SerializeField] private RawImage img_loadingLogo;
    [SerializeField] private RawImage img_smallLoadingLogo;
    [SerializeField] private TMP_Text txt_LoadingText;
    [SerializeField] private TMP_Text txt_TipText;
    [SerializeField] private Button btn_Continue;

    [Header("Scripts")]
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Player_Exoskeleton ExoskeletonScript;
    [SerializeField] private UI_AbilityAssignManager AbilityAssignManagerScript;
    [SerializeField] private UI_AbilitySlot1 AbilitySlot1Script;
    [SerializeField] private UI_AbilitySlot2 AbilitySlot2Script;
    [SerializeField] private UI_AbilitySlot3 AbilitySlot3Script;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool firstload;
    [HideInInspector] public bool isSaving;
    [HideInInspector] public bool isLoading;
    [HideInInspector] public string path;
    [HideInInspector] public string savedFilePath;

    //private variables
    private float time;
    private GameObject equippedWeapon;
    private GameObject equippedFlashlight;
    private List<string> abilityNames = new List<string>();

    //player values
    private Vector3 pos_Player;
    private Vector3 rot_Player;
    private Vector3 rot_PlayerCamera;

    private void Awake()
    {
        //get path to game saves folder
        path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\LightsOff";

        try
        {
            //create LightsOff save folder if it doesnt already exist
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
                //Debug.Log("Successfully created save folder!");

                isLoading = true;
                firstload = true;

                OpenLoadingMenuUI();
            }
            else if (Directory.Exists(path)
                     && !File.Exists(path + @"\Save0001.txt"))
            {
                //initial global cell reset
                par_Managers.GetComponent<GameManager>().GlobalCellReset();

                if (!isLoading)
                {
                    firstload = true;
                    isLoading = true;
                    OpenLoadingMenuUI();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: Failed to create save folder at " + path + "! " + e.Message + ".");
        }

        //loads game data if a save file was found
        if (File.Exists(path + @"\Save0001.txt"))
        {
            if (!isLoading)
            {
                isLoading = true;
                OpenLoadingMenuUI();
            }

            //Debug.Log("Found save file! Loading data...");
            LoadGameData();
        }
    }

    private void Update()
    {
        if (!isSaving)
        {
            //game can only be saved if game isnt currently being saved
            //and if player is alive
            if (Input.GetKeyDown(KeyCode.F5)
                && PlayerHealthScript.health > 0)
            {
                CreateSaveFile();
            }
            //game can only be loaded if game isnt currently being saved
            //and if save file exists in save files directory
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                if (File.Exists(path + @"\Save0001.txt"))
                {
                    SceneManager.LoadScene(1);
                }
                else
                {
                    Debug.LogWarning("Error: Cannot load game - no game save files were found!");
                }
            }
        }

        if (isLoading)
        {
            //time for loading screen timer continues regardless of game being paused
            time += Time.unscaledDeltaTime;

            img_loadingLogo.transform.eulerAngles -= new Vector3(0, 0, 100) * Time.deltaTime;

            //two seconds to load first startup
            //otherwise waiting until game save file finishes loading
            if (time > 0.5f && firstload)
            {
                btn_Continue.gameObject.SetActive(true);
                txt_LoadingText.gameObject.SetActive(false);
                img_loadingLogo.gameObject.SetActive(false);
            }
            if (time > 10)
            {
                int randomTip = UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, par_Managers.GetComponent<GameManager>().tips.Count - 1));
                txt_TipText.text = par_Managers.GetComponent<GameManager>().tips[randomTip];

                time = 0;
            }
        }
        if (isSaving)
        {
            img_smallLoadingLogo.gameObject.SetActive(true);
            img_smallLoadingLogo.transform.eulerAngles -= new Vector3(0, 0, 100) * Time.deltaTime;
        }
        else if (!isSaving)
        {
            img_smallLoadingLogo.gameObject.SetActive(false);
        }
    }

    public void OpenLoadingMenuUI()
    {
        par_LoadingMenu.SetActive(true);
        img_loadingLogo.gameObject.SetActive(true);

        int randomTip = UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, par_Managers.GetComponent<GameManager>().tips.Count - 1));
        txt_TipText.text = par_Managers.GetComponent<GameManager>().tips[randomTip];

        btn_Continue.gameObject.SetActive(false);
    }
    public void CloseLoadingMenuUI()
    {
        par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();

        par_LoadingMenu.SetActive(false);
        isLoading = false;
    }

    //assign saved data to gameobjects
    public void LoadGameData()
    {
        //looks through all the lines
        foreach (string line in File.ReadLines(path + @"\Save0001.txt"))
        {
            //get all separators in line
            char[] separators = new char[] { ' ', ',', '=', '(', ')', '_' };
            //remove unwanted separators and split line into separate strings
            string[] values = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //list of confirmed numbers
            List<string> numbers = new List<string>();
            //add all numbers to new list
            foreach (string value in values)
            {
                bool isFloat = float.TryParse(value, out _);
                if (isFloat)
                {
                    numbers.Add(value);
                }
            }
            //loading global values
            if (line.Contains("gv_"))
            {
                //loading time until global cell reset
                if (line.Contains("hoursUntilGlobalCellReset"))
                {
                    par_Managers.GetComponent<Manager_WorldClock>().hoursUntilCellReset = int.Parse(numbers[0]);
                }
                //loading time
                else if (line.Contains("time")
                    && numbers.Count == 3)
                {
                    par_Managers.GetComponent<Manager_WorldClock>().hour = int.Parse(numbers[0]);
                    par_Managers.GetComponent<Manager_WorldClock>().minute = int.Parse(numbers[1]);
                    par_Managers.GetComponent<Manager_WorldClock>().second = int.Parse(numbers[2]);
                }
                //loading date
                else if (line.Contains("date")
                         && numbers.Count == 3)
                {
                    par_Managers.GetComponent<Manager_WorldCalendar>().dayNumber = int.Parse(numbers[0]);
                    par_Managers.GetComponent<Manager_WorldCalendar>().monthNumber = int.Parse(numbers[1]);
                    par_Managers.GetComponent<Manager_WorldCalendar>().fakeYearNumber = int.Parse(numbers[2]);
                }
            }

            //---
            //loading player values
            else if (line.Contains("pv_"))
            {
                //loading player grounded state
                if (line.Contains("isPlayerGrounded = False"))
                {
                    PlayerMovementScript.isGrounded = false;
                }

                //loading player position
                else if (line.Contains("playerPosition"))
                {
                    //add all values to temporary playerPos value
                    Vector3 playerPos = new Vector3(float.Parse(numbers[0]),
                                                    float.Parse(numbers[1]),
                                                    float.Parse(numbers[2]));
                    //add all temporary playerPos values to real player position
                    player.transform.localPosition = playerPos;
                }
                //loading player rotation
                else if (line.Contains("playerRotation"))
                {
                    //add all values to temporary playerRot value
                    Vector3 playerRot = new Vector3(float.Parse(numbers[0]),
                                                    float.Parse(numbers[1]),
                                                    float.Parse(numbers[2]));
                    //add all temporary playerRot values to real player rotation
                    player.transform.eulerAngles = playerRot;
                }
                //loading player camera rotation
                else if (line.Contains("playerCameraRotation"))
                {
                    //add all values to temporary playerCameraRot value
                    Vector3 playerCameraRot = new Vector3(float.Parse(numbers[0]),
                                                    float.Parse(numbers[1]),
                                                    float.Parse(numbers[2]));
                    //add all temporary playerCameraRot values to real player camera rotation
                    playerCamera.transform.eulerAngles = playerCameraRot;
                }
                //loading player Y velocity
                else if (line.Contains("playerYVelocity"))
                {
                    PlayerMovementScript.velocity.y = float.Parse(numbers[0]);
                }

                //loading player health
                else if (line.Contains("playerHealth"))
                {
                    PlayerHealthScript.health = float.Parse(numbers[0]);
                }
                //loading player max health
                else if (line.Contains("playerMaxHealth"))
                {
                    PlayerHealthScript.maxHealth = float.Parse(numbers[0]);
                }

                //loading player stamina
                else if (line.Contains("playerStamina"))
                {
                    PlayerMovementScript.currentStamina = float.Parse(numbers[0]);
                }
                //loading player max stamina
                else if (line.Contains("playerMaxStamina"))
                {
                    PlayerMovementScript.maxStamina = float.Parse(numbers[0]);
                }

                //loading player radiation
                else if (line.Contains("playerRad"))
                {
                    PlayerHealthScript.radiation = float.Parse(numbers[0]);
                }
                //loading player max radiation
                else if (line.Contains("playerMaxRad"))
                {
                    PlayerHealthScript.maxRadiation = float.Parse(numbers[0]);
                }

                //loading player mental damage
                else if (line.Contains("playerMent"))
                {
                    PlayerHealthScript.mentalState = float.Parse(numbers[0]);
                }
                //loading player max mental damage
                else if (line.Contains("playerMaxMent"))
                {
                    PlayerHealthScript.maxMentalState = float.Parse(numbers[0]);
                }

                //loading player money
                else if (line.Contains("playerMoney"))
                {
                    PlayerInventoryScript.money = int.Parse(numbers[0]);
                }
                //loading player inventory max space
                else if (line.Contains("playerMaxInvSpace"))
                {
                    PlayerInventoryScript.maxInvSpace = int.Parse(numbers[0]);
                }
            }

            //loading player items
            else if (line.Contains("pi_"))
            {
                //loading exoskeleton
                if (line.Contains("hasExoskeleton = True"))
                {
                    GameObject exoskeleton = Instantiate(Exoskeleton,
                                                         PlayerInventoryScript.par_PlayerItems.transform.position,
                                                         Quaternion.identity,
                                                         PlayerInventoryScript.par_PlayerItems.transform);

                    exoskeleton.name = exoskeleton.GetComponent<Env_Item>().str_ItemName;

                    PlayerInventoryScript.inventory.Add(exoskeleton);

                    //item count
                    exoskeleton.GetComponent<Env_Item>().int_itemCount = 1;

                    exoskeleton.GetComponent<Env_Item>().isInPlayerInventory = true;
                    par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(exoskeleton.GetComponent<Env_Item>().str_ItemName);

                    exoskeleton.GetComponent<MeshRenderer>().enabled = false;
                    if (exoskeleton.GetComponent<Rigidbody>() != null)
                    {
                        exoskeleton.GetComponent<Rigidbody>().isKinematic = true;
                    }

                    par_Managers.GetComponent<UI_PlayerMenu>().isExoskeletonEquipped = true;
                    AbilityAssignManagerScript.hasExosuit = true;
                    par_Managers.GetComponent<Manager_UIReuse>().ShowExoskeletonUI();

                    exoskeleton.GetComponent<Env_Item>().DeactivateItem();

                    StartCoroutine(WaitBeforeRemovingExoskeleton());
                }

                //loading player inventory items
                else if (!line.Contains("_ew")
                         && !line.Contains("_ef")
                         && numbers.Count >= 1)
                {
                    foreach (GameObject item in par_Managers.GetComponent<Manager_Console>().spawnables)
                    {
                        string itemName = item.GetComponent<Env_Item>().str_ItemName;

                        //if we found an item from the console spawnables list
                        //that also exists in the player inventory list
                        //then it is duplicated and added to player inventory
                        if (line.Contains(itemName))
                        {
                            GameObject newDuplicate = Instantiate(item,
                                                                  PlayerInventoryScript.par_PlayerItems.transform.position,
                                                                  Quaternion.identity,
                                                                  PlayerInventoryScript.par_PlayerItems.transform);

                            newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;

                            PlayerInventoryScript.inventory.Add(newDuplicate);

                            //item count
                            bool isInt = int.TryParse(numbers[0].ToString(), out _);
                            if (isInt)
                            {
                                newDuplicate.GetComponent<Env_Item>().int_itemCount = int.Parse(numbers[0]);
                            }

                            //if this item is a gun
                            if (newDuplicate.GetComponent<Item_Gun>() != null)
                            {
                                //gun durability
                                newDuplicate.GetComponent<Item_Gun>().durability = float.Parse(numbers[1]);
                                //gun max durability
                                newDuplicate.GetComponent<Item_Gun>().maxDurability = float.Parse(numbers[2]);
                                //gun loaded ammo count
                                newDuplicate.GetComponent<Item_Gun>().currentClipSize = int.Parse(numbers[3]);
                                //gun jam state
                                if (int.Parse(numbers[4]) == 1)
                                {
                                    newDuplicate.GetComponent<Item_Gun>().isGunJammed = true;
                                }
                            }
                            //if this item is a melee weapon
                            else if (newDuplicate.GetComponent<Item_Melee>() != null)
                            {
                                //melee weapon durability
                                newDuplicate.GetComponent<Item_Melee>().durability = float.Parse(numbers[1]);
                                //melee weapon max durability
                                newDuplicate.GetComponent<Item_Melee>().maxDurability = float.Parse(numbers[2]);
                            }
                            //if this item is a consumable
                            else if (newDuplicate.GetComponent<Item_Consumable>() != null)
                            {
                                //consumable remainder
                                newDuplicate.GetComponent<Item_Consumable>().currentConsumableAmount = float.Parse(numbers[1]);
                                //consumable max remainder
                                newDuplicate.GetComponent<Item_Consumable>().maxConsumableAmount = float.Parse(numbers[2]);
                            }
                            //if this item is a battery
                            else if (newDuplicate.GetComponent<Item_Battery>() != null)
                            {
                                //battery remainder
                                newDuplicate.GetComponent<Item_Battery>().currentBattery = float.Parse(numbers[1]);
                                //battery max remainder
                                newDuplicate.GetComponent<Item_Battery>().maxBattery = float.Parse(numbers[2]);
                            }

                            newDuplicate.GetComponent<Env_Item>().isInPlayerInventory = true;
                            par_Managers.GetComponent<Manager_Console>().playeritemnames.Add(newDuplicate.GetComponent<Env_Item>().str_ItemName);

                            //assigns new ammo to currently equipped gun if ammo type is same as equipped gun
                            if (newDuplicate.GetComponent<Item_Ammo>() != null
                                && PlayerInventoryScript.equippedGun != null
                                && newDuplicate.GetComponent<Item_Ammo>().caseType.ToString()
                                == PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().caseType.ToString())
                            {
                                PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().AssignAmmoType();
                            }

                            newDuplicate.GetComponent<MeshRenderer>().enabled = false;
                            if (newDuplicate.GetComponent<Rigidbody>() != null)
                            {
                                newDuplicate.GetComponent<Rigidbody>().isKinematic = true;
                            }

                            newDuplicate.GetComponent<Env_Item>().DeactivateItem();

                            //Debug.Log("Added " + newDuplicate.name + " to player inventory!");
                        }
                    }
                }
                //loading player equipped weapon
                else if (line.Contains("_ew"))
                {
                    foreach (GameObject item in PlayerInventoryScript.inventory)
                    {
                        if (line.Contains(item.name)
                            && (item.GetComponent<Item_Grenade>() != null
                            || (item.GetComponent<Item_Gun>() != null
                            && item.GetComponent<Item_Gun>().durability == float.Parse(numbers[0]))
                            || (item.GetComponent<Item_Melee>() != null
                            && item.GetComponent<Item_Melee>().durability == float.Parse(numbers[0]))))
                        {
                            equippedWeapon = item;

                            //Debug.Log(item.name + " " + line);

                            StartCoroutine(LoadEquippedWeapon());

                            break;
                        }
                    }
                }
                else if (line.Contains("_ef"))
                {
                    foreach (GameObject item in PlayerInventoryScript.inventory)
                    {
                        if (line.Contains(item.name)
                            && item.GetComponent<Item_Flashlight>() != null)
                        {
                            equippedFlashlight = item;

                            //Debug.Log(item.name + " " + line);

                            StartCoroutine(LoadEquippedFlashlight(float.Parse(numbers[0]), float.Parse(numbers[1]), int.Parse(numbers[2])));

                            break;
                        }
                    }
                }
            }

            //---
            //loading player abilities
            else if (line.Contains("pa_"))
            {
                //jump boost ability
                if (line.Contains("jumpBoost"))
                {
                    abilityNames.Add("jumpBoost " + numbers[0] + " " + numbers[1] + " " + numbers[2] + " " + numbers[3]);
                }
                //sprint boost
                else if (line.Contains("sprintBoost"))
                {
                    abilityNames.Add("sprintBoost " + numbers[0] + " " + numbers[1] + " " + numbers[2] + " " + numbers[3]);
                }
                //health regen
                else if (line.Contains("healthRegen"))
                {
                    abilityNames.Add("healthRegen " + numbers[0] + " " + numbers[1] + " " + numbers[2] + " " + numbers[3]);
                }
                //stamina regen
                else if (line.Contains("staminaRegen"))
                {
                    abilityNames.Add("staminaRegen " + numbers[0] + " " + numbers[1] + " " + numbers[2] + " " + numbers[3]);
                }
                //env protection
                else if (line.Contains("envProtection"))
                {
                    abilityNames.Add("envProtection " + numbers[0] + " " + numbers[1] + " " + numbers[2] + " " + numbers[3]);
                }

                StartCoroutine(LoadAbilities());
            }

            //---
            //loading all thrown grenade
            else if (line.Contains("tg_"))
            {
                //get grenade position
                Vector3 position = new Vector3(float.Parse(numbers[0]),
                                               float.Parse(numbers[1]),
                                               float.Parse(numbers[2]));
                //get grenade velocity
                Vector3 velocity = new Vector3(float.Parse(numbers[3]), 
                                               float.Parse(numbers[4]), 
                                               float.Parse(numbers[5]));
                //get grenade time until explosion if this grenade has a timer
                float timeUntilExplosion = 0;
                if (numbers.Count > 6)
                {
                    timeUntilExplosion = float.Parse(numbers[6]);
                }

                //find the correct grenade to instantiate
                GameObject item = null;
                foreach (GameObject spawnable in par_Managers.GetComponent<Manager_Console>().spawnables)
                {
                    if (spawnable.GetComponent<Item_Grenade>() != null)
                    {
                        //frag grenade
                        if (spawnable.GetComponent<Item_Grenade>().grenadeType 
                            == Item_Grenade.GrenadeType.fragmentation
                            && line.Contains(spawnable.name))
                        {
                            item = spawnable;
                            break;
                        }
                        //plasma grenade
                        else if (spawnable.GetComponent<Item_Grenade>().grenadeType
                                 == Item_Grenade.GrenadeType.plasma
                                 && line.Contains(spawnable.name))
                        {
                            item = spawnable;
                            break;
                        }
                        //stun grenade
                        else if (spawnable.GetComponent<Item_Grenade>().grenadeType
                                 == Item_Grenade.GrenadeType.stun
                                 && line.Contains(spawnable.name))
                        {
                            item = spawnable;
                            break;
                        }
                    }
                }

                //spawn the grenade
                GameObject grenade = Instantiate(item, 
                                                 position, 
                                                 Quaternion.identity);

                //add to thrown grenades list
                par_Managers.GetComponent<GameManager>().thrownGrenades.Add(grenade);
                //apply the velocity
                grenade.GetComponent<Rigidbody>().velocity = velocity;

                //set this grenade to thrown grenade
                grenade.GetComponent<Item_Grenade>().isThrownGrenade = true;
                grenade.GetComponent<Item_Grenade>().startedCookingGrenadeTimer = true;
                //enable grenade explosion timer if this grenade is timed
                if (grenade.GetComponent<Item_Grenade>().explosionType
                    == Item_Grenade.ExplosionType.timed)
                {
                    grenade.GetComponent<Item_Grenade>().cookingGrenadeTimer = timeUntilExplosion;
                }

                //Debug.Log(grenade.GetComponent<Env_Item>().int_itemCount + ", " + grenade.GetComponent<Item_Grenade>().isThrownGrenade);
                //Debug.Log("Loaded " + grenade.GetComponent<Env_Item>().str_ItemName + " at position " + position + " with velocity set to " + velocity + " and this grenade has been cooking for " + timeUntilExplosion + " seconds.");
            }

            //---
            //loading all cells
            else if (line.Contains("dc_"))
            {
                foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                {
                    if (line.Contains(cell.GetComponent<Manager_CurrentCell>().str_CellName))
                    {
                        cell.GetComponent<Manager_CurrentCell>().discoveredCell = true;
                        cell.GetComponent<Manager_CurrentCell>().EnableCellTeleportButtonOnMap();

                        if (cell.GetComponent<Manager_CurrentCell>().respawnPositions.Count > 0
                            && int.Parse(numbers[0]) > 0)
                        {
                            par_Managers.GetComponent<GameManager>().RespawnNPCs(cell, int.Parse(numbers[0]));
                        }
                    }
                }
            }

            //loading all cell randomized container contents
            else if (line.Contains("rcc_"))
            {
                //find the correct cell
                foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                {
                    int cellIndex = int.Parse(numbers[0]);
                    if (cellIndex == par_Managers.GetComponent<Manager_Console>().allCells.IndexOf(cell))
                    {
                        //find the correct container in this cell
                        foreach (GameObject container in cell.GetComponent<Manager_CurrentCell>().containers)
                        {
                            int containerIndex = int.Parse(numbers[1]);
                            if (containerIndex == cell.GetComponent<Manager_CurrentCell>().containers.IndexOf(container))
                            {
                                foreach (GameObject item in par_Managers.GetComponent<Manager_Console>().spawnables)
                                {
                                    string itemName = item.GetComponent<Env_Item>().str_ItemName;

                                    //if we found an item from the console spawnables list
                                    //that also exists in the container list
                                    //then it is duplicated and added to container inventory
                                    if (line.Contains(itemName))
                                    {
                                        GameObject newDuplicate = Instantiate(item,
                                                                              container.GetComponent<Inv_Container>().par_ContainerItems.transform.position,
                                                                              Quaternion.identity,
                                                                              container.GetComponent<Inv_Container>().par_ContainerItems.transform);

                                        newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;

                                        container.GetComponent<Inv_Container>().inventory.Add(newDuplicate);

                                        //item count
                                        bool isInt = int.TryParse(numbers[0].ToString(), out _);
                                        if (isInt)
                                        {
                                            newDuplicate.GetComponent<Env_Item>().int_itemCount = int.Parse(numbers[2]);
                                        }

                                        //if this item is a gun
                                        if (newDuplicate.GetComponent<Item_Gun>() != null)
                                        {
                                            //gun durability
                                            newDuplicate.GetComponent<Item_Gun>().durability = float.Parse(numbers[3]);
                                            //gun max durability
                                            newDuplicate.GetComponent<Item_Gun>().maxDurability = float.Parse(numbers[4]);
                                            //gun loaded ammo count
                                            newDuplicate.GetComponent<Item_Gun>().currentClipSize = int.Parse(numbers[5]);
                                            //update gun value and damage
                                            newDuplicate.GetComponent<Item_Gun>().LoadValues();
                                            //gun jam state
                                            if (int.Parse(numbers[6]) == 1)
                                            {
                                                newDuplicate.GetComponent<Item_Gun>().isGunJammed = true;
                                            }
                                        }
                                        //if this item is a melee weapon
                                        else if (newDuplicate.GetComponent<Item_Melee>() != null)
                                        {
                                            //melee weapon durability
                                            newDuplicate.GetComponent<Item_Melee>().durability = float.Parse(numbers[3]);
                                            //melee weapon max durability
                                            newDuplicate.GetComponent<Item_Melee>().maxDurability = float.Parse(numbers[4]);
                                            //update melee weapon value and damage
                                            newDuplicate.GetComponent<Item_Melee>().LoadValues();
                                        }
                                        //if this item is a consumable
                                        else if (newDuplicate.GetComponent<Item_Consumable>() != null)
                                        {
                                            //consumable remainder
                                            newDuplicate.GetComponent<Item_Consumable>().currentConsumableAmount = float.Parse(numbers[3]);
                                            //consumable max remainder
                                            newDuplicate.GetComponent<Item_Consumable>().maxConsumableAmount = float.Parse(numbers[4]);
                                            //update consumable value
                                            newDuplicate.GetComponent<Item_Consumable>().LoadValues();
                                        }
                                        //if this item is a battery
                                        else if (newDuplicate.GetComponent<Item_Battery>() != null)
                                        {
                                            //battery remainder
                                            newDuplicate.GetComponent<Item_Battery>().currentBattery = float.Parse(numbers[3]);
                                            //battery max remainder
                                            newDuplicate.GetComponent<Item_Battery>().maxBattery = float.Parse(numbers[4]);
                                            //update battery value
                                            newDuplicate.GetComponent<Item_Battery>().LoadValues();
                                        }

                                        newDuplicate.GetComponent<Env_Item>().isInContainer = true;

                                        newDuplicate.GetComponent<MeshRenderer>().enabled = false;
                                        if (newDuplicate.GetComponent<Rigidbody>() != null)
                                        {
                                            newDuplicate.GetComponent<Rigidbody>().isKinematic = true;
                                        }

                                        newDuplicate.GetComponent<Env_Item>().DeactivateItem();

                                        //Debug.Log("Added " + newDuplicate.name + " to player inventory!");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //load discovered AI values
            else if (line.Contains("da_"))
            {
                foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
                {
                    //find the correct cell index
                    int cellIndex = int.Parse(numbers[0]);
                    if (cellIndex == par_Managers.GetComponent<Manager_Console>().allCells.IndexOf(cell))
                    {
                        //find the correct npc index
                        foreach (GameObject npc in cell.GetComponent<Manager_CurrentCell>().AI)
                        {
                            int npcIndex = int.Parse(numbers[1]);
                            if (npcIndex == cell.GetComponent<Manager_CurrentCell>().AI.IndexOf(npc))
                            {
                                //load npc position
                                Vector3 AIPos = new Vector3(float.Parse(numbers[2]),
                                                            float.Parse(numbers[3]),
                                                            float.Parse(numbers[4]));
                                npc.transform.localPosition = AIPos;
                                //load npc rotation
                                Vector3 AIRot = new Vector3(float.Parse(numbers[5]),
                                                            float.Parse(numbers[6]),
                                                            float.Parse(numbers[7]));
                                npc.transform.eulerAngles = AIRot;

                                if (numbers.Count > 8)
                                {
                                    //load npc health
                                    npc.transform.GetComponent<AI_Health>().currentHealth = int.Parse(numbers[8]);
                                    //load npc max health
                                    npc.transform.GetComponent<AI_Health>().maxHealth = int.Parse(numbers[9]);
                                }
                            }
                        }
                    }
                }
            }

            //---
            //loading all faction reputations
            else if (line.Contains("fr_"))
            {
                //loading player faction
                if (line.Contains("playerFaction"))
                {
                    //temporary list of all factions
                    List<Manager_FactionReputation.PlayerFaction> factions
                                = Enum.GetValues(typeof(Manager_FactionReputation.PlayerFaction))
                                .Cast<Manager_FactionReputation.PlayerFaction>()
                                .ToList();

                    //get the correct faction
                    string factionName = "";
                    foreach (var faction in factions)
                    {
                        if (faction.ToString().Contains(values[2]))
                        {
                            factionName = values[2];
                            break;
                        }
                    }

                    //convert faction name from string to enum and assign players correct faction in factions script
                    par_Managers.GetComponent<Manager_FactionReputation>().playerFaction = (Manager_FactionReputation.PlayerFaction)Enum.Parse(typeof(Manager_FactionReputation.PlayerFaction), factionName);
                }
                //loading faction values
                else if (line.Contains("vs"))
                {
                    //player vs others
                    if (line.Contains("player_vs_scientists"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv1 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("player_vs_geifers"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv2 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("player_vs_annies"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv3 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("player_vs_verbannte"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv4 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("player_vs_raiders"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv5 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("player_vs_military"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv6 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("player_vs_verteidiger"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv7 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("player_vs_others"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().pv8 = int.Parse(numbers[0]);
                    }

                    //scientists vs others
                    if (line.Contains("scientists_vs_scientists"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v1 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("scientists_vs_geifers"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v2 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("scientists_vs_annies"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v3 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("scientists_vs_verbannte"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v4 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("scientists_vs_raiders"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v5 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("scientists_vs_military"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v6 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("scientists_vs_verteidiger"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v7 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("scientists_vs_others"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep1v8 = int.Parse(numbers[0]);
                    }

                    //geifers vs others
                    if (line.Contains("geifers_vs_scientists"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v1 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("geifers_vs_geifers"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v2 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("geifers_vs_annies"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v3 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("geifers_vs_verbannte"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v4 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("geifers_vs_raiders"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v5 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("geifers_vs_military"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v6 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("geifers_vs_verteidiger"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v7 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("geifers_vs_others"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep2v8 = int.Parse(numbers[0]);
                    }

                    //annies vs others
                    if (line.Contains("annies_vs_scientists"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v1 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("annies_vs_geifers"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v2 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("annies_vs_annies"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v3 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("annies_vs_verbannte"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v4 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("annies_vs_raiders"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v5 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("annies_vs_military"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v6 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("annies_vs_verteidiger"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v7 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("annies_vs_others"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep3v8 = int.Parse(numbers[0]);
                    }

                    //verbannte vs others
                    if (line.Contains("verbannte_vs_scientists"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v1 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verbannte_vs_geifers"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v2 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verbannte_vs_annies"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v3 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verbannte_vs_verbannte"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v4 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verbannte_vs_raiders"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v5 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verbannte_vs_military"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v6 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verbannte_vs_verteidiger"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v7 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verbannte_vs_others"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep4v8 = int.Parse(numbers[0]);
                    }

                    //raiders vs others
                    if (line.Contains("raiders_vs_scientists"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v1 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("raiders_vs_geifers"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v2 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("raiders_vs_annies"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v3 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("raiders_vs_verbannte"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v4 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("raiders_vs_raiders"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v5 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("raiders_vs_military"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v6 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("raiders_vs_verteidiger"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v7 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("raiders_vs_others"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep5v8 = int.Parse(numbers[0]);
                    }

                    //military vs others
                    if (line.Contains("military_vs_scientists"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v1 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("military_vs_geifers"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v2 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("military_vs_annies"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v3 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("military_vs_verbannte"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v4 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("military_vs_raiders"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v5 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("military_vs_military"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v6 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("military_vs_verteidiger"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v7 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("military_vs_others"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep6v8 = int.Parse(numbers[0]);
                    }

                    //verteidiger vs others
                    if (line.Contains("verteidiger_vs_scientists"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v1 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verteidiger_vs_geifers"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v2 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verteidiger_vs_annies"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v3 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verteidiger_vs_verbannte"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v4 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verteidiger_vs_raiders"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v5 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verteidiger_vs_military"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v6 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verteidiger_vs_verteidiger"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v7 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("verteidiger_vs_others"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep7v8 = int.Parse(numbers[0]);
                    }

                    //others vs others
                    if (line.Contains("others_vs_scientists"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v1 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("others_vs_geifers"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v2 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("others_vs_annies"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v3 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("others_vs_verbannte"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v4 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("others_vs_raiders"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v5 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("others_vs_military"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v6 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("others_vs_verteidiger"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v7 = int.Parse(numbers[0]);
                    }
                    if (line.Contains("others_vs_others"))
                    {
                        par_Managers.GetComponent<Manager_FactionReputation>().rep8v8 = int.Parse(numbers[0]);
                    }

                    par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFactionUI();
                }
            }
        }

        Debug.Log("Successfully loaded game from " + path + @"\Save0001.txt!");

        btn_Continue.gameObject.SetActive(true);
        txt_LoadingText.gameObject.SetActive(false);
        img_loadingLogo.gameObject.SetActive(false);
    }

    //create a save file if it doesnt already exist and add data to it
    //or update existing save file
    public void CreateSaveFile()
    {
        isSaving = true;

        //where the save file is located at
        savedFilePath = path + @"\Save0001.txt";

        GetPlayerValues();

        try
        {
            //if we dont have a file with the same name yet
            if (!File.Exists(savedFilePath))
            {
                //creates a new safe file and inserts data into it
                InsertSaveData();

                //Debug.Log("Successfully created save file " + saveFileName + " at " + path + "!");
            }
            //if we already have a file with the same name
            else if (File.Exists(savedFilePath))
            {
                //deletes the old file
                File.Delete(savedFilePath);
                //creates a new safe file and inserts data into it
                InsertSaveData();

                //Debug.Log("Successfully updated save file " + saveFileName + " at " + path + "!");
            }

            Debug.Log("Successfully saved game to " + path + @"\Save0001.txt!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error: Failed to create save file at " + savedFilePath + "! " + e.Message + ".");
        }

        isSaving = false;
    }
    //insert save data into save file
    private void InsertSaveData()
    {
        //using a text editor to write text to the game save file in the saved file path
        using StreamWriter saveFile = File.CreateText(savedFilePath);
        saveFile.WriteLine("Save file for Lights Off Version " + par_Managers.GetComponent<GameManager>().str_GameVersion);
        saveFile.WriteLine("Read more info about the game from https://greeenlaser.itch.io/lightsoff");
        saveFile.WriteLine("Download game versions from https://drive.google.com/drive/folders/12kvUT6EEndku0nDvZVrVd4QRPt50QV7g?usp=sharing");
        saveFile.WriteLine("");
        saveFile.WriteLine("WARNING: Invalid values will break the game - edit at your own risk!");

        saveFile.WriteLine("");
        //save global time, date, etc
        saveFile.WriteLine("--- GLOBAL VALUES ---");

        saveFile.WriteLine("");

        saveFile.WriteLine("gv_hoursUntilGlobalCellReset = " + par_Managers.GetComponent<Manager_WorldClock>().hoursUntilCellReset);
        saveFile.WriteLine("gv_time = " + par_Managers.GetComponent<Manager_WorldClock>().hour + ":"
                                       + par_Managers.GetComponent<Manager_WorldClock>().minute + ":"
                                       + par_Managers.GetComponent<Manager_WorldClock>().second);
        saveFile.WriteLine("gv_date = " + par_Managers.GetComponent<Manager_WorldCalendar>().dayNumber + "."
                                         + par_Managers.GetComponent<Manager_WorldCalendar>().monthNumber + "."
                                         + par_Managers.GetComponent<Manager_WorldCalendar>().fakeYearNumber);

        saveFile.WriteLine("");
        //save player position, player rotation and player camera rotation
        saveFile.WriteLine("--- PLAYER VALUES ---");

        saveFile.WriteLine("");
        //player grounded state
        saveFile.WriteLine("pv_isPlayerGrounded = " + PlayerMovementScript.isGrounded);
        //player Y velocity
        saveFile.WriteLine("pv_playerYVelocity = " + Mathf.Round(PlayerMovementScript.velocity.y * 10) / 10);

        saveFile.WriteLine("");
        //player position
        saveFile.WriteLine("pv_playerPosition = " + pos_Player);
        //player rotation
        saveFile.WriteLine("pv_playerRotation = " + rot_Player);
        //player camera rotation
        saveFile.WriteLine("pv_playerCameraRotation = " + rot_PlayerCamera);

        saveFile.WriteLine("");
        //player health
        saveFile.WriteLine("pv_playerHealth = " + Mathf.Round(PlayerHealthScript.health * 10) / 10);
        //player max health
        saveFile.WriteLine("pv_playerMaxHealth = " + PlayerHealthScript.maxHealth);
        //player stamina
        saveFile.WriteLine("pv_playerStamina = " + Mathf.Round(PlayerMovementScript.currentStamina * 10) / 10);
        //player max stamina
        saveFile.WriteLine("pv_playerMaxStamina = " + PlayerMovementScript.maxStamina);
        //player radiation
        saveFile.WriteLine("pv_playerRad = " + Mathf.Round(PlayerHealthScript.radiation * 10) / 10);
        //player max radiation
        saveFile.WriteLine("pv_playerMaxRad = " + PlayerHealthScript.maxRadiation);
        //player mental damage
        saveFile.WriteLine("pv_playerMent = " + Mathf.Round(PlayerHealthScript.mentalState * 10) / 10);
        //player max mental damage
        saveFile.WriteLine("pv_playerMaxMent = " + PlayerHealthScript.maxMentalState);

        saveFile.WriteLine("");
        //player money
        saveFile.WriteLine("pv_playerMoney = " + PlayerInventoryScript.money);
        //player max inv space
        saveFile.WriteLine("pv_playerMaxInvSpace = " + PlayerInventoryScript.maxInvSpace);

        saveFile.WriteLine("");
        saveFile.WriteLine("--- PLAYER ITEMS ---");

        saveFile.WriteLine("");
        //save all players items
        if (PlayerInventoryScript.inventory.Count > 1
            || (PlayerInventoryScript.inventory.Count == 1
            && PlayerInventoryScript.inventory[0].gameObject.name != "Exoskeleton"))
        {
            saveFile.WriteLine("(1)count (all items)");
            saveFile.WriteLine("(2)current remainder (all consumables) / (2)current durability (all weapons with durability) (-1 means no battery equipped)");
            saveFile.WriteLine("(3)max remainder (all consumables) / (3)max durability (all weapons with durability) (-1 means no battery equipped)");
            saveFile.WriteLine("(4)ammo count (guns)");
            saveFile.WriteLine("(5)gun jam state (0 - not jammed, 1 - jammed) (guns)");

            saveFile.WriteLine("");
            //save all players items
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.GetComponent<Env_Item>().str_ItemName != "Exoskeleton")
                {
                    string finalOutput = "";

                    //save item name
                    string fileName = item.GetComponent<Env_Item>().str_ItemName;
                    //save item count
                    int itemCount = item.GetComponent<Env_Item>().int_itemCount;

                    finalOutput = "pi_" + fileName + " = " + itemCount;

                    //save weapon current and max durability
                    if (item.GetComponent<Item_Gun>() != null)
                    {
                        float itemDurability = item.GetComponent<Item_Gun>().durability;
                        float maxDurability = item.GetComponent<Item_Gun>().maxDurability;

                        finalOutput += ", " + Mathf.Floor(itemDurability * 10) / 10 + ", " + maxDurability;

                        //save gun loaded ammo count
                        int loadedAmmoCount = item.GetComponent<Item_Gun>().currentClipSize;
                        finalOutput += ", " + loadedAmmoCount;

                        //save gun jam state
                        int gunJamState = 0;
                        if (item.GetComponent<Item_Gun>().isGunJammed)
                        {
                            gunJamState = 1;
                        }
                        finalOutput += ", " + gunJamState;
                    }
                    else if (item.GetComponent<Item_Melee>() != null)
                    {
                        float itemDurability = item.GetComponent<Item_Melee>().durability;
                        float maxDurability = item.GetComponent<Item_Melee>().maxDurability;

                        finalOutput += ", " + Mathf.Floor(itemDurability * 10) / 10 + ", " + maxDurability;
                    }

                    //save consumable current and max remainder
                    else if (item.GetComponent<Item_Consumable>() != null)
                    {
                        float itemRemainder = item.GetComponent<Item_Consumable>().currentConsumableAmount;
                        float itemMaxRemainder = item.GetComponent<Item_Consumable>().maxConsumableAmount;

                        finalOutput += ", " + Mathf.Floor(itemRemainder * 10) / 10 + ", " + itemMaxRemainder;
                    }

                    //save flashlight battery current and max remainder
                    else if (item.GetComponent<Item_Flashlight>() != null)
                    {
                        float flbatteryRemainder = -1;
                        float flbatteryMaxRemainder = -1;

                        if (item.GetComponent<Item_Flashlight>().battery != null)
                        {
                            GameObject theBattery = item.GetComponent<Item_Flashlight>().battery;

                            flbatteryRemainder = theBattery.GetComponent<Item_Battery>().currentBattery;
                            flbatteryMaxRemainder = theBattery.GetComponent<Item_Battery>().maxBattery;
                        }

                        finalOutput += ", " + flbatteryRemainder + ", " + flbatteryMaxRemainder;
                    }
                    //save battery current and max remainder
                    else if (item.GetComponent<Item_Battery>() != null)
                    {
                        float batteryRemainder = item.GetComponent<Item_Battery>().currentBattery;
                        float batteryMaxRemainder = item.GetComponent<Item_Battery>().maxBattery;

                        finalOutput += ", " + Mathf.Floor(batteryRemainder * 10) / 10 + ", " + batteryMaxRemainder;
                    }

                    saveFile.WriteLine(finalOutput);
                    saveFile.WriteLine("");
                }
            }
        }
        else if (PlayerInventoryScript.inventory.Count == 0
                || (PlayerInventoryScript.inventory.Count == 1
                && PlayerInventoryScript.inventory[0].gameObject.name == "Exoskeleton"))
        {
            saveFile.WriteLine("<<<NO PLAYER INVENTORY ITEMS FOUND>>>");
            saveFile.WriteLine("");
        }

        saveFile.WriteLine("----");
        saveFile.WriteLine("");
        //save equipped weapon
        if (PlayerInventoryScript.equippedGun != null)
        {
            saveFile.WriteLine("(1)equipped weapon name,");
            saveFile.WriteLine("(2)durability (weapons with durability)");

            saveFile.WriteLine("");

            foreach (GameObject equippedWeapon in PlayerInventoryScript.inventory)
            {
                if (equippedWeapon == PlayerInventoryScript.equippedGun)
                {
                    string finalOutput = "";

                    //save equipped weapon name
                    string weaponName = equippedWeapon.GetComponent<Env_Item>().str_ItemName;
                    finalOutput += "pi_ew = " + weaponName;

                    string weaponDurability = "0";
                    //save equipped weapon current durability if it is a gun
                    if (equippedWeapon.GetComponent<Item_Gun>() != null)
                    {
                        weaponDurability = equippedWeapon.GetComponent<Item_Gun>().durability.ToString();
                        finalOutput += ", " + weaponDurability;
                    }
                    //save equipped weapon current durability if it is a melee weapon
                    else if (equippedWeapon.GetComponent<Item_Melee>() != null)
                    {
                        weaponDurability = equippedWeapon.GetComponent<Item_Melee>().durability.ToString();
                        finalOutput += ", " + weaponDurability;
                    }

                    saveFile.WriteLine(finalOutput);
                    saveFile.WriteLine("");

                    break;
                }
            }
        }
        else
        {
            saveFile.WriteLine("<<<NO EQUIPPED WEAPON FOUND>>>");
            saveFile.WriteLine("");
        }

        saveFile.WriteLine("----");
        saveFile.WriteLine("");
        //save equipped flashlight
        if (PlayerInventoryScript.equippedFlashlight != null)
        {
            saveFile.WriteLine("(1)equipped flashlight name,");
            saveFile.WriteLine("(2)current remainder (-1 means no battery equipped)");
            saveFile.WriteLine("(3)max remainder (-1 means no battery equipped)");
            saveFile.WriteLine("(4)is flashlight enabled (0 is false, 1 is true)");

            saveFile.WriteLine("");

            foreach (GameObject equippedFlashlight in PlayerInventoryScript.inventory)
            {
                if (equippedFlashlight == PlayerInventoryScript.equippedFlashlight)
                {
                    string finalOutput = "";

                    //save equipped flashlight name
                    string flashlightName = equippedFlashlight.GetComponent<Env_Item>().str_ItemName;
                    finalOutput += "pi_ef = " + flashlightName;

                    //save equipped flashlight current and max remainder
                    string flashlightRemainder = "-1";
                    string flashlightMaxRemainder = "-1";
                    if (equippedFlashlight.GetComponent<Item_Flashlight>().battery != null)
                    {
                        var batteryScript = equippedFlashlight.GetComponent<Item_Flashlight>().battery.GetComponent<Item_Battery>();
                        flashlightRemainder = batteryScript.currentBattery.ToString();
                        flashlightMaxRemainder = batteryScript.maxBattery.ToString();
                    }
                    finalOutput += ", " + flashlightRemainder + ", " + flashlightMaxRemainder;

                    if (equippedFlashlight.GetComponent<Item_Flashlight>().isFlashlightEnabled)
                    {
                        finalOutput += ", " + "1";
                    }
                    else
                    {
                        finalOutput += ", " + "0";
                    }

                    saveFile.WriteLine(finalOutput);
                    saveFile.WriteLine("");

                    break;
                }
            }
        }
        else
        {
            saveFile.WriteLine("<<<NO EQUIPPED FLASHLIGHT FOUND>>>");
            saveFile.WriteLine("");
        }

        saveFile.WriteLine("----");
        saveFile.WriteLine("");
        //save exoskeleton if player picked it up
        bool hasExoskeleton = false;
        foreach (GameObject playerItem in PlayerInventoryScript.inventory)
        {
            if (playerItem.GetComponent<Env_Item>().str_ItemName == "Exoskeleton")
            {
                hasExoskeleton = true;

                break;
            }
        }
        saveFile.WriteLine("pi_hasExoskeleton = " + hasExoskeleton);

        saveFile.WriteLine("");
        saveFile.WriteLine("--- PLAYER ABILITIES ---");

        saveFile.WriteLine("");
        saveFile.WriteLine("(1)unlock and upgrade status (0 - not unlocked, 1 - unlocked, 2 - tier 2, 3 - tier 3),");
        saveFile.WriteLine("(2)slot (1-3, 0 means unassigned),");
        saveFile.WriteLine("(3)current timer,");
        saveFile.WriteLine("(4)max timer");

        saveFile.WriteLine("");
        //save all player ability assign, unlock and upgrade abilities
        int abilityIndex = 0;

        foreach (GameObject ability in ExoskeletonScript.abilities)
        {
            //updates values
            ExoskeletonScript.AddValues();

            string str_abilityName = ability.GetComponent<UI_AbilityStatus>().ability.ToString();

            //if the ability isnt unlocked then status = 0
            //or if it is unlocked then status = 1
            //or if it is upgraded to tier 2 then status = 2
            //or if it is upgraded to tier 3 then status = 3
            int status = 0;

            if (ability.GetComponent<UI_AbilityStatus>().isUnlocked)
            {
                status = 1;
            }
            if (ability.GetComponent<UI_AbilityStatus>().isTier2)
            {
                status = 2;
            }
            if (ability.GetComponent<UI_AbilityStatus>().isTier3)
            {
                status = 3;
            }

            //if the ability isnt assigned to any slots then slot = 0
            //or if the ability is assigned to slot 1 then slot = 1
            //or if the ability is assigned to slot 2 then slot = 2
            //or if the ability is assigned to slot 3 then slot = 3
            int slot = 0;

            if (AbilitySlot1Script.assignedAbility.ToString() == str_abilityName)
            {
                slot = 1;
            }
            else if (AbilitySlot2Script.assignedAbility.ToString() == str_abilityName)
            {
                slot = 2;
            }
            else if (AbilitySlot3Script.assignedAbility.ToString() == str_abilityName)
            {
                slot = 3;
            }

            //cooldown for each ability
            float cooldown = ExoskeletonScript.cooldowns[abilityIndex];
            if (cooldown < 0)
            {
                cooldown = 0;
            }

            //max cooldown for each ability
            float maxCooldown = ExoskeletonScript.maxCooldowns[abilityIndex];

            saveFile.WriteLine("pa_" + str_abilityName + " " +
                               "= " + status +
                               ", " + slot +
                               ", " + Mathf.FloorToInt(Mathf.FloorToInt(cooldown)) +
                               ", " + Mathf.FloorToInt(Mathf.FloorToInt(maxCooldown)));

            abilityIndex++;
        }

        saveFile.WriteLine("");
        saveFile.WriteLine("--- THROWN GRENADES ---");

        if (par_Managers.GetComponent<GameManager>().thrownGrenades.Count > 0)
        {
            saveFile.WriteLine("");
            saveFile.WriteLine("(1-3)X,Y and Z positions of this grenade,");
            saveFile.WriteLine("(4-6)X,Y and Z directions of this grenades velocity aka which direction is this grenade heading,");
            saveFile.WriteLine("(7)time since start of grenade cook timer (timed grenades)");

            saveFile.WriteLine("");
            foreach (GameObject grenade in par_Managers.GetComponent<GameManager>().thrownGrenades)
            {
                string finalOutput = "tg_" + grenade.name + " = ";

                //save grenades position
                Vector3 position = grenade.transform.position;
                finalOutput += position;

                //save grenades movement direction
                Vector3 velocity = grenade.GetComponent<Rigidbody>().velocity;
                finalOutput += ", " + velocity;

                //save grenades time until explosion if this is a timed grenade
                if (grenade.GetComponent<Item_Grenade>().explosionType
                    == Item_Grenade.ExplosionType.timed)
                {
                    float timeUntilExplosion = grenade.GetComponent<Item_Grenade>().cookingGrenadeTimer;
                    finalOutput += ", " + Mathf.Round(timeUntilExplosion * 100) / 100;
                }

                saveFile.WriteLine(finalOutput);
            }
        }
        else if (par_Managers.GetComponent<GameManager>().thrownGrenades.Count == 0)
        {
            saveFile.WriteLine("");
            saveFile.WriteLine("<<<NO THROWN GRENADES FOUND>>>");
        }

        saveFile.WriteLine("");
        saveFile.WriteLine("--- DISCOVERED CELLS ---");

        saveFile.WriteLine("");
        saveFile.WriteLine("(1)how many NPCs will respawn in this cell (if this cell supports NPC respawning)");

        saveFile.WriteLine("");
        //save all cells that have been discovered
        foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
        {
            if (cell.GetComponent<Manager_CurrentCell>().discoveredCell)
            {
                //add cell name
                string output = "dc_" + cell.GetComponent<Manager_CurrentCell>().str_CellName;
                //add respawnable npc count if this cell has any respawn positions
                if (cell.GetComponent<Manager_CurrentCell>().respawnPositions.Count > 0)
                {
                    output += " = " + cell.GetComponent<Manager_CurrentCell>().respawnableNPCCount.ToString();
                }

                saveFile.WriteLine(output);
            }
        }

        saveFile.WriteLine("");
        saveFile.WriteLine("--- RANDOMIZABLE CONTAINER CONTENT ---");

        saveFile.WriteLine("");
        saveFile.WriteLine("(1)cell index");
        saveFile.WriteLine("(2)container index in its cell");
        saveFile.WriteLine("(3)count (after =) (all items)");
        saveFile.WriteLine("(4)current remainder (all consumables) / (4)current durability (all weapons with durability) (-1 means no battery equipped)");
        saveFile.WriteLine("(5)max remainder (all consumables) / (5)max durability (all weapons with durability) (-1 means no battery equipped)");
        saveFile.WriteLine("(6)ammo count (guns)");
        saveFile.WriteLine("(7)gun jam state (0 - not jammed, 1 - jammed) (guns)");

        saveFile.WriteLine("");
        //save each containers content if it is randomizable
        foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
        {
            if (cell.GetComponent<Manager_CurrentCell>().discoveredCell)
            {
                if (cell.GetComponent<Manager_CurrentCell>().containers.Count > 0)
                {
                    int randomizableCount = 0;

                    foreach (GameObject container in cell.GetComponent<Manager_CurrentCell>().containers)
                    {
                        if (container.GetComponent<Inv_Container>().randomizeAllContent)
                        {
                            randomizableCount++;

                            foreach (GameObject item in container.GetComponent<Inv_Container>().inventory)
                            {
                                string finalOutput = "rcc_";
                                //add cell index
                                int cellIndex = par_Managers.GetComponent<Manager_Console>().allCells.IndexOf(cell);
                                finalOutput += cellIndex.ToString() + "_";
                                //add container index
                                int containerIndex = cell.GetComponent<Manager_CurrentCell>().containers.IndexOf(container);
                                finalOutput += containerIndex + "_";

                                //save item name
                                string fileName = item.GetComponent<Env_Item>().str_ItemName;
                                //save item count
                                int itemCount = item.GetComponent<Env_Item>().int_itemCount;

                                finalOutput += fileName + " = " + itemCount;

                                //save weapon current and max durability
                                float itemDurability = 0;
                                float maxDurability = 0;
                                if (item.GetComponent<Item_Gun>() != null)
                                {
                                    itemDurability = item.GetComponent<Item_Gun>().durability;
                                    maxDurability = item.GetComponent<Item_Gun>().maxDurability;

                                    finalOutput += ", " + itemDurability + ", " + maxDurability;
                                }
                                else if (item.GetComponent<Item_Melee>() != null)
                                {
                                    itemDurability = item.GetComponent<Item_Melee>().durability;
                                    maxDurability = item.GetComponent<Item_Melee>().maxDurability;

                                    finalOutput += ", " + itemDurability + ", " + maxDurability;
                                }

                                //save consumable current and max remainder
                                float itemRemainder = 0;
                                float itemMaxRemainder = 0;
                                if (item.GetComponent<Item_Consumable>() != null)
                                {
                                    itemRemainder = item.GetComponent<Item_Consumable>().currentConsumableAmount;
                                    itemMaxRemainder = item.GetComponent<Item_Consumable>().maxConsumableAmount;

                                    finalOutput += ", " + itemRemainder + ", " + itemMaxRemainder;
                                }

                                //save flashlight battery current and max remainder
                                if (item.GetComponent<Item_Flashlight>() != null)
                                {
                                    float flbatteryRemainder = -1;
                                    float flbatteryMaxRemainder = -1;

                                    if (item.GetComponent<Item_Flashlight>().battery != null)
                                    {
                                        GameObject theBattery = item.GetComponent<Item_Flashlight>().battery;

                                        flbatteryRemainder = theBattery.GetComponent<Item_Battery>().currentBattery;
                                        flbatteryMaxRemainder = theBattery.GetComponent<Item_Battery>().maxBattery;
                                    }

                                    finalOutput += ", " + flbatteryRemainder + ", " + flbatteryMaxRemainder;
                                }

                                //save battery current and max remainder
                                float batteryRemainder = 0;
                                float batteryMaxRemainder = 0;
                                if (item.GetComponent<Item_Battery>() != null)
                                {
                                    batteryRemainder = item.GetComponent<Item_Battery>().currentBattery;
                                    batteryMaxRemainder = item.GetComponent<Item_Battery>().maxBattery;

                                    finalOutput += ", " + batteryRemainder + ", " + batteryMaxRemainder;
                                }

                                //save gun loaded ammo count
                                int loadedAmmoCount = 0;
                                if (item.GetComponent<Item_Gun>() != null)
                                {
                                    loadedAmmoCount = item.GetComponent<Item_Gun>().currentClipSize;
                                    finalOutput += ", " + loadedAmmoCount;
                                }
                                //save gun jam state
                                int gunJamState = 0;
                                if (item.GetComponent<Item_Gun>() != null)
                                {
                                    if (item.GetComponent<Item_Gun>().isGunJammed)
                                    {
                                        gunJamState = 1;
                                    }
                                    finalOutput += ", " + gunJamState;
                                }

                                saveFile.WriteLine(finalOutput);
                            }
                        }
                    }

                    if (randomizableCount == 0)
                    {
                        saveFile.WriteLine("<<<NO RANDOMIZABLE CONTAINERS FOUND IN DISCOVERED CELL " + cell.name + ">>>");
                    }
                }
                else
                {
                    saveFile.WriteLine("<<<NO RANDOMIZABLE CONTAINERS FOUND IN DISCOVERED CELL " + cell.name + ">>>");
                }

                saveFile.WriteLine("---");
            }
        }

        saveFile.WriteLine("");
        //save all AI positions and rotations
        saveFile.WriteLine("--- DISCOVERED AI ---");

        saveFile.WriteLine("");
        saveFile.WriteLine("(1)cell index,");
        saveFile.WriteLine("(2)NPC index in cell,");
        saveFile.WriteLine("(3, 4, 5)X, Y, Z positions (after =),");
        saveFile.WriteLine("(6, 7, 8)X, Y, Z rotations,");
        saveFile.WriteLine("(9)health (killable NPCs),");
        saveFile.WriteLine("(10)max health (killable NPCs)");

        saveFile.WriteLine("");
        foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
        {
            if (cell.GetComponent<Manager_CurrentCell>().discoveredCell)
            {
                if (cell.GetComponent<Manager_CurrentCell>().AI.Count > 0)
                {
                    foreach (GameObject target in cell.GetComponent<Manager_CurrentCell>().AI)
                    {
                        if ((target.GetComponent<AI_Health>() != null
                            && target.GetComponent<AI_Health>().isAlive)
                            || target.GetComponent<AI_Health>() == null)
                        {
                            Vector3 angle = target.transform.eulerAngles;
                            float x = angle.x;
                            float y = angle.y;
                            //float z = angle.z;

                            if (Vector3.Dot(transform.up, Vector3.up) >= 0f)
                            {
                                if (angle.x >= 0f && angle.x <= 90f)
                                {
                                    x = angle.x;
                                }
                                if (angle.x >= 270f && angle.x <= 360f)
                                {
                                    x = angle.x - 360f;
                                }
                            }
                            if (Vector3.Dot(transform.up, Vector3.up) < 0f)
                            {
                                if (angle.x >= 0f && angle.x <= 90f)
                                {
                                    x = 180 - angle.x;
                                }
                                if (angle.x >= 270f && angle.x <= 360f)
                                {
                                    x = 180 - angle.x;
                                }
                            }

                            if (angle.y > 180)
                            {
                                y = angle.y - 360f;
                            }

                            /*
                            if (angle.z > 180)
                            {
                                z = angle.z - 360f;
                            }

                            Debug.Log(angle + " : " + Mathf.Round(x) + " , " + Mathf.Round(y) + " , " + Mathf.Round(z));
                            */


                            //get AI position and rotation
                            float pos_AIX = target.transform.localPosition.x;
                            float pos_AIY = target.transform.localPosition.y;
                            float pos_AIZ = target.transform.localPosition.z;
                            float rot_AIX = 0;
                            float rot_AIY = y;
                            float rot_AIZ = 0;

                            string output = "da_";

                            int cellIndex = par_Managers.GetComponent<Manager_Console>().allCells.IndexOf(cell);
                            int targetIndex = cell.GetComponent<Manager_CurrentCell>().AI.IndexOf(target);

                            output += cellIndex.ToString() + "_" + targetIndex.ToString() + " = ";

                            Vector3 pos_AI = new Vector3(pos_AIX, pos_AIY, pos_AIZ);
                            output += pos_AI;
                            Vector3 rot_AI = new Vector3(rot_AIX, rot_AIY, rot_AIZ);
                            output += ", " + rot_AI;
                            if (target.GetComponent<AI_Health>() != null
                                && target.GetComponent<AI_Health>().isKillable)
                            {
                                float health = target.GetComponent<AI_Health>().currentHealth;
                                output += ", " + health;
                                float maxHealth = target.GetComponent<AI_Health>().maxHealth;
                                output += ", " + maxHealth;
                            }

                            saveFile.WriteLine(output);
                        }
                    }
                }
                else
                {
                    saveFile.WriteLine("<<<NO AI FOUND IN DISCOVERED CELL " + cell.name + ">>>");
                }

                saveFile.WriteLine("---");
            }
        }

        saveFile.WriteLine("");
        //save all faction reputation values
        saveFile.WriteLine("--- FACTION REPUTATIONS ---");

        saveFile.WriteLine("");
        saveFile.WriteLine("fr_playerFaction = " + par_Managers.GetComponent<Manager_FactionReputation>().playerFaction.ToString());

        saveFile.WriteLine("");
        //player vs others
        saveFile.WriteLine("fr_player_vs_scientists = " + par_Managers.GetComponent<Manager_FactionReputation>().pv1);
        saveFile.WriteLine("fr_player_vs_geifers = " + par_Managers.GetComponent<Manager_FactionReputation>().pv2);
        saveFile.WriteLine("fr_player_vs_annies = " + par_Managers.GetComponent<Manager_FactionReputation>().pv3);
        saveFile.WriteLine("fr_player_vs_verbannte = " + par_Managers.GetComponent<Manager_FactionReputation>().pv4);
        saveFile.WriteLine("fr_player_vs_raiders = " + par_Managers.GetComponent<Manager_FactionReputation>().pv5);
        saveFile.WriteLine("fr_player_vs_military = " + par_Managers.GetComponent<Manager_FactionReputation>().pv6);
        saveFile.WriteLine("fr_player_vs_verteidiger = " + par_Managers.GetComponent<Manager_FactionReputation>().pv7);
        saveFile.WriteLine("fr_player_vs_others = " + par_Managers.GetComponent<Manager_FactionReputation>().pv8);

        saveFile.WriteLine("");
        //scientists vs others
        saveFile.WriteLine("fr_scientists_vs_scientists = " + par_Managers.GetComponent<Manager_FactionReputation>().rep1v1);
        saveFile.WriteLine("fr_scientists_vs_geifers = " + par_Managers.GetComponent<Manager_FactionReputation>().rep1v2);
        saveFile.WriteLine("fr_scientists_vs_annies = " + par_Managers.GetComponent<Manager_FactionReputation>().rep1v3);
        saveFile.WriteLine("fr_scientists_vs_verbannte = " + par_Managers.GetComponent<Manager_FactionReputation>().rep1v4);
        saveFile.WriteLine("fr_scientists_vs_raiders = " + par_Managers.GetComponent<Manager_FactionReputation>().rep1v5);
        saveFile.WriteLine("fr_scientists_vs_military = " + par_Managers.GetComponent<Manager_FactionReputation>().rep1v6);
        saveFile.WriteLine("fr_scientists_vs_verteidiger = " + par_Managers.GetComponent<Manager_FactionReputation>().rep1v7);
        saveFile.WriteLine("fr_scientists_vs_other = " + par_Managers.GetComponent<Manager_FactionReputation>().rep1v8);

        saveFile.WriteLine("");
        //geifers vs others
        saveFile.WriteLine("fr_geifers_vs_scientists = " + par_Managers.GetComponent<Manager_FactionReputation>().rep2v1);
        saveFile.WriteLine("fr_geifers_vs_geifers = " + par_Managers.GetComponent<Manager_FactionReputation>().rep2v2);
        saveFile.WriteLine("fr_geifers_vs_annies = " + par_Managers.GetComponent<Manager_FactionReputation>().rep2v3);
        saveFile.WriteLine("fr_geifers_vs_verbannte = " + par_Managers.GetComponent<Manager_FactionReputation>().rep2v4);
        saveFile.WriteLine("fr_geifers_vs_raiders = " + par_Managers.GetComponent<Manager_FactionReputation>().rep2v5);
        saveFile.WriteLine("fr_geifers_vs_military = " + par_Managers.GetComponent<Manager_FactionReputation>().rep2v6);
        saveFile.WriteLine("fr_geifers_vs_verteidiger = " + par_Managers.GetComponent<Manager_FactionReputation>().rep2v7);
        saveFile.WriteLine("fr_geifers_vs_other = " + par_Managers.GetComponent<Manager_FactionReputation>().rep2v8);

        saveFile.WriteLine("");
        //annies vs others
        saveFile.WriteLine("fr_annies_vs_scientists = " + par_Managers.GetComponent<Manager_FactionReputation>().rep3v1);
        saveFile.WriteLine("fr_annies_vs_geifers = " + par_Managers.GetComponent<Manager_FactionReputation>().rep3v2);
        saveFile.WriteLine("fr_annies_vs_annies = " + par_Managers.GetComponent<Manager_FactionReputation>().rep3v3);
        saveFile.WriteLine("fr_annies_vs_verbannte = " + par_Managers.GetComponent<Manager_FactionReputation>().rep3v4);
        saveFile.WriteLine("fr_annies_vs_raiders = " + par_Managers.GetComponent<Manager_FactionReputation>().rep3v5);
        saveFile.WriteLine("fr_annies_vs_military = " + par_Managers.GetComponent<Manager_FactionReputation>().rep3v6);
        saveFile.WriteLine("fr_annies_vs_verteidiger = " + par_Managers.GetComponent<Manager_FactionReputation>().rep3v7);
        saveFile.WriteLine("fr_annies_vs_other = " + par_Managers.GetComponent<Manager_FactionReputation>().rep3v8);

        saveFile.WriteLine("");
        //verbannte vs others
        saveFile.WriteLine("fr_verbannte_vs_scientists = " + par_Managers.GetComponent<Manager_FactionReputation>().rep4v1);
        saveFile.WriteLine("fr_verbannte_vs_geifers = " + par_Managers.GetComponent<Manager_FactionReputation>().rep4v2);
        saveFile.WriteLine("fr_verbannte_vs_annies = " + par_Managers.GetComponent<Manager_FactionReputation>().rep4v3);
        saveFile.WriteLine("fr_verbannte_vs_verbannte = " + par_Managers.GetComponent<Manager_FactionReputation>().rep4v4);
        saveFile.WriteLine("fr_verbannte_vs_raiders = " + par_Managers.GetComponent<Manager_FactionReputation>().rep4v5);
        saveFile.WriteLine("fr_verbannte_vs_military = " + par_Managers.GetComponent<Manager_FactionReputation>().rep4v6);
        saveFile.WriteLine("fr_verbannte_vs_verteidiger = " + par_Managers.GetComponent<Manager_FactionReputation>().rep4v7);
        saveFile.WriteLine("fr_verbannte_vs_other = " + par_Managers.GetComponent<Manager_FactionReputation>().rep4v8);

        saveFile.WriteLine("");
        //raiders vs others
        saveFile.WriteLine("fr_raiders_vs_scientists = " + par_Managers.GetComponent<Manager_FactionReputation>().rep5v1);
        saveFile.WriteLine("fr_raiders_vs_geifers = " + par_Managers.GetComponent<Manager_FactionReputation>().rep5v2);
        saveFile.WriteLine("fr_raiders_vs_annies = " + par_Managers.GetComponent<Manager_FactionReputation>().rep5v3);
        saveFile.WriteLine("fr_raiders_vs_verbannte = " + par_Managers.GetComponent<Manager_FactionReputation>().rep5v4);
        saveFile.WriteLine("fr_raiders_vs_raiders = " + par_Managers.GetComponent<Manager_FactionReputation>().rep5v5);
        saveFile.WriteLine("fr_raiders_vs_military = " + par_Managers.GetComponent<Manager_FactionReputation>().rep5v6);
        saveFile.WriteLine("fr_raiders_vs_verteidiger = " + par_Managers.GetComponent<Manager_FactionReputation>().rep5v7);
        saveFile.WriteLine("fr_raiders_vs_other = " + par_Managers.GetComponent<Manager_FactionReputation>().rep5v8);

        saveFile.WriteLine("");
        //military vs others
        saveFile.WriteLine("fr_military_vs_scientists = " + par_Managers.GetComponent<Manager_FactionReputation>().rep6v1);
        saveFile.WriteLine("fr_military_vs_geifers = " + par_Managers.GetComponent<Manager_FactionReputation>().rep6v2);
        saveFile.WriteLine("fr_military_vs_annies = " + par_Managers.GetComponent<Manager_FactionReputation>().rep6v3);
        saveFile.WriteLine("fr_military_vs_verbannte = " + par_Managers.GetComponent<Manager_FactionReputation>().rep6v4);
        saveFile.WriteLine("fr_military_vs_raiders = " + par_Managers.GetComponent<Manager_FactionReputation>().rep6v5);
        saveFile.WriteLine("fr_military_vs_military = " + par_Managers.GetComponent<Manager_FactionReputation>().rep6v6);
        saveFile.WriteLine("fr_military_vs_verteidiger = " + par_Managers.GetComponent<Manager_FactionReputation>().rep6v7);
        saveFile.WriteLine("fr_military_vs_other = " + par_Managers.GetComponent<Manager_FactionReputation>().rep6v8);

        saveFile.WriteLine("");
        //verteidiger vs others
        saveFile.WriteLine("fr_verteidiger_vs_scientists = " + par_Managers.GetComponent<Manager_FactionReputation>().rep7v1);
        saveFile.WriteLine("fr_verteidiger_vs_geifers = " + par_Managers.GetComponent<Manager_FactionReputation>().rep7v2);
        saveFile.WriteLine("fr_verteidiger_vs_annies = " + par_Managers.GetComponent<Manager_FactionReputation>().rep7v3);
        saveFile.WriteLine("fr_verteidiger_vs_verbannte = " + par_Managers.GetComponent<Manager_FactionReputation>().rep7v4);
        saveFile.WriteLine("fr_verteidiger_vs_raiders = " + par_Managers.GetComponent<Manager_FactionReputation>().rep7v5);
        saveFile.WriteLine("fr_verteidiger_vs_military = " + par_Managers.GetComponent<Manager_FactionReputation>().rep7v6);
        saveFile.WriteLine("fr_verteidiger_vs_verteidiger = " + par_Managers.GetComponent<Manager_FactionReputation>().rep7v7);
        saveFile.WriteLine("fr_verteidiger_vs_other = " + par_Managers.GetComponent<Manager_FactionReputation>().rep7v8);

        saveFile.WriteLine("");
        //others vs others
        saveFile.WriteLine("fr_other_vs_scientists = " + par_Managers.GetComponent<Manager_FactionReputation>().rep8v1);
        saveFile.WriteLine("fr_other_vs_geifers = " + par_Managers.GetComponent<Manager_FactionReputation>().rep8v2);
        saveFile.WriteLine("fr_other_vs_annies = " + par_Managers.GetComponent<Manager_FactionReputation>().rep8v3);
        saveFile.WriteLine("fr_other_vs_verbannte = " + par_Managers.GetComponent<Manager_FactionReputation>().rep8v4);
        saveFile.WriteLine("fr_other_vs_raiders = " + par_Managers.GetComponent<Manager_FactionReputation>().rep8v5);
        saveFile.WriteLine("fr_other_vs_military = " + par_Managers.GetComponent<Manager_FactionReputation>().rep8v6);
        saveFile.WriteLine("fr_other_vs_verteidiger = " + par_Managers.GetComponent<Manager_FactionReputation>().rep8v7);
        saveFile.WriteLine("fr_other_vs_other = " + par_Managers.GetComponent<Manager_FactionReputation>().rep8v8);
    }

    //get all player values
    private void GetPlayerValues()
    {
        Vector3 angle = playerCamera.transform.eulerAngles;
        float x = angle.x;
        float y = angle.y;
        //float z = angle.z;

        if (Vector3.Dot(transform.up, Vector3.up) >= 0f)
        {
            if (angle.x >= 0f && angle.x <= 90f)
            {
                x = angle.x;
            }
            if (angle.x >= 270f && angle.x <= 360f)
            {
                x = angle.x - 360f;
            }
        }
        if (Vector3.Dot(transform.up, Vector3.up) < 0f)
        {
            if (angle.x >= 0f && angle.x <= 90f)
            {
                x = 180 - angle.x;
            }
            if (angle.x >= 270f && angle.x <= 360f)
            {
                x = 180 - angle.x;
            }
        }

        if (angle.y > 180)
        {
            y = angle.y - 360f;
        }

        /*
        if (angle.z > 180)
        {
            z = angle.z - 360f;
        }

        Debug.Log(angle + " : " + Mathf.Round(x) + " , " + Mathf.Round(y) + " , " + Mathf.Round(z));
        */


        //get player position, player and player camera rotation
        float pos_playerX = player.transform.localPosition.x;
        float pos_playerY = player.transform.localPosition.y;
        float pos_playerZ = player.transform.localPosition.z;
        float rot_playerX = 0;
        float rot_playerY = y;
        float rot_playerZ = 0;
        float rot_playerCameraX = x;
        float rot_playerCameraY = 0;
        float rot_playerCameraZ = 0;

        pos_Player = new Vector3(pos_playerX, pos_playerY, pos_playerZ);
        rot_Player = new Vector3(rot_playerX, rot_playerY, rot_playerZ);
        rot_PlayerCamera = new Vector3(rot_playerCameraX, rot_playerCameraY, rot_playerCameraZ);
    }

    private IEnumerator LoadEquippedWeapon()
    {
        yield return new WaitForSeconds(0.1f);

        //equips gun
        if (equippedWeapon.GetComponent<Item_Gun>() != null)
        {
            equippedWeapon.GetComponent<Item_Gun>().EquipGun();
        }
        //equips grenade
        else if (equippedWeapon.GetComponent<Item_Grenade>() != null)
        {
            equippedWeapon.GetComponent<Item_Grenade>().EquipGrenade();
        }
        //equips melee
        else if (equippedWeapon.GetComponent<Item_Melee>() != null)
        {
            equippedWeapon.GetComponent<Item_Melee>().EquipMeleeWeapon();
        }
    }

    private IEnumerator LoadEquippedFlashlight(float currentRemainder, float maxRemainder, int isEnabled)
    {
        yield return new WaitForSeconds(0.1f);

        //equips flashlight
        equippedFlashlight.GetComponent<Item_Flashlight>().EquipFlashlight();

        //equips battery for flashlight
        foreach (GameObject item in PlayerInventoryScript.inventory)
        {
            if (item.GetComponent<Item_Battery>() != null
                && item.GetComponent<Item_Battery>().currentBattery == currentRemainder
                && item.GetComponent<Item_Battery>().maxBattery == maxRemainder)
            {
                item.GetComponent<Item_Battery>().isBeingAssigned = true;
                break;
            }
        }
        equippedFlashlight.GetComponent<Item_Flashlight>().AssignBattery();

        //turns on flashlight
        if (isEnabled == 1)
        {
            equippedFlashlight.GetComponent<Item_Flashlight>().isFlashlightEnabled = true;
            equippedFlashlight.GetComponent<Item_Flashlight>().CheckForFlashlight();
        }
    }

    private IEnumerator WaitBeforeRemovingExoskeleton()
    {
        yield return new WaitForSeconds(0.1f);

        Exoskeleton.SetActive(false);
    }

    private IEnumerator LoadAbilities()
    {
        yield return new WaitForSeconds(0.1f);

        int mainIndex = 0;

        foreach (string abilityName in abilityNames)
        {
            //get all separators in line
            char[] separators = new char[] { ' ', ',', '=', '(', ')', '_' };
            //remove unwanted separators and split line into separate strings
            string[] values = abilityName.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            //list of confirmed numbers
            List<string> numbers = new List<string>();
            //add all numbers to new list
            foreach (string value in values)
            {
                bool isFloat = float.TryParse(value, out _);
                if (isFloat)
                {
                    numbers.Add(value);
                }
            }

            //if the ability is unlocked
            if (int.Parse(numbers[0]) > 0)
            {
                GameObject ability = ExoskeletonScript.abilities[mainIndex];

                //jump boost
                if (abilityName == abilityNames[0])
                {
                    if (numbers[0] == "1")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;

                        ExoskeletonScript.UnlockAbility_jumpBoost();
                    }
                    else if (numbers[0] == "2")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;

                        ExoskeletonScript.UnlockAbility_jumpBoost();
                        ExoskeletonScript.UnlockAbility_jumpBoost();
                    }
                    else if (numbers[0] == "3")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier3 = true;

                        ExoskeletonScript.UnlockAbility_jumpBoost();
                        ExoskeletonScript.UpgradeAbility_jumpBoost_Tier2();
                        ExoskeletonScript.UpgradeAbility_jumpBoost_Tier3();
                    }

                    //if the ability is assigned to slot 1, 2 or 3
                    if (int.Parse(numbers[1]) > 0)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.interactable = true;
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.onClick.RemoveAllListeners();

                        //assigned to slot 1
                        if (numbers[1] == "1")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1();
                        }
                        //assigned to slot 2
                        else if (numbers[1] == "2")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2();
                        }
                        //assigned to slot 3
                        else if (numbers[1] == "3")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignJumpBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3();
                        }

                        ExoskeletonScript.maxCooldown_jumpBoost = float.Parse(numbers[3]);

                        //if this ability is in use
                        if (int.Parse(numbers[2]) > 0)
                        {
                            yield return new WaitForSeconds(0.1f);

                            ExoskeletonScript.remainingCooldown_jumpBoost = float.Parse(numbers[2]);
                            ExoskeletonScript.UseAbility_jumpBoost();
                        }
                    }
                }
                //sprint boost
                else if (abilityName == abilityNames[1])
                {
                    if (numbers[0] == "1")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;

                        ExoskeletonScript.UnlockAbility_sprintBoost();
                    }
                    else if (numbers[0] == "2")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;

                        ExoskeletonScript.UnlockAbility_sprintBoost();
                        ExoskeletonScript.UpgradeAbility_sprintBoost_Tier2();
                    }
                    else if (numbers[0] == "3")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier3 = true;

                        ExoskeletonScript.UnlockAbility_sprintBoost();
                        ExoskeletonScript.UpgradeAbility_sprintBoost_Tier2();
                        ExoskeletonScript.UpgradeAbility_sprintBoost_Tier3();
                    }

                    //if the ability is assigned to slot 1, 2 or 3
                    if (int.Parse(numbers[1]) > 0)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.interactable = true;
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.onClick.RemoveAllListeners();

                        //assigned to slot 1
                        if (numbers[1] == "1")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1();
                        }
                        //assigned to slot 2
                        else if (numbers[1] == "2")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2();
                        }
                        //assigned to slot 3
                        else if (numbers[1] == "3")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignSprintBoost.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3();
                        }

                        ExoskeletonScript.maxCooldown_sprintBoost = float.Parse(numbers[3]);

                        //if this ability is in use
                        if (int.Parse(numbers[2]) > 0)
                        {
                            yield return new WaitForSeconds(0.1f);

                            ExoskeletonScript.remainingCooldown_sprintBoost = float.Parse(numbers[2]);
                            ExoskeletonScript.UseAbility_sprintBoost();
                        }
                    }
                }
                //health regen
                else if (abilityName == abilityNames[2])
                {
                    if (numbers[0] == "1")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;

                        ExoskeletonScript.UnlockAbility_healthRegen();
                    }
                    else if (numbers[0] == "2")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;

                        ExoskeletonScript.UnlockAbility_healthRegen();
                        ExoskeletonScript.UpgradeAbility_healthRegen_Tier2();
                    }
                    else if (numbers[0] == "3")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier3 = true;

                        ExoskeletonScript.UnlockAbility_healthRegen();
                        ExoskeletonScript.UpgradeAbility_healthRegen_Tier2();
                        ExoskeletonScript.UpgradeAbility_healthRegen_Tier3();
                    }

                    //if the ability is assigned to slot 1, 2 or 3
                    if (int.Parse(numbers[1]) > 0)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.interactable = true;
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.onClick.RemoveAllListeners();

                        //assigned to slot 1
                        if (numbers[1] == "1")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1();
                        }
                        //assigned to slot 2
                        else if (numbers[1] == "2")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2();
                        }
                        //assigned to slot 3
                        else if (numbers[1] == "3")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignHealthRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3();
                        }

                        ExoskeletonScript.maxCooldown_healthRegen = float.Parse(numbers[3]);

                        //if this ability is in use
                        if (int.Parse(numbers[2]) > 0)
                        {
                            yield return new WaitForSeconds(0.1f);

                            ExoskeletonScript.remainingCooldown_healthRegen = float.Parse(numbers[2]);
                            ExoskeletonScript.UseAbility_healthRegen();
                        }
                    }
                }
                //stamina regen
                else if (abilityName == abilityNames[3])
                {
                    if (numbers[0] == "1")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;

                        ExoskeletonScript.UnlockAbility_staminaRegen();
                    }
                    else if (numbers[0] == "2")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;

                        ExoskeletonScript.UnlockAbility_staminaRegen();
                        ExoskeletonScript.UpgradeAbility_staminaRegen_Tier2();
                    }
                    else if (numbers[0] == "3")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier3 = true;

                        ExoskeletonScript.UnlockAbility_staminaRegen();
                        ExoskeletonScript.UpgradeAbility_staminaRegen_Tier2();
                        ExoskeletonScript.UpgradeAbility_staminaRegen_Tier3();
                    }

                    //if the ability is assigned to slot 1, 2 or 3
                    if (int.Parse(numbers[1]) > 0)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.interactable = true;
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.onClick.RemoveAllListeners();

                        //assigned to slot 1
                        if (numbers[1] == "1")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1();
                        }
                        //assigned to slot 2
                        else if (numbers[1] == "2")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2();
                        }
                        //assigned to slot 3
                        else if (numbers[1] == "3")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignStaminaRegen.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3();
                        }

                        ExoskeletonScript.maxCooldown_staminaRegen = float.Parse(numbers[3]);

                        //if this ability is in use
                        if (int.Parse(numbers[2]) > 0)
                        {
                            yield return new WaitForSeconds(0.1f);

                            ExoskeletonScript.remainingCooldown_staminaRegen = float.Parse(numbers[2]);
                            ExoskeletonScript.UseAbility_staminaRegen();
                        }
                    }
                }
                //env protection
                else if (abilityName == abilityNames[4])
                {
                    if (numbers[0] == "1")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;

                        ExoskeletonScript.UnlockAbility_envProtection();
                    }
                    else if (numbers[0] == "2")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;

                        ExoskeletonScript.UnlockAbility_envProtection();
                        ExoskeletonScript.UpgradeAbility_envProtection_Tier2();
                    }
                    else if (numbers[0] == "3")
                    {
                        ability.GetComponent<UI_AbilityStatus>().isUnlocked = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier2 = true;
                        ability.GetComponent<UI_AbilityStatus>().isTier3 = true;

                        ExoskeletonScript.UnlockAbility_envProtection();
                        ExoskeletonScript.UpgradeAbility_envProtection_Tier2();
                        ExoskeletonScript.UpgradeAbility_envProtection_Tier3();
                    }

                    //if the ability is assigned to slot 1, 2 or 3
                    if (int.Parse(numbers[1]) > 0)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.interactable = true;
                        par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.onClick.RemoveAllListeners();

                        //assigned to slot 1
                        if (numbers[1] == "1")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot1();
                        }
                        //assigned to slot 2
                        else if (numbers[1] == "2")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot2();
                        }
                        //assigned to slot 3
                        else if (numbers[1] == "3")
                        {
                            par_Managers.GetComponent<Manager_UIReuse>().btn_assignEnvProtection.GetComponent<UI_AssignThisAbilityToSlot>().AssignToSlot3();
                        }

                        ExoskeletonScript.maxCooldown_envProtection = float.Parse(numbers[3]);

                        //if this ability is in use
                        if (int.Parse(numbers[2]) > 0)
                        {
                            yield return new WaitForSeconds(0.1f);

                            ExoskeletonScript.remainingCooldown_envProtection = float.Parse(numbers[2]);
                            ExoskeletonScript.UseAbility_envProtection();
                        }
                    }
                }
            }

            mainIndex++;
        }
    }
}