using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_DestroyableCrate : MonoBehaviour
{
    [Header("Assignables")]
    public float crateHealth;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject completeCrate;
    [SerializeField] private GameObject brokenCrate;
    [SerializeField] private Transform par_Cratespawns;
    [SerializeField] private GameObject par_Managers;

    //private variables
    private Manager_CurrentCell CurrentCellScript;

    private void Start()
    {
        foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
        {
            if (cell.GetComponent<Manager_CurrentCell>().destroyableCrates.Contains(gameObject))
            {
                CurrentCellScript = cell.GetComponent<Manager_CurrentCell>();
                break;
            }
        }
    }

    public void DealDamage(float damageAmount)
    {
        if (crateHealth - damageAmount > 0)
        {
            crateHealth -= damageAmount;
        }
        else
        {
            crateHealth = 0;
            Destroyed();
        }
    }

    private void Destroyed()
    {
        if (CurrentCellScript != null)
        {
            CurrentCellScript.destroyableCrates.Remove(gameObject);
        }

        Destroy(completeCrate);
        brokenCrate.transform.position = completeCrate.transform.position;
        brokenCrate.SetActive(true);

        foreach (GameObject item in par_Managers.GetComponent<Manager_Console>().spawnables)
        {
            //spawns a consumable item
            if (item.GetComponent<Item_Consumable>() != null)
            {
                //repairkit spawn
                if (item.GetComponent<Item_Consumable>().consumableType
                    == Item_Consumable.ConsumableType.Repairkit)
                {
                    int repairKitDropChance = Random.Range(0, 100);

                    if (thePlayer.GetComponent<Inv_Player>().equippedGun != null)
                    {
                        GameObject equippedWeapon = thePlayer.GetComponent<Inv_Player>().equippedGun;
                        float durability;
                        float maxDurability;

                        if (equippedWeapon.GetComponent<Item_Gun>() != null)
                        {
                            durability = equippedWeapon.GetComponent<Item_Gun>().durability;
                            maxDurability = equippedWeapon.GetComponent<Item_Gun>().maxDurability;

                            if (durability < maxDurability / 10 * 3)
                            {
                                repairKitDropChance += 35;
                            }
                            else if (durability < maxDurability / 10 * 1)
                            {
                                repairKitDropChance = 60;
                            }
                        }
                        else if (equippedWeapon.GetComponent<Item_Melee>() != null)
                        {
                            durability = equippedWeapon.GetComponent<Item_Melee>().durability;
                            maxDurability = equippedWeapon.GetComponent<Item_Melee>().maxDurability;

                            if (durability < maxDurability / 10 * 3)
                            {
                                repairKitDropChance += 35;
                            }
                            else if (durability < maxDurability / 10 * 1)
                            {
                                repairKitDropChance += 60;
                            }
                        }
                    }

                    if (repairKitDropChance > 89)
                    {
                        GameObject spawnedRepairKit = Instantiate(item,
                                                                  completeCrate.transform.position,
                                                                  Quaternion.identity,
                                                                  par_Cratespawns);

                        spawnedRepairKit.name = spawnedRepairKit.GetComponent<Env_Item>().str_ItemName;
                        spawnedRepairKit.GetComponent<Env_Item>().droppedObject = true;
                        spawnedRepairKit.SetActive(true);

                        float consumableMaxRemainder = spawnedRepairKit.GetComponent<Item_Consumable>().maxConsumableAmount;

                        spawnedRepairKit.GetComponent<Item_Consumable>().currentConsumableAmount =
                            Mathf.Round(Random.Range(consumableMaxRemainder / 20, consumableMaxRemainder / 10 * 6) * 10) / 10;
                        spawnedRepairKit.GetComponent<Item_Consumable>().LoadValues();

                        //Debug.Log("This destroyed crate spawned a " + spawnedRepairKit.name + "!");
                    }
                }
                //healthkit spawn
                else if (item.GetComponent<Item_Consumable>().consumableType
                         == Item_Consumable.ConsumableType.Healthkit)
                {
                    //healthkit spawn
                    int healthKitDropChance = Random.Range(0, 100);

                    float playerHealth = thePlayer.GetComponent<Player_Health>().health;
                    float playerMaxHealth = thePlayer.GetComponent<Player_Health>().maxHealth;
                    if (playerHealth < playerMaxHealth / 10 * 3)
                    {
                        healthKitDropChance += 35;
                    }
                    else if (playerHealth < playerMaxHealth / 10 * 1)
                    {
                        healthKitDropChance += 60;
                    }

                    if (healthKitDropChance > 89)
                    {
                        GameObject spawnedHealthKit = Instantiate(item,
                                                      completeCrate.transform.position,
                                                      Quaternion.identity,
                                                      par_Cratespawns);

                        spawnedHealthKit.name = spawnedHealthKit.GetComponent<Env_Item>().str_ItemName;
                        spawnedHealthKit.GetComponent<Env_Item>().droppedObject = true;
                        spawnedHealthKit.SetActive(true);

                        float consumableMaxRemainder = spawnedHealthKit.GetComponent<Item_Consumable>().maxConsumableAmount;

                        spawnedHealthKit.GetComponent<Item_Consumable>().currentConsumableAmount =
                            Mathf.Round(Random.Range(consumableMaxRemainder / 20, consumableMaxRemainder / 10 * 6) * 10) / 10;
                        spawnedHealthKit.GetComponent<Item_Consumable>().LoadValues();

                        //Debug.Log("This destroyed crate spawned a " + spawnedHealthKit.name + "!");
                    }
                }
            }
            //spawns ammo
            else if (item.GetComponent<Item_Ammo>() != null)
            {
                int ammoDropChance = Random.Range(0, 100);
                string playerEquippedGunAmmoName = "";

                if (thePlayer.GetComponent<Inv_Player>().equippedGun != null
                    && thePlayer.GetComponent<Inv_Player>().equippedGun.GetComponent<Item_Gun>() != null
                    && thePlayer.GetComponent<Inv_Player>().equippedGun.GetComponent<Item_Gun>().ammoClip != null)
                {
                    playerEquippedGunAmmoName = thePlayer.GetComponent<Inv_Player>().equippedGun.GetComponent<Item_Gun>().ammoClip.ToString();
                }

                if (playerEquippedGunAmmoName != ""
                    && item.name == playerEquippedGunAmmoName)
                {
                    foreach (GameObject ammo in thePlayer.GetComponent<Inv_Player>().inventory)
                    {
                        if (ammo.GetComponent<Item_Ammo>() != null
                            && ammo.GetComponent<Item_Ammo>().caseType.ToString()
                            == playerEquippedGunAmmoName)
                        {
                            if (ammo.GetComponent<Env_Item>().int_itemCount < 25)
                            {
                                ammoDropChance += 35;
                            }
                            else if (ammo.GetComponent<Env_Item>().int_itemCount < 5)
                            {
                                ammoDropChance += 60;
                            }
                            break;
                        }
                    }
                }

                if (ammoDropChance > 89)
                {
                    GameObject spawnedBullet = Instantiate(item,
                              completeCrate.transform.position,
                              Quaternion.identity,
                              par_Cratespawns);

                    spawnedBullet.name = spawnedBullet.GetComponent<Env_Item>().str_ItemName;
                    spawnedBullet.GetComponent<Env_Item>().droppedObject = true;
                    spawnedBullet.GetComponent<Env_Item>().int_itemCount = Random.Range(1, 30);
                    spawnedBullet.SetActive(true);

                    //Debug.Log("This destroyed crate spawned " + spawnedBullet.GetComponent<Env_Item>().int_itemCount + " " + spawnedBullet.name + " bullet(s)!"); 
                }
            }
        }

        StartCoroutine(DestroyFragments());
    }

    private IEnumerator DestroyFragments()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}