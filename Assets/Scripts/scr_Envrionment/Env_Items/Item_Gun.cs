using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Gun : MonoBehaviour
{
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
    public AmmoType ammoType;
    public enum AmmoType
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
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private Manager_Console ConsoleScript;
    [SerializeField] private GameManager GameManagerScript;

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
    private float maxRange;
    private float defaultDamage;
    private List<GameObject> gunHitCursors = new List<GameObject>();
    private List<GameObject> spawnedBulletCaseSFX = new List<GameObject>();
    private List<GameObject> spawnedBulletCases = new List<GameObject>();
    private List<GameObject> spawnedGunFireSFX = new List<GameObject>();

    private void Awake()
    {
        //durability is random value between third of max durability and 8/10ths of max durability
        //durability = Mathf.FloorToInt(Random.Range(maxDurability / 3, maxDurability / 10 * 8));

        durability = maxDurability;
    }

    private void Update()
    {
        if (hasEquippedGun
            && !PlayerMovementScript.isStunned)
        {
            //unequips the gun if the player is no longer alive
            if (!thePlayer.GetComponent<Player_Health>().isPlayerAlive)
            {
                UnequipGun();
            }

            //if items condition is over 0, the current clip size is over 0,
            //the player is still alive and the game isn't paused
            if (durability > 0
                && thePlayer.GetComponent<Player_Health>().isPlayerAlive
                && !PauseMenuScript.isGamePaused)
            {
                //when the gun is single shot
                if (Input.GetKeyDown(KeyCode.Mouse0)
                    && isSingleShot 
                    && !isReloading
                    && currentClipSize > 0
                    && !isGunJammed)
                {
                    currentClipSize--;
                    UIReuseScript.txt_ammoInClip.text = currentClipSize.ToString();
                    //gun durability only degrades if it isn't protected
                    if (!gameObject.GetComponent<Env_Item>().isProtected)
                    {
                        ShootBullet();
                        durability -= singleShotDegrade;
                        UIReuseScript.durability = durability;
                        UIReuseScript.maxDurability = maxDurability;
                        UIReuseScript.UpdateWeaponQuality();
                    }
                }
                //when the gun is automatic fire
                else if (Input.GetKey(KeyCode.Mouse0)
                         && !isSingleShot 
                         && !isReloading
                         && currentClipSize > 0
                         && !isGunJammed)
                {
                    if (!isFiring)
                    {
                        isFiring = true;
                    }
                    if (isFiring)
                    {
                        //gun durability and ammo only decrease over time if the gun isn't protected
                        if (!gameObject.GetComponent<Env_Item>().isProtected)
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
                //cant fire this gun while it is jammed
                //else if (Input.GetKeyDown(KeyCode.Mouse0) && isGunJammed)
                //{
                    //Debug.Log("Cannot fire " + gameObject.GetComponent<Env_Item>().str_ItemName + " because it is jammed!");
                //}
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
            }
            else if ((durability == 0
                || !thePlayer.GetComponent<Player_Health>().isPlayerAlive
                || PauseMenuScript.isGamePaused
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

            //plays gun jam sfx
            if (!PauseMenuScript.isGamePaused
                && thePlayer.GetComponent<Player_Health>().isPlayerAlive
                && !sfx_gunJam.isPlaying)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0)
                    && (currentClipSize == 0
                    || isGunJammed))
                {
                    sfx_gunJam.Play();
                }
                else if (Input.GetKeyDown(KeyCode.R)
                         && ammoClip == null)
                {
                    sfx_gunJam.Play();
                }
            }

            if (PauseMenuScript.isGamePaused)
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
        }

        //removes gunHitCursors that were created
        if (gunHitCursors.Count > 0)
        {
            timer_gunHitCursor += Time.deltaTime;
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
            timer_gunBullets += Time.deltaTime;

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
            timer_gunSFX += Time.deltaTime;
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

        //finishes gun reload after gun reload SFX finishes playing
        if (isReloading 
            && !sfx_gunReload.isPlaying
            && !PauseMenuScript.isGamePaused
            && thePlayer.GetComponent<Player_Health>().isPlayerAlive)
        {
            isReloading = false;
        }
    }

    private void AutomaticFireShoot()
    {
        ShootBullet();
        currentClipSize--;
        UIReuseScript.txt_ammoInClip.text = currentClipSize.ToString();
        durability -= automaticFireDegrade;
        UIReuseScript.durability = durability;
        UIReuseScript.maxDurability = maxDurability;
        UIReuseScript.UpdateWeaponQuality();
    }

    private void ShootBullet()
    {
        //gunfire sfx
        GameObject spawnedGunSFX = Instantiate(gunFireSFXTemplate, pos_gunFireSFX.transform.position, Quaternion.identity);
        spawnedGunSFX.SetActive(true);
        spawnedGunSFX.transform.parent = par_gunSpawns.transform;
        spawnedGunFireSFX.Add(spawnedGunSFX);
        //bullet drop sfx
        GameObject spawnedBulletSFX = Instantiate(spawnedBulletCaseSFXTemplate, pos_bulletSpawn.transform.position, Quaternion.identity);
        spawnedBulletSFX.SetActive(true);
        spawnedBulletSFX.transform.parent = par_gunSpawns.transform;
        spawnedBulletCaseSFX.Add(spawnedBulletSFX);
        //spawn a bullet
        GameObject spawnedBullet = Instantiate(spawnedBulletCaseTemplate, pos_bulletSpawn.transform.position, Quaternion.identity);
        spawnedBullet.SetActive(true);
        spawnedBullet.transform.parent = par_gunSpawns.transform;
        spawnedBulletCases.Add(spawnedBullet);

        //did the gun shoot anything
        RaycastHit hit;
        if (Physics.Raycast(pos_shoot.position, pos_shoot.TransformDirection(Vector3.forward), out hit, maxRange))
        {
            GameObject target = hit.transform.gameObject;

            //deal damage to AI
            if (target.GetComponent<AI_Health>() != null
                && target.GetComponent<AI_Health>().isKillable
                && target.GetComponent<AI_Health>().isAlive)
            {
                if (defaultDamage <target.GetComponent<AI_Health>().currentHealth)
                {
                    //deals damage to this human if it can take damage
                    target.GetComponent<AI_Health>().currentHealth -= Mathf.FloorToInt(defaultDamage);

                    //if this AI has not yet detected this target and this target shot this AI
                    //and this AI is not currently chasing or attacking another target
                    if (target.GetComponent<AI_Combat>() != null
                        && target.GetComponent<AI_Combat>().confirmedTarget == null
                        && ConsoleScript.toggleAIDetection)
                    {
                        target.GetComponent<AI_Combat>().finishedHostileSearch = true;
                        target.GetComponent<AI_Combat>().foundPossibleHostiles = true;
                        target.GetComponent<AI_Combat>().hostileTargets.Add(thePlayer);
                    }

                    //Debug.Log("Player shot " + hit.transform.gameObject.GetComponent<UI_AIContent>().str_NPCName + " and dealt " + ammoClip.GetComponent<Item_Ammo>().ammoDefaultDamage + " damage. " +
                    //"Remaining health for " + hit.transform.gameObject.GetComponent<UI_AIContent>().str_NPCName + " is "
                    //+ hit.transform.gameObject.GetComponent<AI_Health>().currentHealth + ".");
                }
                else if (defaultDamage >= target.GetComponent<AI_Health>().currentHealth)
                {
                    target.GetComponent<AI_Health>().Death();
                    Debug.Log("Player killed " + hit.transform.gameObject.GetComponent<UI_AIContent>().str_NPCName + ".");
                }
            }
            //deal damage to target in gunrange
            else if (target.GetComponent<Env_TargetPoints>() != null)
            {
                target.GetComponent<Env_TargetPoints>().HitTarget();
            }
            //deal damage to destroyable crate
            else if (target.GetComponentInParent<Env_DestroyableCrate>() != null)
            {
                target.GetComponentInParent<Env_DestroyableCrate>().crateHealth -= defaultDamage;
                //Debug.Log("Dealt " + defaultDamage + " damage to destroyable crate!");
            }

            //create new gun hit cursors
            Vector3 center = new Vector3(960, 540, 0);
            GameObject newCursor = Instantiate(gunHitCursorTemplate, center, Quaternion.identity);
            newCursor.SetActive(true);
            newCursor.transform.SetParent(par_cursorSpawns.transform);
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
                gunJamChance = 25;

                if (durability < thirtyp)
                {
                    gunJamChance = 40;

                    if (durability < twentyp)
                    {
                        gunJamChance = 65;

                        if (durability < tenp)
                        {
                            gunJamChance = 80;
                        }
                    }
                }
            }
        }

        int chance = Mathf.FloorToInt(Random.Range(1, 4) * gunJamChance);

        if (chance >= 150)
        {
            isGunJammed = true;
        }
    }

    public void EquipGun()
    {
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
    public void UnequipGun()
    {
        if (!isReloading)
        {
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
        isGunJammed = false;

        maxRange = ammoClip.GetComponent<Item_Ammo>().maxMinDamageRange;
        defaultDamage = ammoClip.GetComponent<Item_Ammo>().ammoDefaultDamage;

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
            ConsoleScript.playeritemnames.Remove(ammoClip.GetComponent<Env_Item>().str_ItemName);
            ammoClip.GetComponent<Env_Item>().isInPlayerInventory = false;

            //Debug.Log("Reloaded " + gameObject.GetComponent<Env_Item>().str_ItemName + " and added " + usableAmmo + " ammo. Destroyed the ammo gameobject because there was no more ammo left to use.");

            Destroy(ammoClip);
            ammoClip = null;
        }
    }

    public void AssignAmmoType()
    {
        //finds the correct ammo type
        foreach (GameObject item in PlayerInventoryScript.inventory)
        {
            if (item.GetComponent<Item_Ammo>() != null
                && item.GetComponent<Item_Ammo>().ammoType.ToString() 
                == ammoType.ToString())
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
                    && item.GetComponent<Item_Ammo>().ammoType.ToString() 
                    == ammoType.ToString())
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