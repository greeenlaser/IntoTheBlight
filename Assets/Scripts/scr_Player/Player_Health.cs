using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player_Health : MonoBehaviour
{
    public float maxHealth;
    public float maxMentalState;
    public float maxRadiation;

    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private TMP_Text txt_PlayerDied;
    [SerializeField] private GameObject par_Managers;

    //hidden but public variables
    [HideInInspector] public bool isPlayerAlive;
    [HideInInspector] public bool canTakeDamage;
    [HideInInspector] public float health;
    [HideInInspector] public float mentalState;
    [HideInInspector] public float radiation;
    [HideInInspector] public List<GameObject> elementalDamageDealers;

    //private variables
    private Manager_UIReuse UIScript;
    private readonly List<string> damageTypes = new();

    //damage variables
    private bool enableTimer;
    private float timer;
    private float secondTimer;
    private float oldHealth;
    private float oldRad;
    private float oldMental;

    private void Awake()
    {
        isPlayerAlive = true;
        canTakeDamage = true;

        health = maxHealth;
        mentalState = maxMentalState;
        
        txt_PlayerDied.gameObject.SetActive(false);
    }

    private void Start()
    {
        UIScript = par_Managers.GetComponent<Manager_UIReuse>();

        UIScript.health = health;
        UIScript.maxHealth = maxHealth;
        UIScript.UpdatePlayerHealth();

        UIScript.mentalState = mentalState;
        UIScript.maxMentalState = maxMentalState;
        UIScript.UpdatePlayerMentalState();

        UIScript.maxRadiation = maxRadiation;
        UIScript.UpdatePlayerRadiation();
    }

    private void Update()
    {
        if(elementalDamageDealers.Count == 0
           && damageTypes.Count > 0)
        {
            damageTypes.Clear();
        }

        if (enableTimer)
        {
            CallAllLogos();

            timer += Time.deltaTime;

            if (timer > 0.5f)
            {
                DisableAllDamageLogos();

                secondTimer += Time.deltaTime;

                if (secondTimer > 0.5f)
                {
                    if (oldHealth != health
                        || oldRad != radiation
                        || oldMental != mentalState)
                    {
                        ResetTimers();
                    }
                    else
                    {
                        StopTimers();
                    }
                }
            }
        }
    }

    public void DealDamage(string damageDealer, string damageType, float damageAmount)
    {
        if (damageAmount > 0
            && damageDealer != "")
        {
            if (damageType == "fall"
            || damageType == "melee"
            || damageType == "fragmentation"
            || damageType == "plasma"
            || damageType == "fire"
            || damageType == "electricity"
            || damageType == "gas")
            {
                if (health - damageAmount > 0)
                {
                    health -= damageAmount;
                }
                else
                {
                    health = 0;
                    Death("You won't get far in the wasteland if you can't take care of your health...");
                }

                UIScript.health = health;
                UIScript.maxHealth = maxHealth;
                UIScript.UpdatePlayerHealth();
            }
            else if (damageType == "radiation")
            {
                if (damageAmount + radiation < maxRadiation)
                {
                    radiation += damageAmount;
                }
                else
                {
                    radiation = 0;
                    Death("You managed to fry your brains in a pit of radiation like a zombie. Should've brought protection.");
                }

                UIScript.radiation = radiation;
                UIScript.maxRadiation = maxRadiation;
                UIScript.UpdatePlayerRadiation();
            }
            else if (damageType == "psy")
            {
                if (mentalState - damageAmount > 0)
                {
                    mentalState -= damageAmount;
                }
                else
                {
                    mentalState = 0;
                    Death("You went insane trying to uncover the secrets of the reactor and the wasteland...");
                }

                UIScript.mentalState = mentalState;
                UIScript.maxMentalState = maxMentalState;
                UIScript.UpdatePlayerMentalState();
            }

            oldHealth = health;
            oldRad = radiation;
            oldMental = mentalState;

            enableTimer = true;

            //Debug.Log(damageDealer + " dealt " + damageAmount + " " + damageSeverity + " " + damageType + " damage to player.");
        }
    }

    public void Death(string deathMessage)
    {
        thePlayer.GetComponent<Player_Movement>().enabled = false;
        thePlayer.GetComponent<CharacterController>().enabled = false;
        thePlayer.AddComponent<Rigidbody>();
        thePlayer.GetComponent<Rigidbody>().isKinematic = true;

        txt_PlayerDied.gameObject.SetActive(true);
        txt_PlayerDied.text = deathMessage;

        isPlayerAlive = false;
        par_Managers.GetComponent<Manager_Console>().toggleAIDetection = false;

        health = 0;
        UIScript.UpdatePlayerHealth();
    }

    private void CallAllLogos()
    {
        UpdateDamageSeverityList();

        ToggleFireDamageLogos();
        ToggleElectricityDamageLogos();
        ToggleGasDamageLogos();
        ToggleRadiationDamageLogos();
        ToggleMentalDamageLogos();
    }
    private void ResetTimers()
    {
        damageTypes.Clear();

        timer = 0;
        secondTimer = 0;
    }
    private void StopTimers()
    {
        damageTypes.Clear();

        enableTimer = false;
        timer = 0;
        secondTimer = 0;
    }

    private void ToggleFireDamageLogos()
    {
        if (damageTypes.Contains("fireMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorFireDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("fireMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorFireDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("fireModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderateFireDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("fireModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderateFireDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("fireSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severeFireDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("fireSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severeFireDamage.gameObject.SetActive(false);
        }
    }
    private void ToggleElectricityDamageLogos()
    {
        if (damageTypes.Contains("elecMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorElectricityDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("elecMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorElectricityDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("elecModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderateElectricityDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("elecModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderateElectricityDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("elecSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severeElectricityDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("elecSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severeElectricityDamage.gameObject.SetActive(false);
        }
    }
    private void ToggleGasDamageLogos()
    {
        if (damageTypes.Contains("gasMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorGasDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("gasMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorGasDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("gasModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderateGasDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("gasModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderateGasDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("gasSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severeGasDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("gasSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severeGasDamage.gameObject.SetActive(false);
        }
    }
    private void ToggleRadiationDamageLogos()
    {
        if (damageTypes.Contains("radMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorRadiationDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("radMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorRadiationDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("radModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderateRadiationDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("radModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderateRadiationDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("radSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severeRadiationDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("radSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severeRadiationDamage.gameObject.SetActive(false);
        }
    }
    private void ToggleMentalDamageLogos()
    {
        if (damageTypes.Contains("mentalMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorPsyDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("mentalMinor"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().minorPsyDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("mentalModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderatePsyDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("mentalModerate"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().moderatePsyDamage.gameObject.SetActive(false);
        }

        if (damageTypes.Contains("mentalSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severePsyDamage.gameObject.SetActive(true);
        }
        else if (!damageTypes.Contains("mentalSevere"))
        {
            par_Managers.GetComponent<Manager_UIReuse>().severePsyDamage.gameObject.SetActive(false);
        }
    }
    private void DisableAllDamageLogos()
    {
        par_Managers.GetComponent<Manager_UIReuse>().minorFireDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().moderateFireDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().severeFireDamage.gameObject.SetActive(false);

        par_Managers.GetComponent<Manager_UIReuse>().minorElectricityDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().moderateElectricityDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().severeElectricityDamage.gameObject.SetActive(false);

        par_Managers.GetComponent<Manager_UIReuse>().minorGasDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().moderateGasDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().severeGasDamage.gameObject.SetActive(false);

        par_Managers.GetComponent<Manager_UIReuse>().minorRadiationDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().moderateRadiationDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().severeRadiationDamage.gameObject.SetActive(false);

        par_Managers.GetComponent<Manager_UIReuse>().minorPsyDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().moderatePsyDamage.gameObject.SetActive(false);
        par_Managers.GetComponent<Manager_UIReuse>().severePsyDamage.gameObject.SetActive(false);
    }

    private void UpdateDamageSeverityList()
    {
        foreach (GameObject damageDealer in elementalDamageDealers)
        {
            if (damageDealer.GetComponent<Env_DamageType>().damageType
                == Env_DamageType.DamageType.fire)
            {
                if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                    == "minor"
                    && !damageTypes.Contains("fireMinor"))
                {
                    damageTypes.Add("fireMinor");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "moderate"
                         && !damageTypes.Contains("fireModerate"))
                {
                    damageTypes.Add("fireModerate");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "severe"
                         && !damageTypes.Contains("fireSevere"))
                {
                    damageTypes.Add("fireSevere");
                }
            }
            
            if (damageDealer.GetComponent<Env_DamageType>().damageType
                     == Env_DamageType.DamageType.electricity)
            {
                if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                    == "minor"
                    && !damageTypes.Contains("elecMinor"))
                {
                    damageTypes.Add("elecMinor");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "moderate"
                         && !damageTypes.Contains("elecModerate"))
                {
                    damageTypes.Add("elecModerate");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "severe"
                         && !damageTypes.Contains("elecSevere"))
                {
                    damageTypes.Add("elecSevere");
                }
            }

            if (damageDealer.GetComponent<Env_DamageType>().damageType
                     == Env_DamageType.DamageType.gas)
            {
                if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                    == "minor"
                    && !damageTypes.Contains("gasMinor"))
                {
                    damageTypes.Add("gasMinor");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "moderate"
                         && !damageTypes.Contains("gasModerate"))
                {
                    damageTypes.Add("gasModerate");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "severe"
                         && !damageTypes.Contains("gasSevere"))
                {
                    damageTypes.Add("gasSevere");
                }
            }

            if (damageDealer.GetComponent<Env_DamageType>().damageType
                     == Env_DamageType.DamageType.radiation)
            {
                if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                    == "minor"
                    && !damageTypes.Contains("radMinor"))
                {
                    damageTypes.Add("radMinor");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "moderate"
                         && !damageTypes.Contains("radModerate"))
                {
                    damageTypes.Add("radModerate");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "severe"
                         && !damageTypes.Contains("radSevere"))
                {
                    damageTypes.Add("radSevere");
                }
            }

            if (damageDealer.GetComponent<Env_DamageType>().damageType
                     == Env_DamageType.DamageType.psy)
            {
                if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                    == "minor"
                    && !damageTypes.Contains("mentalMinor"))
                {
                    damageTypes.Add("mentalMinor");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "moderate"
                         && !damageTypes.Contains("mentalModerate"))
                {
                    damageTypes.Add("mentalModerate");
                }
                else if (damageDealer.GetComponent<Env_DamageType>().damageDealerSeverity
                         == "severe"
                         && !damageTypes.Contains("mentalSevere"))
                {
                    damageTypes.Add("mentalSevere");
                }
            }
        }
    }
}