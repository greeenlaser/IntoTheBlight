using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_DamageType : MonoBehaviour
{
    [SerializeField] private DamageType damageType;
    [SerializeField] private enum DamageType
    {
        radiation,
        fire,
        electricity,
        gas,
        psy
    }
    [SerializeField] private DamageSeverity damageSeverity;
    [SerializeField] private enum DamageSeverity
    {
        minor,
        moderate,
        severe
    }
    [SerializeField] private int damage;
    [SerializeField] private int radiationDamage;
    [SerializeField] private int mentalDamage;
    [Tooltip("How many times a second does this damageType deal damage?")]
    [Range(1, 10)]
    [SerializeField] private int damageTimer = 1;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    //public but hidden variables
    [HideInInspector] public float damageProtection;
    [HideInInspector] public float mentalProtection;
    [HideInInspector] public float radiationProtection;

    //private variables
    private bool isDealingDamage;
    private bool dealtFirstDamage;
    private float timer;
    private GameObject thePlayer;

    private void OnTriggerEnter(Collider other)
    {
        //if player stepped into of damage collider
        if (other.CompareTag("Player") 
            && other.GetComponent<Player_Health>().health > 0
            && other.GetComponent<Player_Health>().canTakeDamage)
        {
            EnableDamageTypeLogo();

            isDealingDamage = true;

            other.GetComponent<Player_Health>().isTakingDamage = true;
            other.GetComponent<Player_Health>().damageDealers.Add(gameObject);

            if (thePlayer == null)
            {
                thePlayer = other.gameObject;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //if player stepped out of damage collider
        if (other.CompareTag("Player"))
        {
            DisableDamageTypeLogo();

            other.GetComponent<Player_Health>().damageDealers.Remove(gameObject);

            isDealingDamage = false;
            dealtFirstDamage = false;
        }
    }

    private void Update()
    {
        if (isDealingDamage)
        {
            if (!dealtFirstDamage)
            {
                timer = 0;
                dealtFirstDamage = true;
            }

            timer -= damageTimer * Time.deltaTime;
            if (timer <= 0)
            {
                if (damageType == DamageType.fire
                    || damageType == DamageType.electricity
                    || damageType == DamageType.gas)
                {
                    Damage();
                }
                else if (damageType == DamageType.radiation)
                {
                    RadiationDamage();
                }
                else if (damageType == DamageType.psy)
                {
                    MentalDamage();
                }
                timer = 1;
            }

            //if players health - damage dealt is less or equal to 0
            if (thePlayer.GetComponent<Player_Health>().health - damage <= 0)
            {
                thePlayer.GetComponent<Player_Health>().health = 0;
                isDealingDamage = false;
            }
            //if players mentalstate - mentaldamage dealt is less or equal to 0
            if (thePlayer.GetComponent<Player_Health>().mentalState - mentalDamage <= 0)
            {
                thePlayer.GetComponent<Player_Health>().mentalState = 0;
                isDealingDamage = false;
            }
            //if players radiation + radiationdamange dealt is over or equal to maxradiation
            if (thePlayer.GetComponent<Player_Health>().radiation + radiationDamage 
                >= thePlayer.GetComponent<Player_Health>().maxRadiation)
            {
                thePlayer.GetComponent<Player_Health>().radiation = thePlayer.GetComponent<Player_Health>().maxRadiation;
                isDealingDamage = false;
            }
        }
    }

    private void Damage()
    {
        float finalDamage = damage - damageProtection;
        if (finalDamage < 0)
        {
            finalDamage = 0;
        }

        thePlayer.GetComponent<Player_Health>().health -= finalDamage;
    }
    private void RadiationDamage()
    {
        float finalDamage = radiationDamage - radiationProtection;
        if (finalDamage < 0)
        {
            finalDamage = 0;
        }

        thePlayer.GetComponent<Player_Health>().radiation += finalDamage;

        //Debug.Log("Dealt " + finalDamage + " radiation damage. Players recieved radiation: " + thePlayer.GetComponent<Player_Health>().radiation + ".");
    }
    private void MentalDamage()
    {
        float finalDamage = mentalDamage - mentalProtection;
        if (finalDamage < 0)
        {
            finalDamage = 0;
        }

        thePlayer.GetComponent<Player_Health>().mentalState -= finalDamage;
    }

    private void EnableDamageTypeLogo()
    {
        if (damageType == DamageType.radiation)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorRadiationDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderateRadiationDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severeRadiationDamage.gameObject.SetActive(true);
            }
        }
        else if (damageType == DamageType.fire)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorFireDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderateFireDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severeFireDamage.gameObject.SetActive(true);
            }
        }
        else if (damageType == DamageType.electricity)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorElectricityDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderateElectricityDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severeElectricityDamage.gameObject.SetActive(true);
            }
        }
        else if (damageType == DamageType.gas)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorGasDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderateGasDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severeGasDamage.gameObject.SetActive(true);
            }
        }
        else if (damageType == DamageType.psy)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorPsyDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderatePsyDamage.gameObject.SetActive(true);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severePsyDamage.gameObject.SetActive(true);
            }
        }
    }
    private void DisableDamageTypeLogo()
    {
        if (damageType == DamageType.radiation)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorRadiationDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderateRadiationDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severeRadiationDamage.gameObject.SetActive(false);
            }
        }
        else if (damageType == DamageType.fire)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorFireDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderateFireDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severeFireDamage.gameObject.SetActive(false);
            }
        }
        else if (damageType == DamageType.electricity)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorElectricityDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderateElectricityDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severeElectricityDamage.gameObject.SetActive(false);
            }
        }
        else if (damageType == DamageType.gas)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorGasDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderateGasDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severeGasDamage.gameObject.SetActive(false);
            }
        }
        else if (damageType == DamageType.psy)
        {
            if (damageSeverity == DamageSeverity.minor)
            {
                UIReuseScript.minorPsyDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.moderate)
            {
                UIReuseScript.moderatePsyDamage.gameObject.SetActive(false);
            }
            else if (damageSeverity == DamageSeverity.severe)
            {
                UIReuseScript.severePsyDamage.gameObject.SetActive(false);
            }
        }
    }
}