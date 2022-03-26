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
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private Manager_Console ConsoleScript;

    //hidden but public variables
    [HideInInspector] public bool isPlayerAlive;
    [HideInInspector] public bool isTakingDamage;
    [HideInInspector] public bool canTakeDamage;
    [HideInInspector] public float health;
    [HideInInspector] public float mentalState;
    [HideInInspector] public float radiation;
    [HideInInspector] public List<GameObject> damageDealers = new List<GameObject>();

    //private variables
    private bool calledOnce;

    private void Awake()
    {
        isPlayerAlive = true;

        health = maxHealth;
        mentalState = maxMentalState;
        canTakeDamage = true;
        txt_PlayerDied.gameObject.SetActive(false);
    }

    private void Start()
    {
        UIReuseScript.health = health;
        UIReuseScript.maxHealth = maxHealth;
        UIReuseScript.UpdatePlayerHealth();

        UIReuseScript.mentalState = mentalState;
        UIReuseScript.maxMentalState = maxMentalState;
        UIReuseScript.UpdatePlayerMentalState();

        UIReuseScript.maxRadiation = maxRadiation;
        UIReuseScript.UpdatePlayerRadiation();
    }

    private void Update()
    {
        if (isTakingDamage && damageDealers.Count == 0)
        {
            isTakingDamage = false;
        }

        if (health > 0 
            && radiation < maxRadiation 
            && mentalState > 0)
        {
            if (UIReuseScript.health != health)
            {
                UIReuseScript.health = health;
                UIReuseScript.UpdatePlayerHealth();
            }
            if (UIReuseScript.maxHealth != maxHealth)
            {
                UIReuseScript.maxHealth = maxHealth;
                UIReuseScript.UpdatePlayerHealth();
            }

            if (UIReuseScript.mentalState != mentalState)
            {
                UIReuseScript.mentalState = mentalState;
                UIReuseScript.UpdatePlayerMentalState();
            }
            if (UIReuseScript.maxMentalState != maxMentalState)
            {
                UIReuseScript.maxMentalState = maxMentalState;
                UIReuseScript.UpdatePlayerMentalState();
            }

            if (UIReuseScript.radiation != radiation)
            {
                UIReuseScript.radiation = radiation;
                UIReuseScript.UpdatePlayerRadiation();
            }
            if (UIReuseScript.maxRadiation != maxRadiation)
            {
                UIReuseScript.maxRadiation = maxRadiation;
                UIReuseScript.UpdatePlayerRadiation();
            }

            if (mentalState < maxMentalState)
            {
                mentalState += 0.25f * Time.deltaTime;
            }
        }
        //player dies if health or mental state runs out or if radiation reaches max radiation
        else if (health <= 0 
                 || radiation >= maxRadiation 
                 || mentalState <= 0)
        {
            if (health <= 0)
            {
                health = 0;
                UIReuseScript.health = health;
                UIReuseScript.UpdatePlayerHealth();
                UIReuseScript.maxHealth = maxHealth;
                UIReuseScript.UpdatePlayerHealth();
            }
            if (mentalState <= 0)
            {
                mentalState = 0;
                UIReuseScript.mentalState = mentalState;
                UIReuseScript.UpdatePlayerMentalState();
                UIReuseScript.maxMentalState = maxMentalState;
                UIReuseScript.UpdatePlayerMentalState();
            }
            if (radiation >= maxRadiation)
            {
                radiation = maxRadiation;
                UIReuseScript.radiation = radiation;
                UIReuseScript.UpdatePlayerRadiation();
                UIReuseScript.maxRadiation = maxRadiation;
                UIReuseScript.UpdatePlayerRadiation();
            }
            Death();
        }
    }

    private void Death()
    {
        if (!calledOnce)
        {
            thePlayer.GetComponent<Player_Movement>().enabled = false;
            thePlayer.GetComponent<CharacterController>().enabled = false;
            thePlayer.AddComponent<Rigidbody>();
            thePlayer.GetComponent<Rigidbody>().isKinematic = true;

            txt_PlayerDied.gameObject.SetActive(true);
            if (health == 0)
            {
                txt_PlayerDied.text = "You won't get far in the wasteland if you can't take care of your health...";
            }
            else if (radiation == maxRadiation)
            {
                txt_PlayerDied.text = "You managed to fry your brains in a pit of radiation like a zombie. Should've brought protection.";
            }
            else if (mentalState == 0)
            {
                txt_PlayerDied.text = "You went insane trying to uncover the secrets of the reactor and the wasteland...";
            }

            isPlayerAlive = false;
            ConsoleScript.toggleAIDetection = false;

            calledOnce = true;
        }
    }
}