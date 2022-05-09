using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Melee : MonoBehaviour
{
    public float maxDamage;
    [SerializeField] private float swingTimerLimit;
    public float maxDurability;
    [SerializeField] private float durabilityReducedPerHit;
    [SerializeField] private Vector3 correctHoldRotation;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Player_MeleeRangeTargets MeleeTargetsScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool hasEquippedMeleeWeapon;
    [HideInInspector] public float durability;
    [HideInInspector] public float damage;

    //private variables
    private bool isSwinging;
    private float swingTimer;

    private void Awake()
    {
        durability = maxDurability;

        LoadValues();
    }

    private void Update()
    {
        if (!par_Managers.GetComponent<Manager_Console>().consoleOpen
            && !par_Managers.GetComponent<UI_PauseMenu>().isGamePaused
            && PlayerHealthScript.health > 0
            && hasEquippedMeleeWeapon
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !isSwinging)
            {
                if (durability > 0)
                {
                    Swing();
                }
                else
                {
                    Debug.LogWarning("Error: " + name + " cannot be used because it is broken!");
                }
            }

            if (isSwinging)
            {
                swingTimer += Time.deltaTime;

                if (swingTimer > swingTimerLimit)
                {
                    isSwinging = false;
                }
            }
        }
    }

    private void Swing()
    {
        isSwinging = true;
        bool hitSomething = false;

        for (int i = 0; i < MeleeTargetsScript.targets.Count; i++)
        {
            //deals damage to all alive and killable AI in melee range
            if (MeleeTargetsScript.targets[i].GetComponent<AI_Health>() != null
                && MeleeTargetsScript.targets[i].GetComponent<AI_Health>().isAlive
                && MeleeTargetsScript.targets[i].GetComponent<AI_Health>().isKillable)
            {
                MeleeTargetsScript.targets[i].GetComponent<AI_Health>().DealDamage(Mathf.Round(damage * 10) / 10);
                //Debug.Log("Dealt " + meleeDamage + " damage to " + MeleeTargetsScript.targets[i].GetComponent<UI_AIContent>().str_NPCName + "! Targets remaining health is " + MeleeTargetsScript.targets[i].GetComponent<AI_Health>().currentHealth + ".");
            }
            //deals damage to all non-broken destroyable crates in melee range
            if (MeleeTargetsScript.targets[i].GetComponentInParent<Env_DestroyableCrate>() != null
                && MeleeTargetsScript.targets[i].GetComponentInParent<Env_DestroyableCrate>().crateHealth > 0)
            {
                MeleeTargetsScript.targets[i].GetComponentInParent<Env_DestroyableCrate>().DealDamage(Mathf.Round(damage * 10) / 10);
            }

            //protected melee weapons dont take durability damage
            if (!hitSomething && !gameObject.GetComponent<Env_Item>().isProtected)
            {
                hitSomething = true;
            }
        }

        if (hitSomething)
        {
            durability -= durabilityReducedPerHit;

            LoadValues();

            par_Managers.GetComponent<Manager_UIReuse>().durability = durability;
            par_Managers.GetComponent<Manager_UIReuse>().maxDurability = maxDurability;
            par_Managers.GetComponent<Manager_UIReuse>().UpdateWeaponQuality();
        }
    }

    public void LoadValues()
    {
        //update weapon value and damage based off of current durability
        if (durability < maxDurability / 100 * 75)
        {
            //get weapon max value
            int itemValue = gameObject.GetComponent<Env_Item>().int_maxItemValue;
            //get weapon current durability percentage from max durability
            float durabilityPercentage = (durability / maxDurability) * 100;
            //calculate new weapon value according to weapon durability percentage
            itemValue = Mathf.FloorToInt(itemValue / 100 * durabilityPercentage);
            //assign weapon value
            gameObject.GetComponent<Env_Item>().int_ItemValue = itemValue;

            //calculate new weapon damage according to weapon durability percentage
            float itemDamage = Mathf.Floor(maxDamage / 100 * durabilityPercentage * 10) / 10;
            //assign new damage to weapon
            damage = itemDamage;

            //melee weapon damage can never go below 10% max damage
            if (damage < Mathf.FloorToInt(maxDamage / 10))
            {
                damage = Mathf.FloorToInt(maxDamage / 10);
            }
        }
        else
        {
            damage = maxDamage;

            gameObject.GetComponent<Env_Item>().int_ItemValue = gameObject.GetComponent<Env_Item>().int_maxItemValue;
        }
    }

    public void EquipMeleeWeapon()
    {
        try
        {
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.GetComponent<Item_Gun>() != null
                    && item.GetComponent<Item_Gun>().hasEquippedGun)
                {
                    item.GetComponent<Item_Gun>().UnequipGun();
                }
                else if (item.GetComponent<Item_Melee>() != null
                         && item.GetComponent<Item_Melee>().hasEquippedMeleeWeapon)
                {
                    item.GetComponent<Item_Melee>().UnequipMeleeWeapon();
                }
                else if (item.GetComponent<Item_Grenade>() != null
                         && item.GetComponent<Item_Grenade>().hasEquippedGrenade)
                {
                    item.GetComponent<Item_Grenade>().UnequipGrenade();
                }
            }

            par_Managers.GetComponent<Manager_UIReuse>().ClearGrenadeUI();
            par_Managers.GetComponent<Manager_UIReuse>().ClearWeaponUI();

            //enables interpolation on the equipped melee weapon
            rb.interpolation = RigidbodyInterpolation.None;

            hasEquippedMeleeWeapon = true;

            gameObject.GetComponent<Env_Item>().RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
            PlayerInventoryScript.UpdatePlayerInventoryStats();
            PlayerInventoryScript.equippedGun = gameObject;

            gameObject.SetActive(true);
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.transform.parent = PlayerInventoryScript.pos_EquippedItem.transform;
            gameObject.transform.position = PlayerInventoryScript.pos_EquippedItem.position;
            gameObject.transform.localRotation = Quaternion.Euler(correctHoldRotation);

            par_Managers.GetComponent<Manager_UIReuse>().durability = durability;
            par_Managers.GetComponent<Manager_UIReuse>().maxDurability = maxDurability;
            par_Managers.GetComponent<Manager_UIReuse>().UpdateWeaponQuality();

            //Debug.Log("Equipped " + gameObject.GetComponent<Env_Item>().str_ItemName + "!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error: Something prevented equipping this melee weapon!" + e + ".");
        }
    }
    public void UnequipMeleeWeapon()
    {
        if (!isSwinging)
        {
            hasEquippedMeleeWeapon = false;

            //enables interpolation on the unequipped gun
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            gameObject.GetComponent<Env_Item>().RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
            PlayerInventoryScript.UpdatePlayerInventoryStats();
            PlayerInventoryScript.equippedGun = null;
            par_Managers.GetComponent<Manager_UIReuse>().ClearWeaponUI();

            gameObject.SetActive(false);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            transform.parent = PlayerInventoryScript.par_PlayerItems.transform;
            transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;

            //Debug.Log("Unequipped " + gameObject.GetComponent<Env_Item>().str_ItemName + "!");
        }
        else if (isSwinging)
        {
            Debug.LogWarning("Error: Cannot unequip this melee weapon because it is currently being used!");
        }
    }
}