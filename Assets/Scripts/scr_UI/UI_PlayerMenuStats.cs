using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayerMenuStats : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    //public but hidden variables
    [HideInInspector] public float health;
    [HideInInspector] public float maxHealth;
    [HideInInspector] public float stamina;
    [HideInInspector] public float maxStamina;
    [HideInInspector] public float mentalState;
    [HideInInspector] public float maxMentalState;
    [HideInInspector] public float radiation;
    [HideInInspector] public float maxRadiation;

    //updates all the stats displayed in the player menu stats UI
    public void GetStats()
    {
        health = PlayerHealthScript.health;
        maxHealth = PlayerHealthScript.maxHealth;
        UIReuseScript.txt_playerStatsHealth.text 
            = Mathf.FloorToInt(health).ToString() + "/" + Mathf.FloorToInt(maxHealth).ToString();

        float healthPercentage = health / maxHealth * 100;
        if (healthPercentage < 25)
        {
            UIReuseScript.txt_playerStatsHealth.color = Color.red;
        }
        else if (healthPercentage >= 25)
        {
            UIReuseScript.txt_playerStatsHealth.color = Color.green;
        }

        stamina = PlayerMovementScript.currentStamina;
        maxStamina = PlayerMovementScript.maxStamina;
        UIReuseScript.txt_playerStatsStamina.text 
            = Mathf.FloorToInt(stamina).ToString() + "/" + Mathf.FloorToInt(maxStamina).ToString();

        float staminaPercentage = stamina / maxStamina * 100;
        if (staminaPercentage < 25)
        {
            UIReuseScript.txt_playerStatsStamina.color = Color.red;
        }
        else if (staminaPercentage >= 25)
        {
            UIReuseScript.txt_playerStatsStamina.color = Color.green;
        }

        mentalState = PlayerHealthScript.mentalState;
        maxMentalState = PlayerHealthScript.maxMentalState;
        UIReuseScript.txt_playerStatsMentalState.text 
            = Mathf.FloorToInt(mentalState).ToString() + "/" + Mathf.FloorToInt(maxMentalState).ToString();

        float mentalStatePercentage = mentalState / maxMentalState * 100;
        if (mentalStatePercentage < 25)
        {
            UIReuseScript.txt_playerStatsMentalState.color = Color.red;
        }
        else if (mentalStatePercentage >= 25)
        {
            UIReuseScript.txt_playerStatsMentalState.color = Color.green;
        }

        radiation = PlayerHealthScript.radiation;
        maxRadiation = PlayerHealthScript.maxRadiation;
        UIReuseScript.txt_playerStatsRadiation.text 
            = Mathf.FloorToInt(radiation).ToString() + "/" + Mathf.FloorToInt(maxRadiation).ToString();

        float radiationPercentage = radiation / maxRadiation * 100;
        if (radiationPercentage > 75)
        {
            UIReuseScript.txt_playerStatsRadiation.color = Color.red;
        }
        else if (radiationPercentage < 75)
        {
            UIReuseScript.txt_playerStatsRadiation.color = Color.green;
        }

        //shows player equipped gun stats if the player has equipped a gun
        if (PlayerInventoryScript.equippedGun != null)
        {
            //gun name
            UIReuseScript.txt_gunName.text = PlayerInventoryScript.equippedGun.GetComponent<Env_Item>().str_ItemName;
            //shows equipped gun durability
            float gunDurability = PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().durability;
            float gunMaxDurability = PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().maxDurability;
            UIReuseScript.txt_gunDurability.text = "Durability:";
            UIReuseScript.txt_gunDurabilityValue.text = Mathf.FloorToInt(gunDurability) + "/" + Mathf.FloorToInt(gunMaxDurability); 
            //changes the color of the gun durability text
            //based off of the gun durability percentage from max gun durability
            if (gunDurability / gunMaxDurability * 100 < 25)
            {
                UIReuseScript.txt_gunDurabilityValue.color = Color.red;
            }
            else if (gunMaxDurability / gunMaxDurability * 100 >= 25)
            {
                UIReuseScript.txt_gunDurabilityValue.color = Color.green;
            }

            //shows handgun info
            if (PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().repairKitTypeRequired.ToString() 
                == Item_Consumable.RepairKitType.LightGun_Tier1.ToString()
                || PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().repairKitTypeRequired.ToString()
                == Item_Consumable.RepairKitType.LightGun_Tier2.ToString()
                || PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().repairKitTypeRequired.ToString()
                == Item_Consumable.RepairKitType.LightGun_Tier3.ToString())
            {
                UIReuseScript.par_EquippedHandgun.SetActive(true);
            }
        }
    }
}