using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Gun : MonoBehaviour
{
    [Tooltip("How much damage does this gun deal at max condition?")]
    public float maxDamage;
    [Tooltip("How far can this gun shoot?")]
    public float maxRange;
    [Tooltip("Does this gun fire one bullet per lmb click or does it fire bullets while lmb is held down?")]
    [SerializeField] private bool isSingleShot;
    [Range(10, 100)]
    [Tooltip("How many bullets can you reload to the gun?")]
    [SerializeField] private int maxClipSize;
    [Tooltip("How many bullets per second can this gun fire if its not single shot?")]
    [SerializeField] private float fireRate;
    [Tooltip("How many units per shot does this gun wear out?")]
    [SerializeField] private int singleShotDegrade;
    [Tooltip("How many units per second does this gun wear out while being used?")]
    [SerializeField] private int automaticFireDegrade;
    [Tooltip("The max condition of this gun. Affects accuracy, damage and range.")]
    public float maxDurability;
    [Range(25, 10000)]
    [Tooltip("How much does it cost to fully repair this gun?")]
    public float maxRepairPrice;

    [Header("Assignables")]
    [Tooltip("What ammo type does this gun need.")]
    public CaseType caseType;
    public enum CaseType
    {
        unassigned_ammo,
        _22LR_ammo,
        _9mm_ammo,
        _45ACP_ammo,
        _7_62x39_ammo,
        _5_56x45_ammo,
        _308_ammo,
        _12ga_ammo,
        _50BMG_ammo,
        godbullet
    }
    public RepairKitTypeRequired repairKitTypeRequired;
    public enum RepairKitTypeRequired
    {
        repairkit_unassigned,
        Melee_Tier1,
        Melee_Tier2,
        Melee_Tier3,
        LightGun_Tier1,
        LightGun_Tier2,
        LightGun_Tier3,
        HeavyGun_Tier1,
        HeavyGun_Tier2,
        HeavyGun_Tier3
    }
    [Tooltip("Whats the correct rotation for this gun when held?")]
    [SerializeField] private Vector3 correctHoldRotation;
    [SerializeField] private Transform pos_shoot;
    [SerializeField] private Transform pos_aimDownSights;
    [SerializeField] private Transform par_gunSpawns;
    [SerializeField] private Transform par_cursorSpawns;
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    [Header("Gun fire cursor")]
    [SerializeField] private GameObject gunHitCursorTemplate;

    [Header("Spawned bullet cases")]
    [SerializeField] private GameObject spawnedBulletCaseSFXTemplate;
    [SerializeField] private GameObject spawnedBulletCaseTemplate;
    [SerializeField] private GameObject pos_bulletSpawn;

    [Header("Gun fire SFX")]
    [SerializeField] private GameObject gunFireSFXTemplate;
    [SerializeField] private GameObject pos_gunFireSFX;

    [Header("Gun other SFX")]
    [SerializeField] private AudioSource sfx_gunJam;
    [SerializeField] private AudioSource sfx_gunReload;

    //public but hidden variables
    [HideInInspector] public bool hasEquippedGun;
    [HideInInspector] public bool isFiring;
    [HideInInspector] public bool isReloading;
    [HideInInspector] public bool isGunJammed;
    [HideInInspector] public float damage;
    [HideInInspector] public float durability;
    [HideInInspector] public int currentClipSize;
    [HideInInspector] public GameObject ammoClip;

    //private variables
    private bool firedFirstShot;
    private bool isAimingDownSights;
    private bool clearedCursorsList;
    private float timer_automaticFire;
    private float timer_gunHitCursor;
    private float timer_gunSFX;
    private float timer_gunBullets;
    private float gunJamChance;
    private List<GameObject> gunHitCursors = new List<GameObject>();
    private List<GameObject> spawnedBulletCaseSFX = new List<GameObject>();
    private List<GameObject> spawnedBulletCases = new List<GameObject>();
    private List<GameObject> spawnedGunFireSFX = new List<GameObject>();
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        durability = maxDurability;

        LoadValues();

        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
    }

    private void Update()
    {
        if (hasEquippedGun
            && !PlayerMovementScript.isStunned
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading
            && thePlayer.GetComponent<Player_Health>().isPlayerAlive
            && !par_Managers.GetComponent<UI_PauseMenu>().isGamePaused)
        {
            if (!isReloading
                && !isGunJammed
                && currentClipSize > 0)
            {
                //when the gun is single shot
                if (Input.GetKeyDown(KeyCode.Mouse0)
                    && isSingleShot)
                {
                    //gun durability and ammo dont decrease if the gun is protected
                    if (gameObject.GetComponent<Env_Item>().isProtected)
                    {
                        ShootBullet();
                    }
                    //regular gun shooting
                    else if (!gameObject.GetComponent<Env_Item>().isProtected
                             && durability > 0)
                    {
                        ShootBullet();

                        currentClipSize--;
                        UIReuseScript.txt_ammoInClip.text = currentClipSize.ToString();
                        durability -= singleShotDegrade;
                        UIReuseScript.durability = durability;
                        UIReuseScript.maxDurability = maxDurability;
                        UIReuseScript.UpdateWeaponQuality();
                    }
                    else
                    {
                        Debug.LogWarning("Error: " + name + " cannot be shot because it is broken!");
                    }
                }
                //when the gun is automatic fire
                else if (Input.GetKey(KeyCode.Mouse0)
                         && !isSingleShot)
                {
                    //regular gun shooting or if gun is protected
                    if (gameObject.GetComponent<Env_Item>().isProtected
                        || (!gameObject.GetComponent<Env_Item>().isProtected
                        && durability > 0))
                    {
                        if (!isFiring)
                        {
                            isFiring = true;
                        }
                        if (isFiring)
                        {
                            if (!firedFirstShot)
                            {
                                timer_automaticFire = 0;
                                firedFirstShot = true;
                            }

                            timer_automaticFire -= fireRate * Time.deltaTime;
                            if (timer_automaticFire <= 0)
                            {
                                AutomaticFireShoot();
                                timer_automaticFire = 1;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Error: " + name + " cannot be shot because it is broken!");
                    }
                }
                //when the player releases lmb while the automatic fire gun is still being fired
                else if (Input.GetKeyUp(KeyCode.Mouse0)
                         && isFiring
                         && !isSingleShot)
                {
                    timer_automaticFire = 0;
                    firedFirstShot = false;
                    isFiring = false;
                }
            }
            else if (isFiring
                     && !isReloading
                     && currentClipSize == 0)
            {
                isFiring = false;
            }

            //aim down sights
            if (Input.GetKeyDown(KeyCode.Mouse1) && !isReloading)
            {
                if (!isAimingDownSights)
                {
                    PlayerCamera.fieldOfView = 45;
                    PlayerMovementScript.canJump = false;
                    PlayerMovementScript.canSprint = false;
                    PlayerMovementScript.isSprinting = false;
                    gameObject.transform.position = pos_aimDownSights.position;
                    PlayerCamera.gameObject.GetComponent<Player_Camera>().isAimingDownSights = true;
                    isAimingDownSights = true;
                }
            }
            //stop aiming down sights
            else if (Input.GetKeyUp(KeyCode.Mouse1) && isAimingDownSights)
            {
                PlayerCamera.fieldOfView = PlayerCamera.gameObject.GetComponent<Player_Camera>().fov;
                PlayerMovementScript.canJump = true;
                PlayerMovementScript.canSprint = true;
                gameObject.transform.position = PlayerInventoryScript.pos_EquippedItem.position;
                PlayerCamera.gameObject.GetComponent<Player_Camera>().isAimingDownSights = false;
                isAimingDownSights = false;
            }
            //when the gun isnt firing, ammo type is assigned and the player presses R to reload
            else if (Input.GetKeyDown(KeyCode.R)
                     && !isFiring
                     && !isReloading
                     && ammoClip != null
                     && currentClipSize < maxClipSize)
            {
                if (isAimingDownSights)
                {
                    PlayerCamera.fieldOfView = PlayerCamera.gameObject.GetComponent<Player_Camera>().fov;
                    PlayerMovementScript.canJump = true;
                    PlayerMovementScript.canSprint = true;
                    gameObject.transform.position = PlayerInventoryScript.pos_EquippedItem.position;
                    PlayerCamera.gameObject.GetComponent<Player_Camera>().isAimingDownSights = false;
                    isAimingDownSights = false;
                }
                Reload();
            }

            //plays gun jam sfx
            if (Input.GetKeyDown(KeyCode.Mouse0)
                && !sfx_gunJam.isPlaying
                && (currentClipSize == 0
                || isGunJammed))
            {
                sfx_gunJam.Play();
            }
            else if (Input.GetKeyDown(KeyCode.R)
                     && !sfx_gunJam.isPlaying
                     && ammoClip == null)
            {
                sfx_gunJam.Play();
            }
        }
        //unequips the gun if the player is no longer alive
        else if (!thePlayer.GetComponent<Player_Health>().isPlayerAlive)
        {
            UnequipGun();
        }
        else if (par_Managers.GetComponent<UI_PauseMenu>().isGamePaused)
        {
            //resets aim if game is paused and player is aiming down sights
            if (isAimingDownSights)
            {
                PlayerCamera.fieldOfView = PlayerCamera.gameObject.GetComponent<Player_Camera>().fov;
                PlayerMovementScript.canJump = true;
                PlayerMovementScript.canSprint = true;
                gameObject.transform.position = PlayerInventoryScript.pos_EquippedItem.position;
                PlayerCamera.gameObject.GetComponent<Player_Camera>().isAimingDownSights = false;
                isAimingDownSights = false;
            }
            //stops the gun from firing
            if (isFiring)
            {
                timer_automaticFire = 0;
                firedFirstShot = false;
                isFiring = false;
            }
        }
        //destroys all gun fire cursors
        else if ((durability == 0
                 || isGunJammed)
                 && !clearedCursorsList)
        {
            foreach (GameObject cursor in gunHitCursors)
            {
                Destroy(cursor);
            }
            gunHitCursors.Clear();
            clearedCursorsList = true;
        }

        //removes gunHitCursors that were created
        if (gunHitCursors.Count > 0)
        {
            timer_gunHitCursor += Time.unscaledDeltaTime;
            if (timer_gunHitCursor > 0.1f)
            {
                foreach (GameObject cursor in gunHitCursors)
                {
                    Destroy(cursor);
                }
                gunHitCursors.Clear();
                timer_gunHitCursor = 0;
            }
        }
        //removes spawned bullets
        if (spawnedBulletCases.Count > 0)
        {
            timer_gunBullets += Time.unscaledDeltaTime;

            if (spawnedBulletCases.Count < 50
                && timer_gunBullets > 2f)
            {
                GameObject bullet = spawnedBulletCases[0];
                spawnedBulletCases.RemoveAt(0);
                Destroy(bullet);
                timer_gunBullets = 0;
            }
            else if (spawnedBulletCases.Count >= 50
                     && timer_gunBullets > 0.5f)
            {
                GameObject bullet = spawnedBulletCases[0];
                spawnedBulletCases.RemoveAt(0);
                Destroy(bullet);
                timer_gunBullets = 0;
            }
        }
        //removes spawned gunfire sfx and their SFX
        if (spawnedBulletCaseSFX.Count > 0)
        {
            timer_gunSFX += Time.unscaledDeltaTime;
            if (timer_gunSFX > 0.5f)
            {
                GameObject gunFireSFX = spawnedGunFireSFX[0];
                spawnedGunFireSFX.RemoveAt(0);
                Destroy(gunFireSFX);

                GameObject bulletDropSFX = spawnedBulletCaseSFX[0];
                spawnedBulletCaseSFX.RemoveAt(0);
                Destroy(bulletDropSFX);
                timer_gunSFX = 0;
            }
        }
    }

    private void AutomaticFireShoot()
    {
        //gun durability and ammo dont decrease if the gun is protected
        if (gameObject.GetComponent<Env_Item>().isProtected)
        {
            ShootBullet();
        }
        //regular gun shooting
        else if (!gameObject.GetComponent<Env_Item>().isProtected
                 && durability > 0)
        {
            ShootBullet();

            currentClipSize--;
            UIReuseScript.txt_ammoInClip.text = currentClipSize.ToString();
            durability -= automaticFireDegrade;
            UIReuseScript.durability = durability;
            UIReuseScript.maxDurability = maxDurability;
            UIReuseScript.UpdateWeaponQuality();
        }
    }

    private void ShootBullet()
    {
        //gunfire sfx
        GameObject spawnedGunSFX = Instantiate(gunFireSFXTemplate, 
                                               pos_gunFireSFX.transform.position, 
                                               Quaternion.identity, 
                                               par_gunSpawns.transform);

        spawnedGunSFX.SetActive(true);
        spawnedGunFireSFX.Add(spawnedGunSFX);
        //bullet drop sfx
        GameObject spawnedBulletSFX = Instantiate(spawnedBulletCaseSFXTemplate, 
                                                  pos_bulletSpawn.transform.position, 
                                                  Quaternion.identity, 
                                                  par_gunSpawns.transform);

        spawnedBulletSFX.SetActive(true);
        spawnedBulletCaseSFX.Add(spawnedBulletSFX);
        //spawn a bullet
        GameObject spawnedBullet = Instantiate(spawnedBulletCaseTemplate, 
                                               pos_bulletSpawn.transform.position, 
                                               Quaternion.identity, 
                                               par_gunSpawns.transform);

        spawnedBullet.SetActive(true);
        spawnedBulletCases.Add(spawnedBullet);

        LoadValues();

        //did the gun shoot anything
        RaycastHit hit;
        if (Physics.Raycast(pos_shoot.position, 
                            pos_shoot.TransformDirection(Vector3.forward), 
                            out hit, 
                            maxRange,
                            LayerMask.NameToLayer("IgnoreRaycast"), 
                            QueryTriggerInteraction.Ignore))
        {
            GameObject target = hit.transform.gameObject;

            //deal damage to AI
            if (target.GetComponent<AI_Health>() != null
                && target.GetComponent<AI_Health>().isKillable
                && target.GetComponent<AI_Health>().isAlive)
            {
                //deals damage to this AI if it can take damage
                target.GetComponent<AI_Health>().DealDamage("health", Mathf.Round(damage * 10) / 10);

                //if this AI has not yet detected this target and this target shot this AI
                //and this AI is not currently chasing or attacking another target
                if (target.GetComponent<AI_Combat>() != null
                    && target.GetComponent<AI_Combat>().confirmedTarget == null
                    && par_Managers.GetComponent<Manager_Console>().toggleAIDetection)
                {
                    target.GetComponent<AI_Combat>().finishedHostileSearch = true;
                    target.GetComponent<AI_Combat>().foundPossibleHostiles = true;
                    target.GetComponent<AI_Combat>().hostileTargets.Add(thePlayer);
                }
            }
            //deal damage to destroyable crate
            else if (target.GetComponentInParent<Env_DestroyableCrate>() != null
                     && target.GetComponentInParent<Env_DestroyableCrate>().crateHealth > 0)
            {
                target.GetComponentInParent<Env_DestroyableCrate>().DealDamage(Mathf.Round(damage * 10) / 10);
                //Debug.Log("Dealt " + int_damage + " damage to destroyable crate!");
            }
            //deal damage to target in gunrange
            else if (target.GetComponent<Env_TargetPoints>() != null)
            {
                target.GetComponent<Env_TargetPoints>().HitTarget();
            }
            //push rigidbody item forward
            if (target.GetComponent<Rigidbody>() != null
                     && !target.GetComponent<Rigidbody>().isKinematic)
            {
                //very light explosion at bullet hit position
                target.GetComponent<Rigidbody>().AddExplosionForce(3000, hit.point, 0.5f);
            }

            //create new gun hit cursors
            Vector3 center = new Vector3(960, 540, 0);
            GameObject newCursor = Instantiate(gunHitCursorTemplate, 
                                               center, 
                                               Quaternion.identity, 
                                               par_cursorSpawns.transform);

            newCursor.SetActive(true);
            gunHitCursors.Add(newCursor);
        }

        //gun has a chance to jam
        if (durability < maxDurability / 2)
        {
            GunJamChance();
        }
    }

    private void GunJamChance()
    {
        float fiftyp = (maxDurability / 2);
        float fortyp = (maxDurability / 5) * 2;
        float thirtyp = (maxDurability / 10) * 3;
        float twentyp = maxDurability / 5;
        float tenp = maxDurability / 10;

        if (durability < fiftyp)
        {
            gunJamChance = 10;

            if (durability < fortyp)
            {
                gunJamChance = 20;

                if (durability < thirtyp)
                {
                    gunJamChance = 35;

                    if (durability < twentyp)
                    {
                        gunJamChance = 50;

                        if (durability < tenp)
                        {
                            gunJamChance = 65;
                        }
                    }
                }
            }
        }

        int chance = Mathf.FloorToInt(UnityEngine.Random.Range(1, 4) * gunJamChance);

        if (chance >= 150)
        {
            isGunJammed = true;
        }
    }

    public void LoadValues()
    {
        //update weapon value and damage based off of current durability
        if (durability < maxDurability / 100 * 75)
        {
            //get weapon max value
            int itemValue = gameObject.GetComponent<Env_Item>().int_maxItemValue;
            //get weapon current durability percentage from max durability
            float durabilityPercentage = (durability / maxDurability) * 100;
            //calculate new weapon value according to weapon durability percentage
            itemValue = Mathf.FloorToInt(itemValue / 100 * durabilityPercentage);
            //assign weapon value
            gameObject.GetComponent<Env_Item>().int_ItemValue = itemValue;

            //calculate new weapon damage according to weapon durability percentage
            int itemDamage = Mathf.FloorToInt(maxDamage / 100 * durabilityPercentage);
            //assign new damage to weapon
            damage = itemDamage;

            //gun damage can never go below 10% max damage
            if (damage < Mathf.FloorToInt(maxDamage / 10))
            {
                damage = Mathf.FloorToInt(maxDamage / 10);
            }
        }
        else
        {
            damage = maxDamage;
        }
    }

    public void EquipGun()
    {
        try
        {
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

            foreach (Collider collider in GetComponents<Collider>())
            {
                if (collider.GetComponent<BoxCollider>() != null
                    && collider.GetComponent<BoxCollider>().enabled)
                {
                    collider.GetComponent<BoxCollider>().enabled = false;
                }
                else if (collider.GetComponent<SphereCollider>() != null
                         && collider.GetComponent<SphereCollider>().enabled)
                {
                    collider.GetComponent<SphereCollider>().enabled = false;
                }
                else if (collider.GetComponent<MeshCollider>() != null
                         && collider.GetComponent<MeshCollider>().enabled)
                {
                    collider.GetComponent<MeshCollider>().enabled = false;
                }
            }

            //disables interpolation on the equipped gun
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.None;

            hasEquippedGun = true;
            UIReuseScript.txt_ammoInClip.text = currentClipSize.ToString();
            if (ammoClip != null)
            {
                UIReuseScript.txt_ammoForGun.text = ammoClip.GetComponent<Env_Item>().int_itemCount.ToString();
            }
            else if (ammoClip == null)
            {
                UIReuseScript.txt_ammoForGun.text = "0";
                AssignAmmoType();
            }

            gameObject.GetComponent<Env_Item>().RemoveListeners();
            UIReuseScript.ClearAllInventories();
            UIReuseScript.ClearInventoryUI();
            UIReuseScript.ClearWeaponUI();
            UIReuseScript.RebuildPlayerInventory();
            UIReuseScript.txt_InventoryName.text = "Player inventory";
            PlayerInventoryScript.UpdatePlayerInventoryStats();
            PlayerInventoryScript.equippedGun = gameObject;

            gameObject.SetActive(true);
            gameObject.GetComponent<MeshRenderer>().enabled = true;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.transform.parent = PlayerInventoryScript.pos_EquippedItem.transform;
            gameObject.transform.position = PlayerInventoryScript.pos_EquippedItem.position;
            gameObject.transform.localRotation = Quaternion.Euler(correctHoldRotation);

            UIReuseScript.durability = durability;
            UIReuseScript.maxDurability = maxDurability;
            UIReuseScript.UpdateWeaponQuality();

            //Debug.Log("Equipped " + gameObject.GetComponent<Env_Item>().str_ItemName + "!");
        }
        catch (Exception e)
        {
            Debug.LogError("Error: Something prevented equipping this gun!" + e + ".");
        }
    }
    public void UnequipGun()
    {
        if (!isReloading)
        {
            foreach (Collider collider in GetComponents<Collider>())
            {
                if (collider.GetComponent<BoxCollider>() != null
                    && !collider.GetComponent<BoxCollider>().enabled)
                {
                    collider.GetComponent<BoxCollider>().enabled = true;
                }
                else if (collider.GetComponent<SphereCollider>() != null
                         && !collider.GetComponent<SphereCollider>().enabled)
                {
                    collider.GetComponent<SphereCollider>().enabled = true;
                }
                else if (collider.GetComponent<MeshCollider>() != null
                         && !collider.GetComponent<MeshCollider>().enabled)
                {
                    collider.GetComponent<MeshCollider>().enabled = true;
                }
            }

            //enables interpolation on the unequipped gun
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            hasEquippedGun = false;
            isFiring = false;
            ammoClip = null;

            //deletes all spawned SFX and bullet cases
            foreach (GameObject bulletcase in spawnedBulletCases)
            {
                Destroy(bulletcase);
            }
            foreach (GameObject bulletShotSFX in spawnedGunFireSFX)
            {
                Destroy(bulletShotSFX);
            }
            foreach (GameObject bulletcaseSFX in spawnedBulletCaseSFX)
            {
                Destroy(bulletcaseSFX);
            }
            spawnedBulletCases.Clear();
            spawnedGunFireSFX.Clear();
            spawnedBulletCaseSFX.Clear();

            gameObject.GetComponent<Env_Item>().RemoveListeners();
            UIReuseScript.ClearAllInventories();
            UIReuseScript.ClearInventoryUI();
            UIReuseScript.RebuildPlayerInventory();
            UIReuseScript.txt_InventoryName.text = "Player inventory";
            PlayerInventoryScript.UpdatePlayerInventoryStats();
            PlayerInventoryScript.equippedGun = null;
            UIReuseScript.ClearWeaponUI();

            gameObject.SetActive(false);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
            transform.parent = PlayerInventoryScript.par_PlayerItems.transform;
            transform.position = PlayerInventoryScript.par_PlayerItems.transform.position;

            //Debug.Log("Unequipped " + gameObject.GetComponent<Env_Item>().str_ItemName + "!");
        }
        else if (isReloading)
        {
            Debug.Log("Error: Can't unequip " + gameObject.GetComponent<Env_Item>().str_ItemName + " while it is reloading!");
        }
        clearedCursorsList = false;
    }

    public void Reload()
    {
        isReloading = true;

        //plays gun reload sfx
        sfx_gunReload.Play();

        int usableAmmo = ammoClip.GetComponent<Env_Item>().int_itemCount;
        int ammoUntilFullReload = maxClipSize - currentClipSize;
        //if there is enough ammo to fully reload the gun
        if (usableAmmo > ammoUntilFullReload)
        {
            ammoClip.GetComponent<Env_Item>().int_itemCount -= ammoUntilFullReload;
            currentClipSize = maxClipSize;

            UIReuseScript.txt_ammoInClip.text = maxClipSize.ToString();
            UIReuseScript.txt_ammoForGun.text = ammoClip.GetComponent<Env_Item>().int_itemCount.ToString();
            //Debug.Log("Fully reloaded " + gameObject.GetComponent<Env_Item>().str_ItemName + "! Remaining reloadable ammo is " + ammoClip.GetComponent<Env_Item>().int_itemCount + ".");
        }
        //if there is not enough ammo to fully reload the gun
        else if (usableAmmo <= ammoUntilFullReload)
        {
            currentClipSize += usableAmmo;
            UIReuseScript.txt_ammoInClip.text = currentClipSize.ToString();
            UIReuseScript.txt_ammoForGun.text = "0";

            PlayerInventoryScript.inventory.Remove(ammoClip);
            par_Managers.GetComponent<Manager_Console>().playeritemnames.Remove(ammoClip.GetComponent<Env_Item>().str_ItemName);
            ammoClip.GetComponent<Env_Item>().isInPlayerInventory = false;

            //Debug.Log("Reloaded " + gameObject.GetComponent<Env_Item>().str_ItemName + " and added " + usableAmmo + " ammo. Destroyed the ammo gameobject because there was no more ammo left to use.");

            Destroy(ammoClip);
            ammoClip = null;
        }

        isGunJammed = false;
        isReloading = false;
    }

    public void AssignAmmoType()
    {
        //finds the correct ammo type
        foreach (GameObject item in PlayerInventoryScript.inventory)
        {
            if (item.GetComponent<Item_Ammo>() != null
                && item.GetComponent<Item_Ammo>().caseType.ToString() 
                == caseType.ToString())
            {
                ammoClip = item;
                break;
            }
        }

        if (ammoClip == null)
        {
            UIReuseScript.txt_ammoForGun.text = "0";

            //finds the correct ammo type
            foreach (GameObject item in PlayerInventoryScript.inventory)
            {
                if (item.GetComponent<Item_Ammo>() != null
                    && item.GetComponent<Item_Ammo>().caseType.ToString() 
                    == caseType.ToString())
                {
                    ammoClip = item;
                    break;
                }
            }
        }
        else if (ammoClip != null)
        {
            int ammoInInventory = ammoClip.GetComponent<Env_Item>().int_itemCount;
            UIReuseScript.txt_ammoForGun.text = ammoInInventory.ToString();
        }
    }
}