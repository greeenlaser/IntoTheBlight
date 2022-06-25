using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Lockpick : MonoBehaviour
{
    [Header("Assignables")]
    public ItemType itemType;
    public enum ItemType
    {
        key,
        lockpick
    }

    [Header("Key assignables")]
    public GameObject targetLock;

    [Header("Lockpick assignables")]
    public float maxLockpickDurability;

    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public float lockpickDurability;
    [HideInInspector] public Env_Lock LockScript;

    private void Start()
    {
        lockpickDurability = maxLockpickDurability;
    }

    public void UseLockpick()
    {
        if (lockpickDurability > 0)
        {
            lockpickDurability -= 3f * LockScript.lockpickingDifficulty;
            //Debug.Log("Lockpick durability is: " + lockpickDurability);
        }
        else if (lockpickDurability <= 0)
        {
            DestroyLockpick();
        }
    }
    private void DestroyLockpick()
    {
        if (gameObject.GetComponent<Env_Item>().int_itemCount == 1)
        {
            PlayerInventoryScript.inventory.Remove(gameObject);

            par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(gameObject.GetComponent<Env_Item>().str_ItemName);

            gameObject.GetComponent<Env_Item>().isInPlayerInventory = false;

            par_Managers.GetComponent<Manager_UIReuse>().txt_RemainingLockpicks.text = "0";

            LockScript.CloseLockUI();

            //Debug.Log("Broke last lockpick!");

            Destroy(gameObject);
        }
        else if (gameObject.GetComponent<Env_Item>().int_itemCount > 1)
        {
            gameObject.GetComponent<Env_Item>().int_itemCount--;
            par_Managers.GetComponent<Manager_UIReuse>().txt_RemainingLockpicks.text = gameObject.GetComponent<Env_Item>().int_itemCount.ToString();
            lockpickDurability = maxLockpickDurability;
            //Debug.Log("Broke a lockpick! Remaining lockpicks: " + gameObject.GetComponent<Env_Item>().int_itemCount + ".");

            LockScript.ResetLock();
        }
    }
}