using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Flashlight : MonoBehaviour
{
    [Header("Assignables")]
    [Range(50, 500)]
    [SerializeField] private float flashlightIntensity;
    [Range(5, 50)]
    [SerializeField] private float flashlightRange;
    [Range(0, 100)]
    [SerializeField] private float flashlightOuterAngle;
    [Range(0, 100)]
    [SerializeField] private float flashlightInnerAngle;
    [Range(1, 25)]
    [SerializeField] private float flashlightBatteryUsage;
    [SerializeField] private Light flashlight;

    [Header("Scripts")]
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isFlashlightEquipped;
    [HideInInspector] public bool isFlashlightEnabled;
    [HideInInspector] public bool isAssigningBattery;
    [HideInInspector] public GameObject battery;

    //private variables
    private float timer;
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
    }

    private void Update()
    {
        if (isFlashlightEquipped
            && !par_Managers.GetComponent<UI_PauseMenu>().isGamePaused
            && !par_Managers.GetComponent<Manager_Console>().consoleOpen
            && PlayerHealthScript.isPlayerAlive
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            if (Input.GetKeyDown(KeyCode.F)
                && battery != null
                && battery.GetComponent<Item_Battery>().currentBattery > 0)
            {
                isFlashlightEnabled = !isFlashlightEnabled;

                CheckForFlashlight();
            }

            if (isFlashlightEnabled)
            {
                timer += Time.deltaTime;
                if (timer > 0.5f)
                {
                    battery.GetComponent<Item_Battery>().currentBattery -= flashlightBatteryUsage;
                    UpdateBatteryRemainder();
                    timer = 0;
                }
            }
        }

        if (!isFlashlightEnabled
            && timer > 0)
        {
            timer = 0;
        }
    }

    public void AssignBattery()
    {
        foreach (GameObject item in PlayerInventoryScript.inventory)
        {
            if (item.GetComponent<Item_Battery>() != null
                && item.GetComponent<Item_Battery>().isBeingAssigned)
            {
                battery = item;
                break;
            }
        }

        battery.GetComponent<Item_Battery>().isBeingAssigned = false;
        battery.GetComponent<Item_Battery>().isInUse = true;

        ClearUI();

        UpdateBatteryRemainder();

        PlayerInventoryScript.ShowAll();
    }
    public void RemoveBattery()
    {
        isFlashlightEnabled = false;
        battery.GetComponent<Item_Battery>().isInUse = false;
        battery = null;

        flashlight.gameObject.SetActive(false);

        ClearUI();

        par_Managers.GetComponent<Manager_UIReuse>().ClearFlashlightUI();
    }
    public void UpdateBatteryRemainder()
    {
        if (battery.GetComponent<Item_Battery>().currentBattery < flashlightBatteryUsage)
        {
            flashlight.gameObject.SetActive(false);
            isFlashlightEnabled = false;
        }

        par_Managers.GetComponent<Manager_UIReuse>().flashlightBattery = battery.GetComponent<Item_Battery>().currentBattery;
        par_Managers.GetComponent<Manager_UIReuse>().flashlightMaxBattery = battery.GetComponent<Item_Battery>().maxBattery;
        par_Managers.GetComponent<Manager_UIReuse>().UpdatePlayerFlashlight();
    }

    public void CheckForFlashlight()
    {
        if (isFlashlightEnabled
            && !flashlight.gameObject.activeInHierarchy)
        {
            flashlight.gameObject.SetActive(true);
        }
        else if (!isFlashlightEnabled
                 && flashlight.gameObject.activeInHierarchy)
        {
            flashlight.gameObject.SetActive(false);
        }
    }

    public void EquipFlashlight()
    {
        try
        {
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.GetComponent<Item_Flashlight>() != null
                    && item.GetComponent<Item_Flashlight>().isFlashlightEquipped)
                {
                    item.GetComponent<Item_Flashlight>().UnequipFlashlight();
                }
            }

            PlayerInventoryScript.equippedFlashlight = gameObject;
            isFlashlightEquipped = true;

            ClearUI();

            gameObject.SetActive(true);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.transform.position = PlayerInventoryScript.pos_EquippedItem.position;

            if (battery != null)
            {
                UpdateBatteryRemainder();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: Something prevented equipping this flashlight!" + e + ".");
        }
    }
    public void UnequipFlashlight()
    {
        PlayerInventoryScript.equippedFlashlight = null;
        isFlashlightEquipped = false;
        isFlashlightEnabled = false;

        flashlight.gameObject.SetActive(false);

        ClearUI();

        UIReuseScript.ClearFlashlightUI();

        gameObject.SetActive(false);
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;
    }

    private void ClearUI()
    {
        gameObject.GetComponent<Env_Item>().RemoveListeners();
        UIReuseScript.ClearAllInventories();
        UIReuseScript.ClearInventoryUI();
        UIReuseScript.RebuildPlayerInventory();
        UIReuseScript.txt_InventoryName.text = "Player inventory";
        PlayerInventoryScript.UpdatePlayerInventoryStats();
    }
}