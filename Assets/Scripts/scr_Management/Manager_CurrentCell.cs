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

    private void Awake()
    {
        QuitelyUnloadCell();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadCell();
            isPlayerInCell = true;

            other.GetComponent<Player_Movement>().currentCell = gameObject;
        }
        if (other.CompareTag("Item")
            && !other.GetComponent<Env_Item>().isInPlayerInventory
            && !other.GetComponent<Env_Item>().isInContainer
            && !other.GetComponent<Env_Item>().isInTraderShop
            && !items.Contains(other.gameObject))
        {
            if (!isPlayerInCell)
            {
                LoadCell();
                UnloadCell();
            }

            other.GetComponent<Env_Item>().currentCell = gameObject;

            items.Add(other.gameObject);
        }
        if (other.CompareTag("NPC")
            && !AI.Contains(other.gameObject))
        {
            other.GetComponent<AI_Movement>().currentCell = gameObject;

            AI.Add(other.gameObject);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UnloadCell();
            isPlayerInCell = false;

            other.GetComponent<Player_Movement>().currentCell = null;
            other.GetComponent<Player_Movement>().lastCell = gameObject;
        }
        if (other.CompareTag("Item")
            && !other.GetComponent<Env_Item>().isInPlayerInventory
            && !other.GetComponent<Env_Item>().isInContainer
            && !other.GetComponent<Env_Item>().isInTraderShop
            && items.Contains(other.gameObject))
        {
            if (!isPlayerInCell)
            {
                LoadCell();
                UnloadCell();
            }

            other.GetComponent<Env_Item>().currentCell = null;
            other.GetComponent<Env_Item>().lastCell = gameObject;

            items.Remove(other.gameObject);
        }
        if (other.CompareTag("NPC")
            && AI.Contains(other.gameObject))
        {
            other.GetComponent<AI_Movement>().currentCell = null;
            other.GetComponent<AI_Movement>().lastCell = gameObject;

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
        par_Managers.GetComponent<Manager_Console>().currentCell = gameObject;
        par_Managers.GetComponent<Manager_Console>().lastCell = gameObject;

        if (isInteriorCell)
        {
            GlobalVolume.SetActive(false);
        }

        if (items.Count > 0)
        {
            foreach (GameObject item in items)
            {
                if (!item.GetComponent<Env_Item>().disableAtStart)
                {
                    //Debug.Log("Loaded item '" + item.GetComponent<Env_Item>().str_ItemName + "' in cell '" + str_CellName + "'!");
                    item.GetComponent<Env_Item>().ActivateItem();
                }
            }
        }
        if (containers.Count > 0)
        {
            foreach (GameObject container in containers)
            {
                //Debug.Log("Loaded container '" + container.GetComponent<Inv_Container>().str_ContainerName + "' in cell '" + str_CellName + "'!");
                container.GetComponent<Inv_Container>().containerActivated = true;
            }
        }
        if (workbenches.Count > 0)
        {
            foreach (GameObject workbench in workbenches)
            {
                //Debug.Log("Loaded workbench '" + workbench.GetComponent<Env_Workbench>().str_workbenchName + "' in cell '" + str_CellName + "'!");
                workbench.GetComponent<Env_Workbench>().isActive = true;
            }
        }
        if (waitables.Count > 0)
        {
            foreach (GameObject waitable in waitables)
            {
                //Debug.Log("Loaded waitable '" + waitable.name + "' in cell '" + str_CellName + "'!");
                waitable.GetComponent<Env_Wait>().isActivated = true;
            }
        }
        if (destroyableCrates.Count > 0)
        {
            foreach (GameObject crate in destroyableCrates)
            {
                crate.GetComponent<Env_DestroyableCrate>().isActive = true;
            }
        }
        if (doors.Count > 0)
        {
            foreach (GameObject door in doors)
            {
                door.GetComponent<Env_Door>().isActive = true;
            }
        }
        if (AI.Count > 0)
        {
            foreach (GameObject npc in AI)
            {
                npc.GetComponent<AI_Movement>().canMove = true;
            }
        }
    }
    private void UnloadCell()
    {
        par_Managers.GetComponent<Manager_Console>().currentCell = null;
        par_Managers.GetComponent<Manager_Console>().lastCell = gameObject;

        if (!GlobalVolume.activeInHierarchy)
        {
            GlobalVolume.SetActive(true);
        }

        QuitelyUnloadCell();
    }
    public void QuitelyUnloadCell()
    {
        if (items.Count > 0)
        {
            foreach (GameObject item in items)
            {
                if (item == null)
                {
                    items.Remove(null);
                }
            }
        }
        if (containers.Count > 0)
        {
            foreach (GameObject container in containers)
            {
                container.GetComponent<Inv_Container>().containerActivated = false;
            }
        }
        if (workbenches.Count > 0)
        {
            foreach (GameObject workbench in workbenches)
            {
                workbench.GetComponent<Env_Workbench>().isActive = false;
            }
        }
        if (waitables.Count > 0)
        {
            foreach (GameObject waitable in waitables)
            {
                waitable.GetComponent<Env_Wait>().isActivated = false;
            }
        }
        if (waitables.Count > 0)
        {
            foreach (GameObject crate in destroyableCrates)
            {
                crate.GetComponent<Env_DestroyableCrate>().isActive = false;
            }
        }
        if (doors.Count > 0)
        {
            foreach (GameObject door in doors)
            {
                door.GetComponent<Env_Door>().isActive = false;
            }
        }
        if (AI.Count > 0)
        {
            foreach (GameObject npc in AI)
            {
                npc.GetComponent<AI_Movement>().canMove = false;
            }
        }
    }

    public void EnableCellTeleportButtonOnMap()
    {
        par_CellOnMap.SetActive(true);
        //Debug.Log("Discovered cell " + str_CellName + "!");
    }
}