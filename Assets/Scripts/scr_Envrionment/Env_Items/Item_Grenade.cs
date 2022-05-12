using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Grenade : MonoBehaviour
{
    [Header("Assignables")]
    [Range(0, 100)]
    public float maxDamage;
    [Range(0, 500)]
    [SerializeField] private float explosionStrength;
    [Range(0f, 50f)]
    [SerializeField] private float explosionRange;

    public GrenadeType grenadeType;
    public enum GrenadeType
    {
        fragmentation,
        plasma,
        stun
    }
    public ExplosionType explosionType;
    public enum ExplosionType
    {
        timed,
        onCollision
    }
    public StickyType stickyType;
    public enum StickyType
    {
        nonSticky,
        sticky
    }

    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Player_RaycastSystem PlayerRaycastScript;
    [SerializeField] private GameObject particleEffect;
    [SerializeField] private Transform pos_HoldItem;
    [SerializeField] private Transform pos_GrenadeInstantiate;
    [SerializeField] private Transform par_ThrownGrenades;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool hasEquippedGrenade;
    [HideInInspector] public bool startedCookingGrenadeTimer;
    [HideInInspector] public bool isThrownGrenade;
    [HideInInspector] public float cookingGrenadeTimer;

    //private variables
    private bool threwUpperHandGrenade;
    private bool threwLowerHandGrenade;
    private GameObject thrownGrenade;
    private Transform targetParent;
    private Transform pos_sticky;

    //grenade explosion checks
    private bool endedExplosionEffects;
    private bool fragExplode;
    private bool plasmaExplode;
    private bool stunExplode;

    //timers

    //primary timer when grenade is first equipped
    //to prevent grenade throw when equip button is pressed
    private bool endedGrenadeEquipWaitTimer;
    private float grenadeEquipWaitTimer;

    //secondary timer which prevents grenades being thrown with no delay
    private bool startedGrenadeThrowWaitTimer;
    private float grenadeThrowWaitTimer;

    //timer which prevents new grenade throw
    //if last grenade exploded in players hand
    //without player throwing that grenade
    private bool startedGrenadeNonThrownTimer;
    private float grenadeNonThrownTimer;

    //final timer which is the exploded grenade particle effect lifetime
    private bool startedExplodedGrenadeDestroyTimer;
    private float explodedGrenadeDestroyTimer;

    private void Awake()
    {
        //if this grenade was incorrectly assigned as sticky and explode on collision
        if (stickyType == StickyType.sticky
            && explosionType == ExplosionType.onCollision)
        {
            Debug.LogError("Error: " + gameObject.GetComponent<Env_Item>().str_ItemName + " " +
                           "cannot be sticky and explode on collision! Changed it to non-sticky.");

            stickyType = StickyType.nonSticky;
        }
    }

    private void Update()
    {
        if (!par_Managers.GetComponent<UI_PauseMenu>().isGamePaused
            && !par_Managers.GetComponent<Manager_Console>().consoleOpen
            && PlayerHealthScript.isPlayerAlive
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            if (hasEquippedGrenade
                && !isThrownGrenade
                && !PlayerMovementScript.isStunned)
            {
                if (!endedGrenadeEquipWaitTimer)
                {
                    grenadeEquipWaitTimer += Time.deltaTime;

                    if (grenadeEquipWaitTimer > 0.1f)
                    {
                        endedGrenadeEquipWaitTimer = true;
                    }
                }
                else if (endedGrenadeEquipWaitTimer 
                         && !startedGrenadeThrowWaitTimer
                         && !startedGrenadeNonThrownTimer)
                {
                    //upper hand throw + grenade cook
                    if (Input.GetKey(KeyCode.Mouse0)
                        && !Input.GetKey(KeyCode.Mouse1))
                    {
                        //Debug.Log("Cooking upper hand grenade!");

                        startedCookingGrenadeTimer = true;
                        startedGrenadeThrowWaitTimer = true;
                    }
                    //lower hand throw + grenade cook
                    else if (Input.GetKey(KeyCode.Mouse1)
                             && !Input.GetKey(KeyCode.Mouse0))
                    {
                        //Debug.Log("Cooking lower hand grenade!");

                        startedCookingGrenadeTimer = true;
                        startedGrenadeThrowWaitTimer = true;
                    }
                }

                //grenade cook timer
                if (startedCookingGrenadeTimer
                    && !startedGrenadeNonThrownTimer)
                {
                    cookingGrenadeTimer += Time.deltaTime;

                    if (cookingGrenadeTimer >= 1)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer1.color = new Color32(150, 150, 150, 255);
                    }
                    if (cookingGrenadeTimer >= 2)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer2.color = new Color32(150, 150, 150, 255);
                    }
                    if (cookingGrenadeTimer >= 3)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer3.color = new Color32(150, 150, 150, 255);
                    }
                    if (cookingGrenadeTimer >= 4)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer4.color = new Color32(150, 150, 150, 255);
                    }
                    if (cookingGrenadeTimer >= 5)
                    {
                        par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer5.color = new Color32(150, 150, 150, 255);

                        //if time runs out and player doesnt throw grenade
                        if (!threwUpperHandGrenade
                            && !threwLowerHandGrenade)
                        {
                            ThrowGrenade();
                            startedGrenadeNonThrownTimer = true;
                        }

                        startedCookingGrenadeTimer = false;
                    }

                    if (!threwUpperHandGrenade
                        && !threwLowerHandGrenade
                        && !startedGrenadeNonThrownTimer)
                    {
                        //if player does throw grenade
                        if (Input.GetKeyUp(KeyCode.Mouse0) && cookingGrenadeTimer < 5)
                        {
                            //Debug.Log("Threw upper hand grenade!");

                            threwUpperHandGrenade = true;
                            ThrowGrenade();
                        }
                        else if (Input.GetKeyUp(KeyCode.Mouse1) && cookingGrenadeTimer < 5)
                        {
                            //Debug.Log("Threw lower hand grenade!");

                            threwLowerHandGrenade = true;
                            ThrowGrenade();
                        }
                    }
                }

                //primary wait timer to prevent grenade being thrown when player equips grenade
                if (!endedGrenadeEquipWaitTimer)
                {
                    grenadeEquipWaitTimer += Time.deltaTime;

                    if (grenadeEquipWaitTimer > 1f)
                    {
                        endedGrenadeEquipWaitTimer = true;
                    }
                }

                //secondary wait timer to prevent grenades being thrown instantly
                //after previous one was thrown
                if (startedGrenadeThrowWaitTimer)
                {
                    grenadeThrowWaitTimer += Time.deltaTime;

                    if (grenadeThrowWaitTimer > 1f)
                    {
                        startedGrenadeThrowWaitTimer = false;
                    }
                }

                //timer to prevent grenade re-use after last non-thrown one exploded in players hand
                if (startedGrenadeNonThrownTimer)
                {
                    grenadeNonThrownTimer += Time.deltaTime;

                    if (grenadeNonThrownTimer > 1f)
                    {
                        startedGrenadeNonThrownTimer = false;
                    }
                }
            }

            //continues explosion countdown for thrown grenade
            if (isThrownGrenade
                && explosionType
                == ExplosionType.timed)
            {
                cookingGrenadeTimer += Time.deltaTime;

                if (cookingGrenadeTimer > 5f)
                {
                    ExplodeGrenade();
                }
            }

            //updates sticky grenade positon in case the parent moves
            if (targetParent != null
                && stickyType == StickyType.sticky)
            {
                transform.position = pos_sticky.position;
                //Debug.Log(pos_sticky.position);
            }

            //starts particle effect timer which destroys
            //both particle effect and thrown grenade
            if (startedExplodedGrenadeDestroyTimer)
            {
                explodedGrenadeDestroyTimer += Time.deltaTime;

                if (explodedGrenadeDestroyTimer > 0.25f)
                {
                    if (particleEffect != null)
                    {
                        Destroy(particleEffect);
                    }
                    Destroy(gameObject);
                }
            }

            //explosion effects of the exploded grenades
            if (isThrownGrenade 
                && startedExplodedGrenadeDestroyTimer
                && !endedExplosionEffects)
            {
                //get all colliders in sphere radius and add effects
                Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange);
                if (colliders.Length > 0)
                {
                    foreach (Collider target in colliders)
                    {
                        //grenade only affects player, AI and destroyable crates
                        if ((target.GetComponent<Player_Health>() != null
                            && PlayerHealthScript.canTakeDamage
                            && PlayerHealthScript.isPlayerAlive)
                            || (target.GetComponent<AI_Health>() != null
                            && target.GetComponent<AI_Health>().isKillable
                            && target.GetComponent<AI_Health>().isAlive)
                            || (target.name == "completeCrate"
                            && target.transform.parent.GetComponent<Env_DestroyableCrate>() != null
                            && target.transform.parent.GetComponent<Env_DestroyableCrate>().crateHealth > 0))
                        {
                            //get the direction of the target
                            Vector3 destination = target.transform.position - gameObject.transform.position;

                            //shoot out a ray towards the target and look if the target isnt behind walls
                            if (Physics.Raycast(transform.position,
                                                destination,
                                                out RaycastHit hit,
                                                explosionRange))
                            {
                                //shoot out a debug ray to confirm the direction of the raycast
                                Debug.DrawRay(transform.position,
                                              destination,
                                              Color.green,
                                              2);

                                //frag and plasma grenade effects
                                if ((fragExplode
                                    || plasmaExplode)
                                    && hit.transform.name == target.name)
                                {
                                    //get distance between target and grenade
                                    float distance = Vector3.Distance(target.transform.position, transform.position);

                                    //deal damage to the player
                                    if (target.GetComponent<Player_Health>() != null)
                                    {
                                        //grenade always deals full damage if the target is less than 0.5m from it
                                        if (distance <= 0.5f)
                                        {
                                            target.GetComponent<Player_Health>().DealDamage(name, grenadeType.ToString(), maxDamage);
                                        }
                                        else
                                        {
                                            //get true grenade damage by distance
                                            float damageByDistance = Mathf.Floor(maxDamage / distance * 10) / 10;

                                            //grenade damage can never go below 10% max damage
                                            if (damageByDistance < Mathf.Floor(maxDamage / 10))
                                            {
                                                damageByDistance = Mathf.Floor(maxDamage / 10);
                                            }

                                            target.GetComponent<Player_Health>().DealDamage(name, grenadeType.ToString(), damageByDistance);
                                        }
                                    }
                                    //deal damage to the alive AI
                                    else if (target.GetComponent<AI_Health>() != null)
                                    {
                                        //grenade always deals full damage if the target is less than 0.5m from it
                                        if (distance <= 0.5f)
                                        {
                                            target.GetComponent<AI_Health>().DealDamage(maxDamage);
                                        }
                                        else
                                        {
                                            //get true grenade damage by distance
                                            float damageByDistance = Mathf.Floor(maxDamage / distance * 10) / 10;

                                            //grenade damage can never go below 10% max damage
                                            if (damageByDistance < Mathf.Floor(maxDamage / 10))
                                            {
                                                damageByDistance = Mathf.Floor(maxDamage / 10);
                                            }

                                            target.GetComponent<AI_Health>().DealDamage(damageByDistance);
                                        }
                                    }
                                    //deal damage to the destroyable crate
                                    else if (target.name == "completeCrate")
                                    {
                                        //grenade always deals full damage if the target is less than 0.5m from it
                                        if (distance <= 0.5f)
                                        {
                                            target.transform.parent.GetComponent<Env_DestroyableCrate>().DealDamage(maxDamage);
                                        }
                                        else
                                        {
                                            //get true grenade damage by distance
                                            float damageByDistance = Mathf.Floor(maxDamage / distance * 10) / 10;

                                            //grenade damage can never go below 10% max damage
                                            if (damageByDistance < Mathf.Floor(maxDamage / 10))
                                            {
                                                damageByDistance = Mathf.Floor(maxDamage / 10);
                                            }

                                            target.transform.parent.GetComponent<Env_DestroyableCrate>().DealDamage(damageByDistance);
                                        }
                                    }
                                }
                                //stun grenade effects
                                else if (stunExplode
                                         && hit.transform.name == target.name)
                                {
                                    //stun the non-stunned player
                                    if (target.GetComponent<Player_Health>() != null
                                        && !PlayerMovementScript.isStunned)
                                    {
                                        //player needs to see the stun grenade to be stunned by it
                                        foreach (GameObject item in PlayerRaycastScript.targets)
                                        {
                                            if (item.name == name)
                                            {
                                                //stuns the player
                                                PlayerMovementScript.Stun();

                                                break;
                                            }
                                        }
                                    }
                                    //stun the non-stunned AI
                                    else if (target.GetComponent<AI_Health>() != null
                                             && !target.GetComponent<AI_Movement>().isStunned)
                                    {
                                        //AI needs to see the stun grenade to be stunned by it
                                        foreach (GameObject item in target.GetComponent<AI_Combat>().collidingObjects)
                                        {
                                            if (item.name == name)
                                            {
                                                //stuns the AI
                                                target.GetComponent<AI_Movement>().isStunned = true;

                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                gameObject.GetComponent<MeshRenderer>().enabled = false;

                if (particleEffect != null)
                {
                    particleEffect.SetActive(true);
                }

                //allows all effects to only run once for all affected targets with this thrown grenade
                endedExplosionEffects = true;
            }
        }

        //resets grenade equip wait timer
        if (endedGrenadeEquipWaitTimer && grenadeEquipWaitTimer > 0)
        {
            grenadeEquipWaitTimer = 0;
        }
        //resets grenade throw wait timer
        if (!startedGrenadeThrowWaitTimer && grenadeThrowWaitTimer > 0)
        {
            grenadeThrowWaitTimer = 0;
        }
        //resets grenade non-thrown timer
        if (!startedGrenadeNonThrownTimer && grenadeNonThrownTimer > 0)
        {
            startedCookingGrenadeTimer = false;
            grenadeNonThrownTimer = 0;
        }
        //resets grenade cook timer
        if (!startedCookingGrenadeTimer && cookingGrenadeTimer > 0)
        {
            cookingGrenadeTimer = 0;
        }

        //if player happens to die
        if (PlayerHealthScript.health <= 0)
        {
            //moves all thrown grenades out of the world
            foreach (Transform child in par_ThrownGrenades)
            {
                child.transform.position = new Vector3(0, -1000, 0);
            }

            //unequips equipped grenade if player dies
            if (hasEquippedGrenade)
            {
                startedCookingGrenadeTimer = false;
                UnequipGrenade();
            }
        }
    }

    public void EquipGrenade()
    {
        try
        {
            bool isCookingGrenade = false;
            foreach (GameObject grenade in PlayerInventoryScript.inventory)
            {
                if (grenade.GetComponent<Item_Grenade>() != null
                    && grenade.GetComponent<Item_Grenade>().startedCookingGrenadeTimer)
                {
                    isCookingGrenade = true;
                }
            }

            if (isCookingGrenade)
            {
                Debug.LogWarning("Error: Cannot equip a different grenade because currently equipped grenade is still cooking!");
            }
            else if (!isCookingGrenade)
            {
                par_Managers.GetComponent<Manager_UIReuse>().ClearGrenadeUI();
                par_Managers.GetComponent<Manager_UIReuse>().ClearWeaponUI();

                par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer1.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer2.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer3.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer4.gameObject.SetActive(true);
                par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer5.gameObject.SetActive(true);

                par_Managers.GetComponent<Manager_UIReuse>().txt_ammoForGun.text = gameObject.GetComponent<Env_Item>().int_itemCount.ToString();

                foreach (GameObject item in PlayerInventoryScript.inventory)
                {
                    if (item.GetComponent<Item_Gun>() != null
                        && item.GetComponent<Item_Gun>().hasEquippedGun)
                    {
                        item.GetComponent<Item_Gun>().UnequipGun();
                    }
                    else if (item.GetComponent<Item_Melee>() != null
                             && item.GetComponent<Item_Melee>().hasEquippedMeleeWeapon)
                    {
                        item.GetComponent<Item_Melee>().UnequipMeleeWeapon();
                    }
                    else if (item.GetComponent<Item_Grenade>() != null
                             && item.GetComponent<Item_Grenade>().hasEquippedGrenade)
                    {
                        item.GetComponent<Item_Grenade>().UnequipGrenade();
                    }
                }
                
                //enables interpolation on the equipped grenade
                rb.interpolation = RigidbodyInterpolation.None;

                hasEquippedGrenade = true;

                endedGrenadeEquipWaitTimer = false;
                grenadeEquipWaitTimer = 0;

                gameObject.GetComponent<Env_Item>().RemoveListeners();
                par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
                par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
                par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
                par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
                PlayerInventoryScript.UpdatePlayerInventoryStats();
                PlayerInventoryScript.equippedGun = gameObject;

                gameObject.SetActive(true);
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                gameObject.transform.parent = PlayerInventoryScript.pos_EquippedItem.transform;
                gameObject.transform.position = PlayerInventoryScript.pos_EquippedItem.position;

                //Debug.Log("Equipped " + gameObject.GetComponent<Env_Item>().str_ItemName + "!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error: Something prevented equipping this grenade!" + e + ".");
        }
    }
    public void UnequipGrenade()
    {
        if (!startedCookingGrenadeTimer)
        {
            hasEquippedGrenade = false;

            endedGrenadeEquipWaitTimer = false;
            grenadeEquipWaitTimer = 0;

            startedGrenadeThrowWaitTimer = false;
            grenadeThrowWaitTimer = 0;

            startedCookingGrenadeTimer = false;
            cookingGrenadeTimer = 0;

            par_Managers.GetComponent<Manager_UIReuse>().ClearGrenadeUI();

            par_Managers.GetComponent<Manager_UIReuse>().txt_ammoForGun.text = "";

            //enables interpolation on the unequipped gun
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            gameObject.GetComponent<Env_Item>().RemoveListeners();
            par_Managers.GetComponent<Manager_UIReuse>().ClearAllInventories();
            par_Managers.GetComponent<Manager_UIReuse>().ClearInventoryUI();
            par_Managers.GetComponent<Manager_UIReuse>().RebuildPlayerInventory();
            par_Managers.GetComponent<Manager_UIReuse>().txt_InventoryName.text = "Player inventory";
            PlayerInventoryScript.UpdatePlayerInventoryStats();
            PlayerInventoryScript.equippedGun = null;

            gameObject.SetActive(false);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            transform.parent = PlayerInventoryScript.par_PlayerItems.transform;
            transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;

            //Debug.Log("Unequipped " + gameObject.GetComponent<Env_Item>().str_ItemName + "!");
        }
        else if (startedCookingGrenadeTimer)
        {
            Debug.LogWarning("Error: Cannot unequip this grenade because it is cooking!");
        }
    }

    private void ThrowGrenade()
    {
        thrownGrenade = Instantiate(gameObject, 
                                    pos_GrenadeInstantiate.position, 
                                    Quaternion.identity, 
                                    par_ThrownGrenades);

        thrownGrenade.GetComponent<Env_Item>().int_itemCount = 1;

        par_Managers.GetComponent<GameManager>().thrownGrenades.Add(thrownGrenade);

        PlayerInventoryScript.inventory.Remove(thrownGrenade);
        PlayerInventoryScript.invSpace += gameObject.GetComponent<Env_Item>().int_ItemWeight;

        //enables interpolation on the thrown grenade
        thrownGrenade.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;

        thrownGrenade.GetComponent<Item_Grenade>().cookingGrenadeTimer = cookingGrenadeTimer;
        thrownGrenade.GetComponent<Item_Grenade>().isThrownGrenade = true;

        thrownGrenade.GetComponent<Rigidbody>().isKinematic = false;

        startedCookingGrenadeTimer = false;
        cookingGrenadeTimer = 0;

        if (threwUpperHandGrenade)
        {
            thrownGrenade.GetComponent<Env_ObjectPickup>().speedLimit = 10f;
            thrownGrenade.GetComponent<Rigidbody>().velocity = pos_HoldItem.forward * 100;
        }
        else if (threwLowerHandGrenade)
        {
            thrownGrenade.GetComponent<Rigidbody>().velocity = pos_HoldItem.forward * 100 / 25;
        }

        thrownGrenade = null;

        threwLowerHandGrenade = false;
        threwUpperHandGrenade = false;

        //removes thrown grenade from player inventory
        bool foundGrenade = false;
        foreach (GameObject grenade in PlayerInventoryScript.inventory)
        {
            if (grenade == gameObject)
            {
                foundGrenade = true;
            }
        }

        //if there are more than 1 grenades left
        if (foundGrenade
            && gameObject.GetComponent<Env_Item>().int_itemCount > 1)
        {
            gameObject.GetComponent<Env_Item>().int_itemCount--;

            par_Managers.GetComponent<Manager_UIReuse>().txt_ammoForGun.text = gameObject.GetComponent<Env_Item>().int_itemCount.ToString();

            par_Managers.GetComponent<Manager_UIReuse>().ClearGrenadeUI();

            par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer1.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer2.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer3.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer4.gameObject.SetActive(true);
            par_Managers.GetComponent<Manager_UIReuse>().bgr_grenadeTimer5.gameObject.SetActive(true);

            //Debug.Log("Grenades left: " + gameObject.GetComponent<Env_Item>().int_itemCount + ".");
        }
        //if there is only 1 grenade left
        else if (foundGrenade
                 && gameObject.GetComponent<Env_Item>().int_itemCount == 1)
        {
            par_Managers.GetComponent<Manager_UIReuse>().ClearGrenadeUI();

            par_Managers.GetComponent<Manager_UIReuse>().txt_ammoForGun.text = "";

            //Debug.Log("No more grenades left!");

            PlayerInventoryScript.inventory.Remove(gameObject);

            par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(gameObject.GetComponent<Env_Item>().str_ItemName);
            Destroy(gameObject);
        }
    }

    //if this thrown grenade collides with anything
    private void OnCollisionEnter(Collision collision)
    {
        if (isThrownGrenade)
        {
            //explodes on impact
            if (explosionType == ExplosionType.onCollision)
            {
                ExplodeGrenade();
            }
            //sticks to first collider
            else if (explosionType == ExplosionType.timed
                     && stickyType == StickyType.sticky)
            {
                //assign contact
                ContactPoint contact = collision.contacts[0];
                //assign sticking position
                Vector3 stickyPosition = contact.point;
                //move gameobject to sticking position
                transform.position = stickyPosition;
                //assign target parent for updating sticked grenade position
                targetParent = contact.otherCollider.transform;
                //Debug.Log("Target parent is " + targetParent.name);
                //create new empty gameobject to use as guide where to move this grenade if the parent moves
                GameObject stickyPos = Instantiate(new GameObject("pos_stickyGrenade"), 
                                                   stickyPosition, 
                                                   Quaternion.identity, 
                                                   targetParent);

                pos_sticky = stickyPos.transform;
                //stop this grenade from moving any further
                rb.velocity = new Vector3(0, 0, 0);
            }
        }
    }

    public void ExplodeGrenade()
    {
        //Debug.Log("Exploded " + stickyType + " " +  grenadeType.ToString() + " grenade!");

        par_Managers.GetComponent<GameManager>().thrownGrenades.Remove(gameObject);

        //get all colliders in sphere radius and add effects
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange);
        foreach (Collider target in colliders)
        {
            //add explosion effect for each rigidbody in grenades explosion range
            if (target.GetComponent<Rigidbody>() != null
                && !target.GetComponent<Rigidbody>().isKinematic)
            {
                Rigidbody targetRB = target.GetComponent<Rigidbody>();

                if (explosionType == ExplosionType.onCollision)
                {
                    targetRB.AddExplosionForce(explosionStrength * 15, transform.position, explosionRange);
                }
                else if (explosionType == ExplosionType.timed)
                {
                    targetRB.AddExplosionForce(explosionStrength, transform.position, explosionRange);
                }
            }
        }

        //start frag grenade effects
        if (grenadeType == GrenadeType.fragmentation)
        {
            fragExplode = true;

            particleEffect.transform.parent = par_ThrownGrenades;
            particleEffect.transform.localScale = new Vector3(explosionRange, explosionRange, explosionRange);
        }
        //start plasma grenade effects
        else if (grenadeType == GrenadeType.plasma)
        {
            plasmaExplode = true;
        }
        //start stun grenade effects
        else if (grenadeType == GrenadeType.stun)
        {
            stunExplode = true;
        }

        startedExplodedGrenadeDestroyTimer = true;
    }
}