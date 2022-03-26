using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Door : MonoBehaviour
{
    [Header("Assignables")]
    public bool isProtected;
    public bool isLocked;
    public bool needsKey;
    [Range(1, 10)]
    [SerializeField] private float doorMoveSpeed;
    public Vector3 trigger_Open;
    public Vector3 trigger_Unlock;
    [SerializeField] private GameObject door;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private Transform pos_DoorOpen;
    [SerializeField] private Transform pos_DoorClosed;
    [SerializeField] private Env_Lock LockScript;

    //public but hidden variables
    [HideInInspector] public bool isActive;

    //private variables
    private bool openDoor;
    private bool closeDoor;
    private bool isClosed;
    private float doorDistanceFromEndPos;
    private List<GameObject> targetsInTrigger = new List<GameObject>();

    private void Start()
    {
        isClosed = true;
    }

    private void Update()
    {
        if (openDoor)
        {
            closeDoor = false;
            isClosed = false;

            doorDistanceFromEndPos = Vector3.Distance(pos_DoorOpen.position, door.transform.position);

            float step = doorMoveSpeed * Time.deltaTime;
            door.transform.position = Vector3.MoveTowards(door.transform.position, pos_DoorOpen.position, step);

            if (doorDistanceFromEndPos < 0.01f)
            {
                DoorIsOpen();
            }
            else if (doorDistanceFromEndPos > 0.01f && targetsInTrigger.Count == 0 && !closeDoor)
            {
                closeDoor = true;
            }
        }
        if (closeDoor)
        {
            openDoor = false;
            doorDistanceFromEndPos = Vector3.Distance(pos_DoorClosed.position, door.transform.position);

            float step = doorMoveSpeed * Time.deltaTime;
            door.transform.position = Vector3.MoveTowards(door.transform.position, pos_DoorClosed.position, step);

            if (doorDistanceFromEndPos < 0.01f)
            {
                DoorIsClosed();
            }
        }

        if (targetsInTrigger.Count == 0 && !isClosed && !closeDoor)
        {
            closeDoor = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActive
            && !isLocked 
            && (other.CompareTag("Player") 
            || other.CompareTag("NPC")))
        {
            if (!targetsInTrigger.Contains(other.gameObject))
            {
                targetsInTrigger.Add(other.gameObject);
            }

            if (isClosed && !openDoor)
            {
                openDoor = true;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (isActive
            && !isLocked
            && (other.CompareTag("Player") 
            || other.CompareTag("NPC")))
        {
            if (targetsInTrigger.Contains(other.gameObject))
            {
                targetsInTrigger.Remove(other.gameObject);
            }
        }
    }

    public void CheckIfKeyIsNeeded()
    {
        if (!needsKey)
        {
            bool hasLockpick = false;
            foreach (GameObject item in thePlayer.GetComponent<Inv_Player>().inventory)
            {
                if (item.GetComponent<Item_Lockpick>() != null
                    && item.GetComponent<Item_Lockpick>().itemType
                    == Item_Lockpick.ItemType.lockpick)
                {
                    hasLockpick = true;
                    break;
                }
            }
            if (hasLockpick)
            {
                LockScript.OpenLockUI();
            }
            else if (!hasLockpick)
            {
                //<<< play locked sound here <<<
                Debug.Log("Error: Did not find any lockpicks in players inventory!");
            }
        }
        else if (needsKey)
        {
            bool foundCorrectKey = false;
            foreach (GameObject item in thePlayer.GetComponent<Inv_Player>().inventory)
            {
                if (item.GetComponent<Item_Lockpick>() != null
                    && item.GetComponent<Item_Lockpick>().itemType == Item_Lockpick.ItemType.key
                    && item.GetComponent<Item_Lockpick>().targetLock == gameObject)
                {
                    foundCorrectKey = true;
                    break;
                }
            }
            if (foundCorrectKey)
            {
                //Debug.Log("Unlocked this door with key!");
                LockScript.Unlock();
            }
            else if (!foundCorrectKey)
            {
                //<<< play locked sound here <<<
                Debug.Log("Error: Did not find correct key in players inventory!");
            }
        }
    }

    private void DoorIsOpen()
    {
        closeDoor = false;
        openDoor = false;

        //Debug.Log("Door is open!");
    }
    private void DoorIsClosed()
    {
        closeDoor = false;
        openDoor = false;
        isClosed = true;

        //Debug.Log("Door is closed!");
    }
}