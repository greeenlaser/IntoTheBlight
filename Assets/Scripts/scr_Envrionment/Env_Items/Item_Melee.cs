using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Melee : MonoBehaviour
{
    public float int_maxDamage;
    [SerializeField] private float swingTimerLimit;
    public float maxDurability;
    [SerializeField] private float durabilityReducedPerHit;
    [SerializeField] private Vector3 correctHoldRotation;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Player_MeleeRangeTargets MeleeTargetsScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private UI_PauseMenu PausemenuScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    //public but hidden variables
    [HideInInspector] public bool hasEquippedMeleeWeapon;
    [HideInInspector] public float durability;
    [HideInInspector] public float int_damage;

    //private variables
    private bool isSwinging;
    private float swingTimer;

    private void Awake()
    {
        //durability is random value between third of max durability and 8/10ths of max durability
        //durability = Mathf.FloorToInt(Random.Range(maxDurability / 3, maxDurability / 10 * 8));

        durability = maxDurability;

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
            int itemDamage = Mathf.FloorToInt(int_maxDamage / 100 * durabilityPercentage);
            //assign new damage to weapon
            int_damage = Mathf.FloorToInt(itemDamage);
        }
        else
        {
            int_damage = int_maxDamage;
        }
    }

    private void Update()
    {
        if (!ConsoleScript.consoleOpen
            && !PausemenuScript.isGamePaused
            && PlayerHealthScript.health > 0
            && hasEquippedMeleeWeapon)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !isSwinging)
            {
                Swing();
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
                MeleeTargetsScript.targets[i].GetComponent<AI_Health>().currentHealth -= Mathf.FloorToInt(int_damage);
                //Debug.Log("Dealt " + meleeDamage + " damage to " + MeleeTargetsScript.targets[i].GetComponent<UI_AIContent>().str_NPCName + "! Targets remaining health is " + MeleeTargetsScript.targets[i].GetComponent<AI_Health>().currentHealth + ".");
            }
            //deals damage to all non-broken destroyable crates in melee range
            if (MeleeTargetsScript.targets[i].GetComponentInParent<Env_DestroyableCrate>() != null
                && MeleeTargetsScript.targets[i].GetComponentInParent<Env_DestroyableCrate>().crateHealth > 0)
            {
                MeleeTargetsScript.targets[i].GetComponentInParent<Env_DestroyableCrate>().crateHealth -= Mathf.FloorToInt(int_damage);
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
                int itemDamage = Mathf.FloorToInt(int_maxDamage / 100 * durabilityPercentage);
                //assign new damage to weapon
                int_damage = itemDamage;
            }
            else
            {
                int_damage = int_maxDamage;
            }

            UIReuseScript.durability = durability;
            UIReuseScript.maxDurability = maxDurability;
            UIReuseScript.UpdateWeaponQuality();

            hitSomething = false;
        }
    }

    public void EquipMeleeWeapon()
    {
        UIReuseScript.ClearGrenadeUI();
        UIReuseScript.ClearWeaponUI();

        //unequips previously equipped melee weapon if there is any
        foreach (GameObject meleeWeapon in PlayerInventoryScript.inventory)
        {
            if (meleeWeapon.GetComponent<Item_Melee>() != null
                && meleeWeapon.GetComponent<Item_Melee>().hasEquippedMeleeWeapon)
            {
                meleeWeapon.GetComponent<Item_Melee>().UnequipMeleeWeapon();
            }
        }
        //unequips previously equipped gun if there is any
        foreach (GameObject gun in PlayerInventoryScript.inventory)
        {
            if (gun.GetComponent<Item_Gun>() != null
                && gun.GetComponent<Item_Gun>().hasEquippedGun)
            {
                gun.GetComponent<Item_Gun>().UnequipGun();
            }
        }
        //unequips previously equipped grenade if there is any
        foreach (GameObject grenade in PlayerInventoryScript.inventory)
        {
            if (grenade.GetComponent<Item_Grenade>() != null
                && grenade.GetComponent<Item_Grenade>().hasEquippedGrenade)
            {
                grenade.GetComponent<Item_Grenade>().UnequipGrenade();
            }
        }

        //enables interpolation on the equipped melee weapon
        rb.interpolation = RigidbodyInterpolation.None;

        hasEquippedMeleeWeapon = true;

        gameObject.GetComponent<Env_Item>().RemoveListeners();
        UIReuseScript.ClearAllInventories();
        UIReuseScript.ClearInventoryUI();
        UIReuseScript.RebuildPlayerInventory();
        UIReuseScript.txt_InventoryName.text = "Player inventory";
        PlayerInventoryScript.UpdatePlayerInventoryStats();
        PlayerInventoryScript.equippedGun = gameObject;

        gameObject.SetActive(true);
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.transform.parent = PlayerInventoryScript.pos_EquippedItem.transform;
        gameObject.transform.position = PlayerInventoryScript.pos_EquippedItem.position;
        gameObject.transform.localRotation = Quaternion.Euler(correctHoldRotation);

        UIReuseScript.durability = durability;
        UIReuseScript.maxDurability = maxDurability;
        UIReuseScript.UpdateWeaponQuality();

        //Debug.Log("Equipped " + gameObject.GetComponent<Env_Item>().str_ItemName + "!");
    }
    public void UnequipMeleeWeapon()
    {
        if (!isSwinging)
        {
            hasEquippedMeleeWeapon = false;

            //enables interpolation on the unequipped gun
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            gameObject.GetComponent<Env_Item>().RemoveListeners();
            UIReuseScript.ClearAllInventories();
            UIReuseScript.ClearInventoryUI();
            UIReuseScript.RebuildPlayerInventory();
            UIReuseScript.txt_InventoryName.text = "Player inventory";
            PlayerInventoryScript.UpdatePlayerInventoryStats();
            PlayerInventoryScript.equippedGun = null;
            UIReuseScript.ClearWeaponUI();

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