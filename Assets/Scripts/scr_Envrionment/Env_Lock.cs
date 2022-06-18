using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_Lock : MonoBehaviour
{
    [Header("Assignables")]
    [Range(1, 3)]
    public int lockpickingDifficulty = 1;
    [SerializeField] private Target target;
    [SerializeField] private enum Target
    {
        door,
        container
    }
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    [Header("For doors")]
    [SerializeField] private Env_Door DoorScript;

    [Header("For containers")]
    [SerializeField] private Inv_Container ContainerScript;

    //public but hidden variables
    [HideInInspector] public bool isPickingLock;

    //private variables
    private float lockZRot;
    private float positiveResult;
    private float negativeResult;
    private float lockCorrectRotation;
    private float timer;
    private GameObject lockpick;

    private void Start()
    {
        lockCorrectRotation = Random.Range(6, 354);

        if (target == Target.door)
        {
            if (gameObject.GetComponent<Env_Door>().isLocked)
            {
                transform.localScale = DoorScript.trigger_Unlock;
            }
            else if (!gameObject.GetComponent<Env_Door>().isLocked)
            {
                transform.localScale = DoorScript.trigger_Open;
            }
        }

        positiveResult = lockCorrectRotation + 2;
        negativeResult = lockCorrectRotation - 2;
        //Debug.Log("New value: " + lockCorrectRotation + ", new min value: " + negativeResult + ", new max value: " + positiveResult + ".");

        //assigns a new correct rotation until current rotation min and max value is 5 units over correct rotation
        if (lockZRot > negativeResult && lockZRot < positiveResult)
        {
            StartCoroutine(AssignNewCorrectRotation());
        }
    }

    private void Update()
    {
        if (isPickingLock)
        {
            //looks for a lockpick in the player inventory
            if (lockpick == null)
            {
                if (timer > 0)
                {
                    timer = 0;
                }

                bool foundLockpick = false;
                foreach (GameObject item in PlayerInventoryScript.inventory)
                {
                    if (item.GetComponent<Item_Lockpick>() != null
                        && item.GetComponent<Item_Lockpick>().itemType
                        == Item_Lockpick.ItemType.lockpick)
                    {
                        foundLockpick = true;
                        lockpick = item;
                        item.GetComponent<Item_Lockpick>().LockScript = gameObject.GetComponent<Env_Lock>();
                        par_Managers.GetComponent<Manager_UIReuse>().txt_RemainingLockpicks.text = lockpick.GetComponent<Env_Item>().int_itemCount.ToString();
                        break;
                    }
                }
                if (!foundLockpick)
                {
                    //Debug.Log("No more lockpicks left!");
                    if (target == Target.door)
                    {
                        //reset lock rotation
                        par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles
                        = new Vector3(par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.x,
                                      par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.y,
                                      0);

                        //close lock UI because player cant rotate lock without lockpicks
                        CloseLockUI();
                    }
                }
            }
            //if lockpick was found
            else if (lockpick != null)
            {
                //rotate lock left
                if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)
                    && lockZRot < 180)
                {
                    //get current lock z rotation
                    lockZRot = par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.z;
                    //rotate lock z rotation to left
                    lockZRot += 1.5f;
                    //assign new rotation to lock rotation
                    par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles
                        = new Vector3(par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.x,
                                      par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.y,
                                      lockZRot);

                    //lockpick durability degrades over time
                    timer += 0.1f;
                    //Debug.Log("Timer is: " + timer);
                    if (timer > 0.5f)
                    {
                        lockpick.GetComponent<Item_Lockpick>().UseLockpick();
                        timer = 0;
                    }

                    //checks if the current lock rotation and lock correct rotation are less than 5
                    if (lockZRot > negativeResult && lockZRot < positiveResult)
                    {
                        Unlock();
                    }
                }
                //rotate lock right
                else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)
                         && par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.GetComponent<RectTransform>().rotation.z > -0.02f
                         && par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.GetComponent<RectTransform>().rotation.z < 0.9998f)
                {
                    //get current lock z rotation
                    lockZRot = par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.z;
                    //rotate lock z rotation to right
                    lockZRot -= 1.5f;
                    //assign new rotation to lock rotation
                    par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles
                        = new Vector3(par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.x,
                                      par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.y,
                                      lockZRot);

                    //lockpick durability degrades over time
                    timer += 0.1f;
                    //Debug.Log("Timer is: " + timer);
                    if (timer > 0.5f)
                    {
                        lockpick.GetComponent<Item_Lockpick>().UseLockpick();
                        timer = 0;
                    }

                    //checks if the current lock rotation and lock correct rotation are less than 5
                    if (lockZRot > negativeResult && lockZRot < positiveResult)
                    {
                        Unlock();
                    }
                }
                //rotates lock Z rotation back to 0
                else if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                {
                    if (timer > 0)
                    {
                        timer = 0;
                    }

                    //get current lock z rotation
                    lockZRot = par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.z;
                    //rotates left
                    if (lockZRot >= 180.1f)
                    {
                        lockZRot += 10;
                        par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles
                        = new Vector3(par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.x,
                                      par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.y,
                                      lockZRot);
                        //fixes rotation to 0
                        if (lockZRot > 359.9f)
                        {
                            lockZRot = 0;
                            par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles
                                = new Vector3(par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.x,
                            par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.y,
                            0);
                        }
                    }
                    //rotates right
                    else if (lockZRot < 180.1f)
                    {
                        lockZRot -= 10;
                        par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles
                        = new Vector3(par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.x,
                                      par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.y,
                                      lockZRot);
                        //fixes rotation to 0
                        if (lockZRot <= 0.1f)
                        {
                            lockZRot = 0;
                            par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles
                                = new Vector3(par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.x,
                            par_Managers.GetComponent<Manager_UIReuse>().lockpick_Body.transform.eulerAngles.y,
                            0);
                        }
                    }
                }
            }
        }
    }

    public void Unlock()
    {
        if (target == Target.door)
        {
            gameObject.transform.localScale = DoorScript.trigger_Open;
            DoorScript.isLocked = false;
            CloseLockUI();
            //Debug.Log("Unlocked this door!");
        }
        else if (target == Target.container)
        {
            ContainerScript.isLocked = false;
            CloseLockUI();
            ContainerScript.CheckIfLocked();
            //Debug.Log("Unlocked this container!");
        }
    }
    public void OpenLockUI()
    {
        par_Managers.GetComponent<UI_PauseMenu>().PauseGameAndCloseUIAndResetBools();
        par_Managers.GetComponent<UI_PauseMenu>().canPauseGame = false;
        par_Managers.GetComponent<UI_PlayerMenu>().lockpickUI = gameObject;
        par_Managers.GetComponent<Manager_Console>().lockpickUI = gameObject;

        par_Managers.GetComponent<Manager_UIReuse>().par_Lock.SetActive(true);

        if (lockpickingDifficulty == 1)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_LockDifficulty.text = "easy";
        }
        else if (lockpickingDifficulty == 2)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_LockDifficulty.text = "moderate";
        }
        else if (lockpickingDifficulty == 1)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_LockDifficulty.text = "hard";
        }

        if (target == Target.door)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_LockStatus.text = "Picking door lock";
        }
        else if (target == Target.container)
        {
            par_Managers.GetComponent<Manager_UIReuse>().txt_LockStatus.text = "Picking container lock";
        }
        par_Managers.GetComponent<Manager_UIReuse>().btn_CancelLockpicking.onClick.AddListener(CloseLockUI);

        isPickingLock = true;
    }
    public void CloseLockUI()
    {
        par_Managers.GetComponent<Manager_UIReuse>().btn_CancelLockpicking.onClick.RemoveAllListeners();
        par_Managers.GetComponent<Manager_UIReuse>().txt_LockStatus.text = "";
        par_Managers.GetComponent<Manager_UIReuse>().par_Lock.SetActive(false);

        if (timer > 0)
        {
            timer = 0;
        }

        if (lockpick != null && lockpick.GetComponent<Item_Lockpick>().LockScript != null)
        {
            lockpick.GetComponent<Item_Lockpick>().LockScript = null;
        }
        lockpick = null;
        isPickingLock = false;

        par_Managers.GetComponent<UI_PauseMenu>().callPMCloseOnce = false;
        par_Managers.GetComponent<UI_PauseMenu>().UnpauseGame();
        par_Managers.GetComponent<UI_PauseMenu>().canPauseGame = true;
        par_Managers.GetComponent<UI_PlayerMenu>().lockpickUI = null;
        par_Managers.GetComponent<Manager_Console>().lockpickUI = null;
    }

    //assigns a new correct rotation until current rotation min and max value is 5 units over correct rotation
    private IEnumerator AssignNewCorrectRotation()
    {
        while (lockZRot > negativeResult && lockZRot < positiveResult)
        {
            yield return new WaitForSeconds(0.1f);
            lockCorrectRotation = Random.Range(6, 354);
            positiveResult = lockCorrectRotation + 2;
            negativeResult = lockCorrectRotation - 2;
            //Debug.Log("New value: " + lockCorrectRotation + ", new min value: " + negativeResult + ", new max value: " + positiveResult + ".");
        }
    }
}