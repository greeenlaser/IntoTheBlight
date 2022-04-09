using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Health : MonoBehaviour
{
    [Header("Assignables")]
    public bool isKillable;
    public bool isRespawnable;
    public int maxHealth;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject par_deadAILoot;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isAlive;
    [HideInInspector] public bool canBeHostile;
    [HideInInspector] public int currentHealth;

    //private variables
    private float lastHealth;
    private float timer;

    private void Start()
    {
        isAlive = true;
        currentHealth = maxHealth;

        canBeHostile = true;

        par_deadAILoot.SetActive(false);
    }

    private void Update()
    {
        if (isAlive && lastHealth != currentHealth)
        {
            //if ai health is over 25% then it is colored green
            if (currentHealth > maxHealth / 4)
            {
                NormalHealth();
            }
            //if ai health is 25% or less then it is colored yellow
            else if (currentHealth <= maxHealth / 4)
            {
                LowHealth();
            }
            //if ai health is 0
            else if (currentHealth <= 0)
            {
                Death();
            }

            lastHealth = currentHealth;
        }

        //starts countdown and checks player distance from dead AI
        //if time runs out or player is further than 25 meters from the looted dead AI
        //then the dead AI is deleted
        if (!isAlive 
            && !par_Managers.GetComponent<UI_PauseMenu>().isGamePaused
            && par_deadAILoot.GetComponent<Inv_Container>().hasLootedDeadAIInventoryOnce)
        {
            timer += Time.deltaTime;

            if (timer > 120)
            {
                Debug.Log("System: " + gameObject.GetComponent<UI_AIContent>().str_NPCName + " was destroyed after timer ran out.");
                Destroy(par_deadAILoot.GetComponent<Inv_Container>().discardableDeadNPC);
            }
        }
    }

    //currentHealth > 25%
    private void NormalHealth()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.green;
    }
    //currentHealth <= 25%
    private void LowHealth()
    {
        gameObject.GetComponent<Renderer>().material.color = Color.yellow;
    }
    //currentHealth <= 0
    public void Death()
    {
        gameObject.GetComponent<AI_Movement>().canMove = false;
        isAlive = false;

        gameObject.GetComponent<Renderer>().material.color = Color.red;
        gameObject.GetComponent<AI_Movement>().canMove = false;
        gameObject.GetComponent<NavMeshAgent>().isStopped = true;

        currentHealth = 0;

        RandomLoot();
    }
    //gives the dead AI random loot based off of many stats
    //player level?
    //AI rank in faction?
    private void RandomLoot()
    {
        par_deadAILoot.SetActive(true);
        par_deadAILoot.GetComponent<Inv_Container>().containerActivated = true;

        foreach (GameObject item in par_deadAILoot.GetComponent<Inv_Container>().inventory)
        {
            //random gun durability between 10% and 50%
            if (item.GetComponent<Item_Gun>() != null)
            {
                float tenp = item.GetComponent<Item_Gun>().maxDurability / 90;
                float fiftyp = item.GetComponent<Item_Gun>().maxDurability / 2;
                item.GetComponent<Item_Gun>().durability = Random.Range(tenp, fiftyp);
            }
            //random ammo count
            if (item.GetComponent<Item_Ammo>() != null)
            {
                item.GetComponent<Env_Item>().int_itemCount = Random.Range(15, 45);
            }
            //random health kit count
            if (item.GetComponent<Item_Consumable>() != null
                && item.GetComponent<Item_Consumable>().consumableType == Item_Consumable.ConsumableType.Healthkit)
            {
                item.GetComponent<Env_Item>().int_itemCount = Random.Range(1, 3);
            }
            //random money count
            if (item.GetComponent<Env_Item>().str_ItemName == "money")
            {
                item.GetComponent<Env_Item>().int_itemCount = Random.Range(15, 200);
            }
        }
    }
}