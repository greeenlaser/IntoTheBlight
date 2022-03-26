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
    [SerializeField] private Manager_CurrentCell CurrentCellScript;

    [Header("Spawnables")]
    [SerializeField] private GameObject repairKit;
    [SerializeField] private GameObject healthkKit;
    [SerializeField] private GameObject ammo_9mm;

    //public but hidden variables
    [HideInInspector] public bool isActive;

    //private variables
    private bool calledCrateDestroyOnce;

    private void Update()
    {
        if (isActive 
            && !calledCrateDestroyOnce
            && crateHealth <= 0)
        {
            Destroyed();
        }
    }

    public void Destroyed()
    {
        calledCrateDestroyOnce = true;

        if (CurrentCellScript != null)
        {
            CurrentCellScript.destroyableCrates.Remove(gameObject);
        }

        Destroy(completeCrate);
        brokenCrate.transform.position = completeCrate.transform.position;
        brokenCrate.SetActive(true);

        //   repairkit spawn start

        int randomRepairKitDropChance = Random.Range(0, 4);

        if (thePlayer.GetComponent<Inv_Player>().equippedGun.GetComponent<Item_Gun>() != null)
        {
            float gunDurabilityPercentage
            = thePlayer.GetComponent<Inv_Player>().equippedGun.GetComponent<Item_Gun>().durability
            / thePlayer.GetComponent<Inv_Player>().equippedGun.GetComponent<Item_Gun>().maxDurability
            * 100;
            //increases repair kit drop chance if players equipped gun condition is below 25%
            if (thePlayer.GetComponent<Inv_Player>().equippedGun != null
                && thePlayer.GetComponent<Inv_Player>().equippedGun.GetComponent<Item_Gun>().repairKitTypeRequired.ToString()
                == repairKit.GetComponent<Item_Consumable>().repairKitType.ToString()
                && gunDurabilityPercentage < 25)
            {
                randomRepairKitDropChance += 2;
            }
        }
        
        if (randomRepairKitDropChance > 3)
        {
            GameObject spawnedRepairKit = Instantiate(repairKit, transform.position, Quaternion.identity, par_Cratespawns);
            spawnedRepairKit.GetComponent<Env_Item>().itemActivated = true;
            spawnedRepairKit.GetComponent<Env_Item>().droppedObject = true;
            spawnedRepairKit.transform.position = completeCrate.transform.position;
            spawnedRepairKit.SetActive(true);
            Debug.Log("This destroyed crate spawned " + 1 + " " + repairKit.GetComponent<Item_Consumable>().repairKitType +  " repairkit!");
        }

        //   healthkit spawn start

        int randomHealthKitDropChance = Random.Range(0, 4);
        //increases health kit drop chance if players health is below 25
        if (thePlayer.GetComponent<Player_Health>().health < 25)
        {
            randomHealthKitDropChance += 2;
        }
        //Debug.Log("final health kit drop chance: " + randomHealthKitDropChance);
        if (randomHealthKitDropChance > 3)
        {
            GameObject spawnedHealthkit = Instantiate(healthkKit, transform.position, Quaternion.identity, par_Cratespawns);
            spawnedHealthkit.GetComponent<Env_Item>().itemActivated = true;
            spawnedHealthkit.GetComponent<Env_Item>().droppedObject = true;
            spawnedHealthkit.transform.position = completeCrate.transform.position;
            spawnedHealthkit.SetActive(true);
            Debug.Log("This destroyed crate spawned " + 1 + " medkit!");
        }
        Destroy(healthkKit);

        //   ammo spawn start

        int randomAmmoDropChance = Random.Range(0, 5);

        if (thePlayer.GetComponent<Inv_Player>().equippedGun.GetComponent<Item_Gun>() != null)
        {
            //increases ammo drop chance if players equipped gun ammo count is below 25
            if (thePlayer.GetComponent<Inv_Player>().equippedGun != null
            && thePlayer.GetComponent<Inv_Player>().equippedGun.GetComponent<Item_Gun>().ammoType.ToString()
            == ammo_9mm.name)
            {
                foreach (GameObject item in thePlayer.GetComponent<Inv_Player>().inventory)
                {
                    if (item.GetComponent<Item_Ammo>() != null
                        && item.GetComponent<Item_Ammo>().ammoType.ToString()
                        == ammo_9mm.name
                        && item.GetComponent<Env_Item>().int_itemCount < 25)
                    {
                        randomAmmoDropChance += 2;
                        break;
                    }
                }
            }
        }

        //Debug.Log("final ammo drop chance: " + randomAmmoDropChance);
        if (randomAmmoDropChance > 3)
        {
            int randomCount = Random.Range(5, 25);
            GameObject spawnedAmmo = Instantiate(ammo_9mm, transform.position, Quaternion.identity, par_Cratespawns);
            spawnedAmmo.GetComponent<Env_Item>().itemActivated = true;
            spawnedAmmo.GetComponent<Env_Item>().droppedObject = true;
            spawnedAmmo.GetComponent<Env_Item>().int_itemCount = randomCount;
            spawnedAmmo.transform.position = completeCrate.transform.position;
            spawnedAmmo.SetActive(true);
            Debug.Log("This destroyed crate spawned " + randomCount + " 9mm bullets!");
        }
        Destroy(ammo_9mm);

        StartCoroutine(DestroyFragments());
    }

    private IEnumerator DestroyFragments()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}