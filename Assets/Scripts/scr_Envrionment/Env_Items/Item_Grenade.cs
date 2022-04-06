using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Grenade : MonoBehaviour
{
    [Header("Assignables")]
    public float damage;
    [SerializeField] private float explosionStrength;
    [Range(0f, 50f)]
    [SerializeField] private float explosionRange;

    public GrenadeType grenadeType;
    public enum GrenadeType
    {
        frag,
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
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private UI_PauseMenu PausemenuScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private GameManager GameManagerScript;

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
        if (!PausemenuScript.isGamePaused
            && !ConsoleScript.consoleOpen
            && PlayerHealthScript.health > 0)
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
                        UIReuseScript.bgr_grenadeTimer1.color = new Color32(150, 150, 150, 255);
                    }
                    if (cookingGrenadeTimer >= 2)
                    {
                        UIReuseScript.bgr_grenadeTimer2.color = new Color32(150, 150, 150, 255);
                    }
                    if (cookingGrenadeTimer >= 3)
                    {
                        UIReuseScript.bgr_grenadeTimer3.color = new Color32(150, 150, 150, 255);
                    }
                    if (cookingGrenadeTimer >= 4)
                    {
                        UIReuseScript.bgr_grenadeTimer4.color = new Color32(150, 150, 150, 255);
                    }
                    if (cookingGrenadeTimer >= 5)
                    {
                        UIReuseScript.bgr_grenadeTimer5.color = new Color32(150, 150, 150, 255);

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
                //frag grenade effects
                if (fragExplode)
                {
                    //get all colliders in sphere radius and add effects
                    Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange);
                    foreach (Collider target in colliders)
                    {
                        //get distance between target and grenade
                        float distance = Vector3.Distance(target.transform.position, transform.position);

                        //full grenade damage
                        if (distance <= 2f)
                        {
                            //if player was in explosion range
                            if (target.GetComponent<Player_Health>() != null
                                && target.GetComponent<Player_Health>().health > 0
                                && PlayerHealthScript.canTakeDamage)
                            {
                                target.GetComponent<Player_Health>().health -= damage;
                                //Debug.Log("Grenade distance from player was " + Mathf.Round(distance * 100f) / 100f + " and frag grenade dealt " + damage + " damage.");
                            }
                            //if AI was in explosion range
                            else if (target.GetComponent<AI_Health>() != null
                                     && target.GetComponent<AI_Health>().currentHealth > 0)
                            {
                                target.GetComponent<AI_Health>().currentHealth -= Mathf.FloorToInt(damage);
                                //Debug.Log("Grenade distance from " + target.GetComponent<UI_AIContent>().str_NPCName + " was " + Mathf.Round(distance * 100f) / 100f + " and frag grenade dealt " + damage + " damage.");
                            }
                            //if destroyable crate was in explosion range
                            else if (target.name == "completeCrate"
                                     && target.transform.parent.GetComponent<Env_DestroyableCrate>() != null
                                     && target.transform.parent.GetComponent<Env_DestroyableCrate>().crateHealth > 0)
                            {
                                target.transform.parent.GetComponent<Env_DestroyableCrate>().crateHealth -= damage;
                                //Debug.Log("Grenade distance from destroyable crate was " + Mathf.Round(distance * 100f) / 100f + " and frag grenade dealt " + damage + " damage.");
                            }
                        }
                        //60% grenade damage
                        else if (distance > 2f && distance <= 3.5f)
                        {
                            //if player was in explosion range
                            if (target.GetComponent<Player_Health>() != null
                                && target.GetComponent<Player_Health>().health > 0
                                && PlayerHealthScript.canTakeDamage)
                            {
                                target.GetComponent<Player_Health>().health -= Mathf.FloorToInt(damage / 3 * 2);
                                //Debug.Log("Grenade distance from player was " + Mathf.Round(distance * 100f) / 100f + " and frag grenade dealt " + Mathf.FloorToInt(damage / 3 * 2) + " damage.");
                            }
                            //if AI was in explosion range
                            else if (target.GetComponent<AI_Health>() != null
                                     && target.GetComponent<AI_Health>().currentHealth > 0)
                            {
                                target.GetComponent<AI_Health>().currentHealth -= Mathf.FloorToInt(damage / 3 * 2);
                                //Debug.Log("Grenade distance from " + target.GetComponent<UI_AIContent>().str_NPCName + " was " + Mathf.Round(distance * 100f) / 100f + " and frag grenade dealt " + Mathf.FloorToInt(damage / 3 * 2) + " damage.");
                            }
                            //if destroyable crate was in explosion range
                            else if (target.name == "completeCrate"
                                     && target.transform.parent.GetComponent<Env_DestroyableCrate>() != null
                                     && target.transform.parent.GetComponent<Env_DestroyableCrate>().crateHealth > 0)
                            {
                                target.transform.parent.GetComponent<Env_DestroyableCrate>().crateHealth -= Mathf.FloorToInt(damage / 3 * 2);
                                //Debug.Log("Grenade distance from destroyable crate was " + Mathf.Round(distance * 100f) / 100f + " and frag grenade dealt " + Mathf.FloorToInt(damage / 3 * 2) + " damage.");
                            }
                        }
                        //30% grenade damage
                        else if (distance > 3.5f && distance <= explosionRange)
                        {
                            //if player was in explosion range
                            if (target.GetComponent<Player_Health>() != null
                                && target.GetComponent<Player_Health>().health > 0
                                && PlayerHealthScript.canTakeDamage)
                            {
                                target.GetComponent<Player_Health>().health -= Mathf.FloorToInt(damage / 3);
                                //Debug.Log("Grenade distance from player was " + Mathf.Round(distance * 100f) / 100f + " and frag grenade dealt " + Mathf.FloorToInt(damage / 3) + " damage.");
                            }
                            //if AI was in explosion range
                            else if (target.GetComponent<AI_Health>() != null
                                     && target.GetComponent<AI_Health>().currentHealth > 0)
                            {
                                target.GetComponent<AI_Health>().currentHealth -= Mathf.FloorToInt(damage / 3);
                                //Debug.Log("Grenade distance from " + target.GetComponent<UI_AIContent>().str_NPCName + " was " + Mathf.Round(distance * 100f) / 100f + " and frag renade dealt " + Mathf.FloorToInt(damage / 3) + " damage.");
                            }
                            //if destroyable crate was in explosion range
                            else if (target.name == "completeCrate"
                                     && target.transform.parent.GetComponent<Env_DestroyableCrate>() != null
                                     && target.transform.parent.GetComponent<Env_DestroyableCrate>().crateHealth > 0)
                            {
                                target.transform.parent.GetComponent<Env_DestroyableCrate>().crateHealth -= Mathf.FloorToInt(damage / 3);
                                //Debug.Log("Grenade distance from destroyable crate was " + Mathf.Round(distance * 100f) / 100f + " and frag grenade dealt " + Mathf.FloorToInt(damage / 3) + " damage.");
                            }
                        }
                    }
                }
                //plasma grenade effects
                else if (plasmaExplode)
                {
                    
                }
                //stun grenade effects
                else if (stunExplode)
                {
                    //get all colliders in sphere radius and add effects
                    Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange);
                    foreach (Collider target in colliders)
                    {
                        //player in stungrenade explosion range
                        //and if player doesnt have godmode enabled
                        if (target.GetComponent<Player_Movement>() != null
                            && PlayerHealthScript.canTakeDamage
                            && PlayerRaycastScript.targets.Contains(gameObject))
                        {
                            //stuns the player
                            target.GetComponent<Player_Movement>().Stun();
                        }
                        //killable AI in stungrenade explosion range
                        //and if this AI actually also saw this grenade
                        //and if this AI isn't already stunned
                        else if (target.GetComponent<AI_Health>() != null
                                 && target.GetComponent<AI_Health>().currentHealth > 0
                                 && target.GetComponent<AI_Combat>().collidingObjects.Contains(gameObject)
                                 && !target.GetComponent<AI_Movement>().isStunned)
                        {
                            //stuns the AI
                            target.GetComponent<AI_Movement>().isStunned = true;
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
            UIReuseScript.ClearGrenadeUI();
            UIReuseScript.ClearWeaponUI();

            UIReuseScript.bgr_grenadeTimer1.gameObject.SetActive(true);
            UIReuseScript.bgr_grenadeTimer2.gameObject.SetActive(true);
            UIReuseScript.bgr_grenadeTimer3.gameObject.SetActive(true);
            UIReuseScript.bgr_grenadeTimer4.gameObject.SetActive(true);
            UIReuseScript.bgr_grenadeTimer5.gameObject.SetActive(true);

            UIReuseScript.txt_ammoForGun.text = gameObject.GetComponent<Env_Item>().int_itemCount.ToString();

            //unequips previously equipped melee weapon if there is any
            foreach (GameObject meleeWeapon in PlayerInventoryScript.inventory)
            {
                if (meleeWeapon.GetComponent<Item_Melee>() != null
                    && meleeWeapon.GetComponent<Item_Melee>().hasEquippedMeleeWeapon)
                {
                    meleeWeapon.GetComponent<Item_Melee>().UnequipMeleeWeapon();
                }
            }
            //unequips previously equipped gun if there is any
            foreach (GameObject gun in PlayerInventoryScript.inventory)
            {
                if (gun.GetComponent<Item_Gun>() != null
                    && gun.GetComponent<Item_Gun>().hasEquippedGun)
                {
                    gun.GetComponent<Item_Gun>().UnequipGun();
                }
            }
            //unequips previously equipped grenade if there is any
            foreach (GameObject grenade in PlayerInventoryScript.inventory)
            {
                if (grenade.GetComponent<Item_Grenade>() != null
                    && grenade.GetComponent<Item_Grenade>().hasEquippedGrenade)
                {
                    grenade.GetComponent<Item_Grenade>().UnequipGrenade();
                }
            }

            //enables interpolation on the equipped grenade
            rb.interpolation = RigidbodyInterpolation.None;

            hasEquippedGrenade = true;

            endedGrenadeEquipWaitTimer = false;
            grenadeEquipWaitTimer = 0;

            gameObject.GetComponent<Env_Item>().RemoveListeners();
            UIReuseScript.ClearAllInventories();
            UIReuseScript.ClearInventoryUI();
            UIReuseScript.RebuildPlayerInventory();
            UIReuseScript.txt_InventoryName.text = "Player inventory";
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

            UIReuseScript.ClearGrenadeUI();

            UIReuseScript.txt_ammoForGun.text = "";

            //enables interpolation on the unequipped gun
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            gameObject.GetComponent<Env_Item>().RemoveListeners();
            UIReuseScript.ClearAllInventories();
            UIReuseScript.ClearInventoryUI();
            UIReuseScript.RebuildPlayerInventory();
            UIReuseScript.txt_InventoryName.text = "Player inventory";
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

        GameManagerScript.thrownGrenades.Add(thrownGrenade);

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

            UIReuseScript.txt_ammoForGun.text = gameObject.GetComponent<Env_Item>().int_itemCount.ToString();

            UIReuseScript.ClearGrenadeUI();

            UIReuseScript.bgr_grenadeTimer1.gameObject.SetActive(true);
            UIReuseScript.bgr_grenadeTimer2.gameObject.SetActive(true);
            UIReuseScript.bgr_grenadeTimer3.gameObject.SetActive(true);
            UIReuseScript.bgr_grenadeTimer4.gameObject.SetActive(true);
            UIReuseScript.bgr_grenadeTimer5.gameObject.SetActive(true);

            //Debug.Log("Grenades left: " + gameObject.GetComponent<Env_Item>().int_itemCount + ".");
        }
        //if there is only 1 grenade left
        else if (foundGrenade
                 && gameObject.GetComponent<Env_Item>().int_itemCount == 1)
        {
            UIReuseScript.ClearGrenadeUI();

            UIReuseScript.txt_ammoForGun.text = "";

            //Debug.Log("No more grenades left!");

            PlayerInventoryScript.inventory.Remove(gameObject);

            ConsoleScript.playeritemnames.Remove(gameObject.GetComponent<Env_Item>().str_ItemName);
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

        GameManagerScript.thrownGrenades.Remove(gameObject);

        //get all colliders in sphere radius and add effects
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRange);
        foreach (Collider target in colliders)
        {
            //add explosion effect for each rigidbody in grenades explosion range
            if (target.GetComponent<Rigidbody>() != null)
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
        if (grenadeType == GrenadeType.frag)
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