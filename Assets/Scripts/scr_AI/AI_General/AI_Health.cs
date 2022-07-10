using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float currentHealth;

    //private variables
    private float timer;

    private void Start()
    {
        isAlive = true;

        canBeHostile = true;

        par_deadAILoot.SetActive(false);
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
        RandomizeAllContent(par_deadAILoot, par_deadAILoot.transform);
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

    //used to randomize this containers content
    private void RandomizeAllContent(GameObject inv, Transform spawnables)
    {
        //get total item count
        int totalItemCount = par_Managers.GetComponent<Manager_Console>().spawnables.Count;
        //get random amount of items we want to spawn
        int selectedItemCount = Random.Range(3, 10);
        //create list for selected item indexes
        List<int> selectedItems = new();
        //pick selectedItemCount amount of random item indexes and assign to list
        for (int i = 0; i < selectedItemCount; i++)
        {
            selectedItems.Add(Random.Range(0, totalItemCount));
        }
        //look for duplicate indexes and remove them
        selectedItems = selectedItems.Distinct().ToList();

        //spawn items in container
        foreach (int i in selectedItems)
        {
            //get item by index
            GameObject foundItem = null;
            foreach (GameObject item in par_Managers.GetComponent<Manager_Console>().spawnables)
            {
                if (par_Managers.GetComponent<Manager_Console>().spawnables.IndexOf(item) == i)
                {
                    foundItem = item;
                    break;
                }
            }

            //spawn item if it isnt null
            if (foundItem != null)
            {
                GameObject newDuplicate = Instantiate(foundItem,
                                                      transform.position,
                                                      Quaternion.identity,
                                                      spawnables.transform);

                newDuplicate.name = newDuplicate.GetComponent<Env_Item>().str_ItemName;

                inv.GetComponent<Inv_Container>().inventory.Add(newDuplicate);

                //item count
                if (!newDuplicate.GetComponent<Env_Item>().isStackable
                    || newDuplicate.GetComponent<Item_Consumable>() != null)
                {
                    newDuplicate.GetComponent<Env_Item>().int_itemCount = 1;
                }
                else
                {
                    newDuplicate.GetComponent<Env_Item>().int_itemCount = Random.Range(1, 35);
                }

                //item durability/remainder

                //if this item is a gun
                if (newDuplicate.GetComponent<Item_Gun>() != null)
                {
                    //gun durability
                    float gunMaxDurability = newDuplicate.GetComponent<Item_Gun>().maxDurability;

                    newDuplicate.GetComponent<Item_Gun>().durability =
                        Mathf.Round(Random.Range(gunMaxDurability / 20, gunMaxDurability / 10 * 6) * 10) / 10;
                    newDuplicate.GetComponent<Item_Gun>().LoadValues();
                }
                //if this item is a melee weapon
                else if (newDuplicate.GetComponent<Item_Melee>() != null)
                {
                    //melee weapon durability
                    float meleeWeaponMaxDurability = newDuplicate.GetComponent<Item_Melee>().maxDurability;

                    newDuplicate.GetComponent<Item_Melee>().durability =
                        Mathf.Round(Random.Range(meleeWeaponMaxDurability / 20, meleeWeaponMaxDurability / 10 * 6) * 10) / 10;
                    newDuplicate.GetComponent<Item_Melee>().LoadValues();
                }
                //if this item is a consumable
                else if (newDuplicate.GetComponent<Item_Consumable>() != null)
                {
                    //consumable remainder
                    float consumableMaxRemainder = newDuplicate.GetComponent<Item_Consumable>().maxConsumableAmount;

                    newDuplicate.GetComponent<Item_Consumable>().currentConsumableAmount =
                        Mathf.Round(Random.Range(consumableMaxRemainder / 20, consumableMaxRemainder / 10 * 6) * 10) / 10;
                    newDuplicate.GetComponent<Item_Consumable>().LoadValues();
                }
                //if this item is a battery
                else if (newDuplicate.GetComponent<Item_Battery>() != null)
                {
                    //battery remainder
                    float batteryMaxRemainder = newDuplicate.GetComponent<Item_Battery>().maxBattery;

                    newDuplicate.GetComponent<Item_Battery>().currentBattery =
                        Mathf.Round(Random.Range(batteryMaxRemainder / 20, batteryMaxRemainder / 10 * 6) * 10) / 10;
                    newDuplicate.GetComponent<Item_Battery>().LoadValues();
                }

                newDuplicate.GetComponent<Env_Item>().isInContainer = true;

                newDuplicate.GetComponent<MeshRenderer>().enabled = false;
                if (newDuplicate.GetComponent<Rigidbody>() != null)
                {
                    newDuplicate.GetComponent<Rigidbody>().isKinematic = true;
                }

                newDuplicate.GetComponent<Env_Item>().DeactivateItem();

                //Debug.Log("Spawned item " + newDuplicate.name + " with count " + newDuplicate.GetComponent<Env_Item>().int_itemCount + " in container " + name + ".");
            }
        }
    }
}