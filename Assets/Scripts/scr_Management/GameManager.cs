using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game version")]
    public string str_GameVersion;
    [SerializeField] private TMP_Text txt_GameVersion;

    [Header("Framerate")]
    [SerializeField] private int maxFPS;
    [Range(0.01f, 1f)]
    [SerializeField] private float fpsUpdateSpeed;
    [SerializeField] private TMP_Text txt_fpsValue;

    [Header("Message send test")]
    [SerializeField] private GameObject logo;
    [SerializeField] private GameObject par_Managers;

    [Header("Tip list")]
    public List<string> tips = new List<string>();

    //public but hidden variables
    [HideInInspector] public bool respawnNPCs;
    [HideInInspector] public int respawnCount;
    [HideInInspector] public List<GameObject> thrownGrenades = new List<GameObject>();

    //private variables
    private float fps;
    private int scrCount;
    private GameObject currentCell;

    private void Awake()
    {
        txt_GameVersion.text = str_GameVersion;

        //EXCLUSIVE WINDOW AND WINDOWED MODE WILL BREAK THE GAME UI!
        //MAXIMIZED WINDOW IS NOT SUPPORTED IN WINDOWS!
        Screen.SetResolution(1920, 1080, true, maxFPS);

        InvokeRepeating(nameof(GetFPS), 1, fpsUpdateSpeed);
    }

    private void Update()
    {
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

        if (respawnNPCs)
        {
            for (int i = 0; i < respawnCount; i++)
            {
                //create a temporary template npc gameobject
                GameObject templateNPC = currentCell.GetComponent<Manager_CurrentCell>().AITemplate;
                //pick a random respawn positions list index
                int randomSpawnPosIndex = Random.Range(0, currentCell.GetComponent<Manager_CurrentCell>().respawnPositions.Count);
                //pick a spawn position according to the randomly picked spawn position list index
                Transform pos_randomSpawn = currentCell.GetComponent<Manager_CurrentCell>().respawnPositions[randomSpawnPosIndex];
                //spawn the npc at the position
                GameObject respawnedNPC = Instantiate(templateNPC,
                                                     templateNPC.transform.position,
                                                     Quaternion.identity,
                                                     currentCell.GetComponent<Manager_CurrentCell>().par_CellNPCs);
                //assign the spawned npc to the npcs list in the current cell
                currentCell.GetComponent<Manager_CurrentCell>().AI.Add(respawnedNPC);
                //activate the NPC
                respawnedNPC.gameObject.SetActive(true);
            }

            respawnCount = 0;
            respawnNPCs = false;
        }
    }

    private void GetFPS()
    {
        fps = Mathf.FloorToInt(1 / Time.deltaTime);
        txt_fpsValue.text = fps.ToString();
    }

    //press F12 for screenshot
    private void Screenshot()
    {
        scrCount++;
        ScreenCapture.CaptureScreenshot("GameScreenshot_" + scrCount + ".png");
        Debug.Log("Screenshot " + scrCount + " was taken!");
    }

    public void GlobalCellReset()
    {
        Debug.Log("System: Global cell reset.");

        //get all game cells
        foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
        {
            //get each container
            if (cell.GetComponent<Manager_CurrentCell>().containers.Count > 0)
            {
                foreach (GameObject container in cell.GetComponent<Manager_CurrentCell>().containers)
                {
                    //remove previous container content if there was any
                    if (container.GetComponent<Inv_Container>().inventory.Count > 0)
                    {
                        for (int i = 0; i < container.GetComponent<Inv_Container>().inventory.Count; i++)
                        {
                            GameObject item = container.GetComponent<Inv_Container>().inventory[i];
                            Destroy(item);
                            container.GetComponent<Inv_Container>().inventory.Remove(item);
                        }
                    }

                    //randomize container content
                    if (container.GetComponent<Inv_Container>().randomizeAllContent)
                    {
                        container.GetComponent<Inv_Container>().RandomizeAllContent();
                    }
                }
            }
            //respawn all dead respawnable NPCs
            //and refill alive NPC health if theyre not in combat
            if (cell.GetComponent<Manager_CurrentCell>().AI.Count > 0)
            {
                foreach (GameObject npc in cell.GetComponent<Manager_CurrentCell>().AI)
                {
                    if (npc.GetComponent<AI_Health>() != null
                        && npc.GetComponent<AI_Health>().isAlive
                        && !npc.GetComponent<AI_Combat>().searchingForHostiles)
                    {
                        npc.GetComponent<AI_Health>().currentHealth
                            = npc.GetComponent<AI_Health>().maxHealth;
                    }
                }
            }
            if (cell.GetComponent<Manager_CurrentCell>().respawnableNPCCount > 0)
            {
                currentCell = cell;
                respawnNPCs = true;
                respawnCount = cell.GetComponent<Manager_CurrentCell>().respawnableNPCCount;
            }
        }
    }
}