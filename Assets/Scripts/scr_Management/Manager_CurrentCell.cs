using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_CurrentCell : MonoBehaviour
{
    [Header("Assignables")]
    public string str_CellName;
    public Transform currentCellSpawnpoint;
    [SerializeField] private Manager_Console ConsoleScript;

    [Header("Teleportable cell")]
    public bool canBeTeleportedTo;
    [SerializeField] private GameObject par_CellOnMap;

    [Header("Toggle entire cell")]
    [SerializeField] private bool toggleEntireCell;
    [SerializeField] private GameObject par_entireCell;

    [Header("Toggle global volume")]
    [SerializeField] private bool isInteriorCell;
    [SerializeField] private GameObject GlobalVolume;

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
        }
        if (other.CompareTag("Item")
            && !other.GetComponent<Env_Item>().isInPlayerInventory
            && !other.GetComponent<Env_Item>().isInContainer
            && !other.GetComponent<Env_Item>().isInTraderShop
            && !items.Contains(other.gameObject))
        {
            items.Add(other.gameObject);
            if (!isPlayerInCell)
            {
                LoadCell();
                UnloadCell();
            }
        }
        if (other.CompareTag("NPC")
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
        if (other.CompareTag("Item")
            && !other.GetComponent<Env_Item>().isInPlayerInventory
            && !other.GetComponent<Env_Item>().isInContainer
            && !other.GetComponent<Env_Item>().isInTraderShop
            && items.Contains(other.gameObject))
        {
            items.Remove(other.gameObject);
            if (!isPlayerInCell)
            {
                LoadCell();
                UnloadCell();
            }
        }
        if (other.CompareTag("NPC")
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
        ConsoleScript.currentCell = gameObject;
        ConsoleScript.lastCell = gameObject;

        if (toggleEntireCell)
        {
            par_entireCell.SetActive(true);
        }
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
    }
    private void UnloadCell()
    {
        ConsoleScript.currentCell = null;
        ConsoleScript.lastCell = gameObject;

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

        if (toggleEntireCell)
        {
            par_entireCell.SetActive(false);
        }
    }

    public void EnableCellTeleportButtonOnMap()
    {
        par_CellOnMap.SetActive(true);
        //Debug.Log("Discovered cell " + str_CellName + "!");
    }
}