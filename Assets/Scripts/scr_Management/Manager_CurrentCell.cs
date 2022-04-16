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
    public GameObject AITemplate;
    public Transform par_CellNPCs;
    public List<Transform> respawnPositions = new List<Transform>(); 

    [Header("Main cell lists")]
    public List<GameObject> items = new List<GameObject>();
    public List<GameObject> containers = new List<GameObject>();
    public List<GameObject> workbenches = new List<GameObject>();
    public List<GameObject> waitables = new List<GameObject>();
    public List<GameObject> destroyableCrates = new List<GameObject>();
    public List<GameObject> doors = new List<GameObject>();
    public List<GameObject> AI = new List<GameObject>();

    //public but hidden variables
    [HideInInspector] public bool discoveredCell;
    [HideInInspector] public bool isPlayerInCell;
    [HideInInspector] public int respawnableNPCCount;

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
}