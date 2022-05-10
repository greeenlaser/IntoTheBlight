using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_CurrentCell : MonoBehaviour
{
    [Header("Assignables")]
    public string str_CellName;
    public Transform currentCellSpawnpoint;
    [SerializeField] private GameObject par_Managers;

    [Header("Teleportable cell")]
    public bool canBeTeleportedTo;
    [SerializeField] private GameObject par_CellOnMap;

    [Header("Toggle global volume")]
    [SerializeField] private bool isInteriorCell;
    [SerializeField] private GameObject GlobalVolume;

    [Header("Respawnable NPCs")]
    [SerializeField] private int maxNPCSpawnCount;
    [SerializeField] private GameObject NPCTemplate;
    [SerializeField] private Transform par_CellNPCs;
    [SerializeField] private List<Transform> spawnPositions; 

    [Header("Main cell lists")]
    public List<GameObject> items;
    public List<GameObject> containers;
    public List<GameObject> workbenches;
    public List<GameObject> waitables;
    public List<GameObject> destroyableCrates;
    public List<GameObject> doors;
    public List<GameObject> AI;

    //public but hidden variables
    [HideInInspector] public bool discoveredCell;
    [HideInInspector] public bool isPlayerInCell;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadCell();
            isPlayerInCell = true;
        }
        else if (other.CompareTag("Item")
                 && !other.GetComponent<Env_Item>().isInPlayerInventory
                 && !other.GetComponent<Env_Item>().isInContainer
                 && !other.GetComponent<Env_Item>().isInTraderShop
                 && !items.Contains(other.gameObject))
        {
            items.Add(other.gameObject);
        }
        else if (other.CompareTag("NPC")
                 && !AI.Contains(other.gameObject))
        {
            AI.Add(other.gameObject);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UnloadCell();
            isPlayerInCell = false;
        }
        else if (other.CompareTag("Item")
                 && !other.GetComponent<Env_Item>().isInPlayerInventory
                 && !other.GetComponent<Env_Item>().isInContainer
                 && !other.GetComponent<Env_Item>().isInTraderShop
                 && items.Contains(other.gameObject))
        {
            items.Remove(other.gameObject);
        }
        else if (other.CompareTag("NPC")
                 && AI.Contains(other.gameObject))
        {
            AI.Remove(other.gameObject);
        }
    }

    public void LoadCell()
    {
        if (!discoveredCell)
        {
            if (canBeTeleportedTo)
            {
                EnableCellTeleportButtonOnMap();
            }
            discoveredCell = true;
        }

        if (isInteriorCell)
        {
            GlobalVolume.SetActive(false);
        }
    }
    private void UnloadCell()
    {
        if (!GlobalVolume.activeInHierarchy)
        {
            GlobalVolume.SetActive(true);
        }
    }

    public void EnableCellTeleportButtonOnMap()
    {
        par_CellOnMap.SetActive(true);
        //Debug.Log("Discovered cell " + str_CellName + "!");
    }

    public void CellReset()
    {
        if (AI.Count > 0)
        {
            RefillAliveNPCHealth();
        }
        if (maxNPCSpawnCount > 0)
        {
            int remainder = maxNPCSpawnCount - AI.Count;
            RespawnNPCs(remainder);
        }

        ContainerReset();
    }

    //fills up all AI health who arent in combat
    //and whose health is below their max health
    private void RefillAliveNPCHealth()
    {
        foreach (GameObject npc in AI)
        {
            if (npc.GetComponent<AI_Health>() != null
                && npc.GetComponent<AI_Health>().isAlive
                && !npc.GetComponent<AI_Combat>().searchingForHostiles
                && npc.GetComponent<AI_Health>().currentHealth
                < npc.GetComponent<AI_Health>().maxHealth)
            {
                npc.GetComponent<AI_Health>().currentHealth
                    = npc.GetComponent<AI_Health>().maxHealth;
            }
        }
    }
    //respawns dead NPCs
    private void RespawnNPCs(int respawnCount)
    {
        for (int i = 0; i < respawnCount -1; i++)
        {
            //pick a random respawn positions list index
            int randomSpawnPosIndex = Random.Range(0, spawnPositions.Count);
            //pick a spawn position according to the randomly picked spawn position list index
            Transform pos_randomSpawn = spawnPositions[randomSpawnPosIndex];
            //spawn the npc at the position
            GameObject respawnedNPC = Instantiate(NPCTemplate,
                                                 pos_randomSpawn.position,
                                                 Quaternion.identity,
                                                 par_CellNPCs);
            //activate the NPC
            respawnedNPC.SetActive(true);

            foreach (Transform child in respawnedNPC.transform)
            {
                if (child.name == "par_AIContent"
                    && child.GetComponent<AI_Health>() != null)
                {
                    //fill up respawned npc health and set color to green
                    AI_Health npcHealthScript = child.GetComponent<AI_Health>();
                    npcHealthScript.currentHealth = npcHealthScript.maxHealth;
                    child.GetComponent<Renderer>().material.color = Color.green;

                    break;
                }
            }

            Debug.Log("aaa");
        }
    }
    //resets all resettable containers
    private void ContainerReset()
    {
        foreach (GameObject container in containers)
        {
            if (container.GetComponent<Inv_Container>().randomizeAllContent)
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

                container.GetComponent<Inv_Container>().RandomizeAllContent();
            }
        }
    }
}