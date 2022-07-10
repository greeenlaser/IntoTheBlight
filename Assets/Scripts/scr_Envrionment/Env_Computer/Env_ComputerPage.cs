using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_ComputerPage : MonoBehaviour
{
    [Header("Assignables")]
    public string str_PageTitle;
    public string str_PageDescription;
    public List<GameObject> pages = new();

    [Header("Targets")]
    public bool canReuseTarget;
    public Env_Door targetDoor;

    [Header("Scripts")]
    public Env_ComputerManager ComputerManagerScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool targetIsEnabled;

    //private variables
    private Manager_UIReuse UIReuseManager;

    private void Awake()
    {
        UIReuseManager = par_Managers.GetComponent<Manager_UIReuse>();
    }

    public void LoadPageContent()
    {
        UIReuseManager.RebuildComputerPageList(gameObject);
    }

    public void OpenDoor()
    {
        if (targetDoor.isClosed
            && !targetDoor.openDoor
            && !targetIsEnabled
            && canReuseTarget)
        {
            OpenDoorFunction();
        }

        par_Managers.GetComponent<UI_RadioManager>().staticAudio.volume = 0;
    }
    private void OpenDoorFunction()
    {
        targetDoor.openDoor = true;
        targetIsEnabled = true;

        UIReuseManager.RebuildComputerPageList(gameObject);
    }

    public void CloseDoor()
    {
        if (!targetDoor.isClosed
            && !targetDoor.closeDoor
            && targetIsEnabled
            && canReuseTarget)
        {
            CloseDoorFunction();
        }

        par_Managers.GetComponent<UI_RadioManager>().staticAudio.volume = 0;
    }
    private void CloseDoorFunction()
    {
        targetDoor.forceCloseDoor = true;
        targetIsEnabled = false;

        UIReuseManager.RebuildComputerPageList(gameObject);
    }

    public void UnlockDoor()
    {
        if (!targetIsEnabled
            && !canReuseTarget)
        {
            UnlockDoorFunction();
        }

        par_Managers.GetComponent<UI_RadioManager>().staticAudio.volume = 0;
    }
    private void UnlockDoorFunction()
    {
        targetDoor.isLocked = false;
        targetDoor.openDoor = true;

        targetIsEnabled = true;

        UIReuseManager.RebuildComputerPageList(gameObject);
    }
}