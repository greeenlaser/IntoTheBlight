using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI_Health : MonoBehaviour
{
    [Header("Assignables")]
    public bool isKillable;
    public bool isRespawnable;
    public bool canBeHostile;
    public float maxHealth;
    [SerializeField] private GameObject par_deadAILoot;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isAlive;
    [HideInInspector] public float currentHealth;

    //private variables
    private float timer;

    private void Start()
    {
        isAlive = true;
        currentHealth = maxHealth;

        canBeHostile = true;

        par_deadAILoot.SetActive(false);

        gameObject.GetComponent<Renderer>().material.color = Color.green;
    }

    private void Update()
    {
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

    //deal damage to this AI
    //NOTE: damageType is currently only a placeholder value,
    //      it will deal different element damage types like player health
    //      in the future
    public void DealDamage(float damageAmount)
    {
        if (currentHealth - damageAmount > 0)
        {
            currentHealth -= damageAmount;

            //if ai health is over 25% then it is colored green
            if (currentHealth > maxHealth / 4)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.green;
            }
            //if ai health is 25% or less then it is colored yellow
            else if (currentHealth <= maxHealth / 4)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }
        else
        {
            Death();
        }
    }

    //currentHealth <= 0
    public void Death()
    {
        isAlive = false;
        currentHealth = 0;

        gameObject.GetComponent<Renderer>().material.color = Color.red;

        gameObject.GetComponent<AI_Movement>().canMove = false;
        gameObject.GetComponent<NavMeshAgent>().isStopped = true;

        RandomLoot();
    }

    //gives the dead AI random loot based off of many stats
    private void RandomLoot()
    {
        par_deadAILoot.SetActive(true);

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
                item.GetComponent<Env_Item>().int_itemCount = Random.Range(3, 25);
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