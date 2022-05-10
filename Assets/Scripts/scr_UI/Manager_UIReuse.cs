using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Manager_UIReuse : MonoBehaviour
{
    [Header("Basic reusables")]
    public RawImage img_Interact;
    public TMP_Text txt_HoverItemCount;
    public RawImage bgr_HoverItemCountBackground;
    public GameObject cursor;
    public Button btn_CloseUI;
    public RawImage bgr_PlayerStun;

    [Header("Player menu")]
    public GameObject par_PlayerMenu;
    public Button btn_Inventory;
    public Button btn_Stats;
    public Button btn_Upgrades;
    public Button btn_Quests;
    public Button btn_Factions;
    public Button btn_Radio;
    public Button btn_Map;
    //player menu parents
    public GameObject par_Inventory;
    public GameObject par_PlayerMenuStats;
    public GameObject par_PlayerUpgrades;
    public GameObject par_PlayerFactionUI;
    public GameObject par_PlayerMenuRadio;
    public GameObject par_PlayerMenuMap;

    [Header("Inventory")]
    public GameObject par_Panel;
    public Button btn_Template;
    public Button btn_BuyItem;
    public Button btn_SellItem;
    public Button btn_Take;
    public Button btn_TakeFromContainer;
    public Button btn_BuyFromTrader;
    public Button btn_Place;
    public Button btn_PlaceIntoContainer;
    public Button btn_SellToTrader;
    public Button btn_Drop;
    public Button btn_Destroy;
    public Button btn_Equip;
    public Button btn_Unequip;
    public Button btn_Consume;
    public Button btn_Repair;
    public Button btn_ShowAll;
    public Button btn_ShowWeapons;
    public Button btn_ShowArmor;
    public Button btn_ShowConsumables;
    public Button btn_ShowAmmo;
    public Button btn_ShowGear;
    public Button btn_ShowMisc;
    public Button btn_DiscardDeadBody;
    public Button btn_AddBattery;
    public Button btn_RemoveBattery;
    public TMP_Text txt_InventoryName;
    public TMP_Text txt_PlayerInventorySpace;
    public TMP_Text txt_PlayerMoney;
    [SerializeField] private Inv_Player PlayerInventoryScript;

    [Header("Item stats")]
    public GameObject par_Stats;
    public TMP_Text txt_ItemName;
    public TMP_Text txt_ItemDescription;
    public TMP_Text txt_ItemWeight;
    public TMP_Text txt_ItemValue;
    public TMP_Text txt_ItemDurability;
    public TMP_Text txt_ItemRemainder;
    public TMP_Text txt_WeaponDamage;
    public TMP_Text txt_AmmoCount;
    public TMP_Text txt_protected;
    public TMP_Text txt_notStackable;
    public TMP_Text txt_tooHeavy;
    public TMP_Text txt_tooExpensive;

    [Header("Item count")]
    public GameObject par_ItemCount;
    public Slider itemCountSlider;
    public TMP_Text txt_CountInfo;
    public TMP_Text txt_CountValue;
    public TMP_Text txt_SliderInfo;
    public Button btn_ConfirmCount;
    public Button btn_CancelCount;

    [Header("Quests")]
    //hover quest parts
    public TMP_Text txt_QuestHoverTitle;
    public RawImage bgr_QuestHoverBackground;
    [HideInInspector] public string questTitle;
    //real quest parts
    public GameObject par_RealQuestUI;
    public GameObject par_QuestsPanel;
    public TMP_Text txt_QuestName;
    public TMP_Text txt_QuestGiver;
    public TMP_Text txt_QuestGiverClan;
    public TMP_Text txt_QuestStatus;
    public TMP_Text txt_QuestRewards;
    public TMP_Text txt_QuestDescription;
    public Button btn_AcceptedQuests;
    public Button btn_FinishedQuests;
    [SerializeField] private UI_AcceptedQuests PlayerQuestsScript;

    [Header("Player menu stats")]
    public GameObject par_EquippedHandgun;
    public GameObject par_HandgunClipSizeUpgrade;
    public GameObject par_HandgunAmmoTypeUpgrade;
    public GameObject par_HandgunFireRateUpgrade;
    public GameObject par_HandgunBulletDropUpgrade;
    public GameObject par_HandgunAccuracyUpgrade;
    public TMP_Text txt_playerStatsHealth;
    public TMP_Text txt_playerStatsHealthProtection;
    public TMP_Text txt_playerStatsHealthRegen;
    public TMP_Text txt_playerStatsStamina;
    public TMP_Text txt_playerStatsStaminaRegen;
    public TMP_Text txt_playerStatsMentalState;
    public TMP_Text txt_playerStatsMentalProtection;
    public TMP_Text txt_playerStatsRadiation;
    public TMP_Text txt_playerStatsRadiationProtection;
    public TMP_Text txt_gunName;
    public TMP_Text txt_gunDurability;
    public TMP_Text txt_gunDurabilityValue;
    public RawImage playerStatsMinorBleeding;
    public RawImage playerStatsModerateBleeding;
    public RawImage playerStatsSevereBleeding;
    public RawImage playerStatsMinorRadiationDamage;
    public RawImage playerStatsModerateRadiationDamage;
    public RawImage playerStatsSevereRadiationDamage;
    public RawImage playerStatsMinorPsyDamage;
    public RawImage playerStatsModeratePsyDamage;
    public RawImage playerStatsSeverePsyDamage;

    [Header("Ability UI")]
    public TMP_Text txt_assigningToSlot;
    public TMP_Text txt_UpgradeCellCount;
    public GameObject par_AbilityUI;
    public GameObject par_AssingedAbilityButtonUI;

    [Header("Factions")]
    [SerializeField] private TMP_Text txt_factionRepTemplate;
    [SerializeField] private Transform pos_factionRepTemplateSpawn;
    [SerializeField] private Transform par_textSpawn;
    [HideInInspector] public List<TMP_Text> texts;

    [Header("Player map")]
    public GameObject par_MainMapMask;
    public GameObject par_PlayerMinimap;
    public GameObject par_MinimapMask;
    public RawImage Minimap;
    public RawImage MinimapPlayerPosition;
    public GameObject par_TeleportCheck;
    public TMP_Text txt_Teleport;
    public Button btn_ConfirmTeleport;
    public Button btn_CancelTeleport;

    [HideInInspector] public float health;
    [HideInInspector] public float maxHealth;
    [Header("Player Values")]
    [SerializeField] private GameObject par_PlayerMainUIValues;
    [SerializeField] private RawImage bgr_health1;
    [SerializeField] private RawImage bgr_health2;
    [SerializeField] private RawImage bgr_health3;
    [SerializeField] private RawImage bgr_health4;
    [SerializeField] private RawImage bgr_health5;
    [SerializeField] private RawImage bgr_health6;
    [SerializeField] private RawImage bgr_health7;
    [SerializeField] private RawImage bgr_health8;
    [SerializeField] private RawImage bgr_health9;
    [SerializeField] private RawImage bgr_health10;
    [HideInInspector] public float stamina;
    [HideInInspector] public float maxStamina;
    [SerializeField] private RawImage bgr_stamina1;
    [SerializeField] private RawImage bgr_stamina2;
    [SerializeField] private RawImage bgr_stamina3;
    [SerializeField] private RawImage bgr_stamina4;
    [SerializeField] private RawImage bgr_stamina5;
    [SerializeField] private RawImage bgr_stamina6;
    [SerializeField] private RawImage bgr_stamina7;
    [SerializeField] private RawImage bgr_stamina8;
    [SerializeField] private RawImage bgr_stamina9;
    [SerializeField] private RawImage bgr_stamina10;
    [HideInInspector] public float mentalState;
    [HideInInspector] public float maxMentalState;
    [SerializeField] private RawImage bgr_mentalState1;
    [SerializeField] private RawImage bgr_mentalState2;
    [SerializeField] private RawImage bgr_mentalState3;
    [SerializeField] private RawImage bgr_mentalState4;
    [SerializeField] private RawImage bgr_mentalState5;
    [SerializeField] private RawImage bgr_mentalState6;
    [SerializeField] private RawImage bgr_mentalState7;
    [SerializeField] private RawImage bgr_mentalState8;
    [SerializeField] private RawImage bgr_mentalState9;
    [SerializeField] private RawImage bgr_mentalState10;
    [HideInInspector] public float radiation;
    [HideInInspector] public float maxRadiation;
    [SerializeField] private RawImage bgr_radiation1;
    [SerializeField] private RawImage bgr_radiation2;
    [SerializeField] private RawImage bgr_radiation3;
    [SerializeField] private RawImage bgr_radiation4;
    [SerializeField] private RawImage bgr_radiation5;
    [SerializeField] private RawImage bgr_radiation6;
    [SerializeField] private RawImage bgr_radiation7;
    [SerializeField] private RawImage bgr_radiation8;
    [SerializeField] private RawImage bgr_radiation9;
    [SerializeField] private RawImage bgr_radiation10;
    [HideInInspector] public float flashlightBattery;
    [HideInInspector] public float flashlightMaxBattery;
    [SerializeField] private RawImage bgr_flashlight1;
    [SerializeField] private RawImage bgr_flashlight2;
    [SerializeField] private RawImage bgr_flashlight3;
    [SerializeField] private RawImage bgr_flashlight4;
    [SerializeField] private RawImage bgr_flashlight5;
    [SerializeField] private RawImage bgr_flashlight6;
    [SerializeField] private RawImage bgr_flashlight7;
    [SerializeField] private RawImage bgr_flashlight8;
    [SerializeField] private RawImage bgr_flashlight9;
    [SerializeField] private RawImage bgr_flashlight10;

    [Header("Element damage types")]
    [SerializeField] private GameObject par_PlayerMainUIElementIcons;
    public RawImage minorRadiationDamage;
    public RawImage moderateRadiationDamage;
    public RawImage severeRadiationDamage;
    public RawImage minorFireDamage;
    public RawImage moderateFireDamage;
    public RawImage severeFireDamage;
    public RawImage minorElectricityDamage;
    public RawImage moderateElectricityDamage;
    public RawImage severeElectricityDamage;
    public RawImage minorGasDamage;
    public RawImage moderateGasDamage;
    public RawImage severeGasDamage;
    public RawImage minorPsyDamage;
    public RawImage moderatePsyDamage;
    public RawImage severePsyDamage;

    [Header("Gun values")]
    public TMP_Text txt_ammoInClip;
    public TMP_Text txt_ammoForGun;
    [HideInInspector] public float durability;
    [HideInInspector] public float maxDurability;
    [SerializeField] private RawImage bgr_weaponQuality1;
    [SerializeField] private RawImage bgr_weaponQuality2;
    [SerializeField] private RawImage bgr_weaponQuality3;
    [SerializeField] private RawImage bgr_weaponQuality4;
    [SerializeField] private RawImage bgr_weaponQuality5;
    [SerializeField] private RawImage bgr_weaponQuality6;
    [SerializeField] private RawImage bgr_weaponQuality7;
    [SerializeField] private RawImage bgr_weaponQuality8;
    [SerializeField] private RawImage bgr_weaponQuality9;
    [SerializeField] private RawImage bgr_weaponQuality10;

    [Header("Grenade values")]
    public RawImage bgr_grenadeTimer1;
    public RawImage bgr_grenadeTimer2;
    public RawImage bgr_grenadeTimer3;
    public RawImage bgr_grenadeTimer4;
    public RawImage bgr_grenadeTimer5;

    [Header("Dialogue")]
    public GameObject par_Dialogue;
    public TMP_Text txt_NPCName;
    public TMP_Text txt_NPCDialogue;
    public Button btn_dialogueTemplate;
    public GameObject par_DialoguePanel;
    [HideInInspector] public List<Button> buttons;

    [Header("Time")]
    public GameObject par_TimeSlider;
    public TMP_Text txt_TimeToWait;
    public TMP_Text txt_CurrentTime;
    public Slider timeSlider;
    public Button btn_Confirm;
    public Button btn_Cancel;

    [Header("Lock")]
    public GameObject par_Lock;
    public GameObject lockpick_Body;
    public TMP_Text txt_LockStatus;
    public TMP_Text txt_LockDifficulty;
    public TMP_Text txt_RemainingLockpicks;
    public Button btn_CancelLockpicking;

    [Header("Console")]
    public GameObject par_Console;
    public GameObject par_Content;
    public TMP_Text txt_InsertedTextTemplate;
    public TMP_InputField txt_InsertedTextSlot;

    [Header("Pause menu")]
    public GameObject par_PauseMenu;
    public GameObject par_PauseMenuContent;
    public GameObject par_KeyCommandsContent;
    public GameObject par_ConsoleCommandsContent;
    public Button btn_ReturnToGame;
    public Button btn_ReturnToPauseMenu;

    private void Start()
    {
        InteractUIDisabled();

        txt_HoverItemCount.text = "";
        bgr_HoverItemCountBackground.gameObject.SetActive(false);

        //stun image is always white but invisible when not in use
        //and it is positioned away from the screen to avoid UI interaction problems
        bgr_PlayerStun.color = new Color32(255, 255, 255, 0);
        bgr_PlayerStun.transform.localPosition = new Vector3(0, -1200, 0);

        HideExoskeletonUI();

        par_AbilityUI.SetActive(false);
        txt_assigningToSlot.text = "";

        ClearWeaponUI();

        ClearGrenadeUI();

        ClearFlashlightUI();

        ClearStatsUI();

        txt_playerStatsHealthProtection.text = "+0";
        txt_playerStatsHealthRegen.text = "+ 0/s";
        txt_playerStatsStaminaRegen.text = "+ 0/s";
        txt_playerStatsMentalProtection.text = "-0";
        txt_playerStatsRadiationProtection.text = "-0";

        ClearInventoryUI();
        ClearAllInventories();

        ClearCountSliderUI();

        ClearTimeSliderUI();

        par_ItemCount.SetActive(false);

        btn_TakeFromContainer.gameObject.SetActive(false);
        btn_BuyFromTrader.gameObject.SetActive(false);
        btn_PlaceIntoContainer.gameObject.SetActive(false);
        btn_SellToTrader.gameObject.SetActive(false);

        minorRadiationDamage.gameObject.SetActive(false);
        moderateRadiationDamage.gameObject.SetActive(false);
        severeRadiationDamage.gameObject.SetActive(false);
        minorFireDamage.gameObject.SetActive(false);
        moderateFireDamage.gameObject.SetActive(false);
        severeFireDamage.gameObject.SetActive(false);
        minorElectricityDamage.gameObject.SetActive(false);
        moderateElectricityDamage.gameObject.SetActive(false);
        severeElectricityDamage.gameObject.SetActive(false);
        minorGasDamage.gameObject.SetActive(false);
        moderateGasDamage.gameObject.SetActive(false);
        severeGasDamage.gameObject.SetActive(false);
        minorPsyDamage.gameObject.SetActive(false);
        moderatePsyDamage.gameObject.SetActive(false);
        severePsyDamage.gameObject.SetActive(false);

        txt_playerStatsHealth.text = "";
        txt_playerStatsStamina.text = "";
        txt_playerStatsMentalState.text = "";
        txt_playerStatsRadiation.text = "";

        txt_gunName.text = "";
        txt_gunDurability.text = "";
        txt_gunDurabilityValue.text = "";
        par_EquippedHandgun.SetActive(false);
        par_HandgunClipSizeUpgrade.SetActive(false);
        par_HandgunAmmoTypeUpgrade.SetActive(false);
        par_HandgunFireRateUpgrade.SetActive(false);
        par_HandgunBulletDropUpgrade.SetActive(false);
        par_HandgunAccuracyUpgrade.SetActive(false);

        playerStatsMinorBleeding.gameObject.SetActive(false);
        playerStatsModerateBleeding.gameObject.SetActive(false);
        playerStatsSevereBleeding.gameObject.SetActive(false);
        playerStatsMinorPsyDamage.gameObject.SetActive(false);
        playerStatsModeratePsyDamage.gameObject.SetActive(false);
        playerStatsSeverePsyDamage.gameObject.SetActive(false);
        playerStatsMinorRadiationDamage.gameObject.SetActive(false);
        playerStatsModerateRadiationDamage.gameObject.SetActive(false);
        playerStatsSevereRadiationDamage.gameObject.SetActive(false);

        txt_QuestHoverTitle.text = "";
        bgr_QuestHoverBackground.gameObject.SetActive(false);
        ClearQuestUI();
        par_RealQuestUI.SetActive(false);

        CloseAllDialogueUI();

        par_TeleportCheck.SetActive(false);

        par_Lock.SetActive(false);

        par_Console.SetActive(false);
        txt_InsertedTextTemplate.text = "";

        par_PauseMenu.SetActive(false);
        par_PauseMenuContent.SetActive(false);
        par_KeyCommandsContent.SetActive(false);
        par_ConsoleCommandsContent.SetActive(false);

        par_Inventory.SetActive(false);
        par_Stats.SetActive(false);
        par_PlayerMenuStats.SetActive(false);
        par_PlayerUpgrades.SetActive(false);
        par_PlayerFactionUI.SetActive(false);
        par_PlayerMenuRadio.SetActive(false);
        par_PlayerMenuMap.SetActive(false);
        par_PlayerMenu.SetActive(false);
        btn_ReturnToPauseMenu.gameObject.SetActive(false);

        btn_CloseUI.gameObject.SetActive(false);
    }

    public void InteractUIEnabled()
    {
        img_Interact.enabled = true;
        cursor.SetActive(false);
    }

    public void InteractUIDisabled()
    {
        img_Interact.enabled = false;
        cursor.SetActive(true);
    }

    private void HideExoskeletonUI()
    {
        par_PlayerMinimap.transform.localPosition += new Vector3(0, 500, 0);
        par_PlayerMainUIValues.transform.localPosition -= new Vector3(0, 750, 0);
        par_PlayerMainUIElementIcons.transform.localPosition -= new Vector3(100, 0, 0);
        par_AbilityUI.transform.localPosition -= new Vector3(0, 1000, 0);
        par_AssingedAbilityButtonUI.transform.localPosition -= new Vector3(0, 300, 0);
    }
    public void ShowExoskeletonUI()
    {
        par_PlayerMinimap.transform.localPosition -= new Vector3(0, 500, 0);
        par_PlayerMainUIValues.transform.localPosition += new Vector3(0, 750, 0);
        par_PlayerMainUIElementIcons.transform.localPosition += new Vector3(100, 0, 0);
        par_AbilityUI.transform.localPosition += new Vector3(0, 1000, 0);
        par_AssingedAbilityButtonUI.transform.localPosition += new Vector3(0, 300, 0);
    }

    public void RebuildPlayerInventory()
    {
        //Debug.Log("Rebuilding player inventory...");
        if (PlayerInventoryScript.showingAllItems)
        {
            PlayerInventoryScript.ShowAll();
        }
        else if (PlayerInventoryScript.showingAllWeapons)
        {
            PlayerInventoryScript.ShowWeapons();
        }
        else if (PlayerInventoryScript.showingAllArmor)
        {
            PlayerInventoryScript.ShowArmor();
        }
        else if (PlayerInventoryScript.showingAllConsumables)
        {
            PlayerInventoryScript.ShowConsumables();
        }
        else if (PlayerInventoryScript.showingAllAmmo)
        {
            PlayerInventoryScript.ShowAmmo();
        }
        else if (PlayerInventoryScript.showingAllGear)
        {
            PlayerInventoryScript.ShowGear();
        }
        else if (PlayerInventoryScript.showingAllMisc)
        {
            PlayerInventoryScript.ShowMisc();
        }
    }
    public void RebuildContainerInventory()
    {
        var containerScript = PlayerInventoryScript.Container.GetComponent<Inv_Container>();

        //Debug.Log("Rebuilding container inventory...");
        if (containerScript.showingAllItems)
        {
            containerScript.ShowAll();
        }
        else if (containerScript.showingAllWeapons)
        {
            containerScript.ShowWeapons();
        }
        else if (containerScript.showingAllArmor)
        {
            containerScript.ShowArmor();
        }
        else if (containerScript.showingAllConsumables)
        {
            containerScript.ShowConsumables();
        }
        else if (containerScript.showingAllAmmo)
        {
            containerScript.ShowAmmo();
        }
        else if (containerScript.showingAllGear)
        {
            containerScript.ShowGear();
        }
        else if (containerScript.showingAllMisc)
        {
            containerScript.ShowMisc();
        }
    }
    public void RebuildShopInventory()
    {
        var shopScript = PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>();

        //Debug.Log("Rebuilding trader inventory...");
        if (shopScript.showingAllItems)
        {
            shopScript.ShowAll();
        }
        else if (shopScript.showingAllWeapons)
        {
            shopScript.ShowWeapons();
        }
        else if (shopScript.showingAllArmor)
        {
            shopScript.ShowArmor();
        }
        else if (shopScript.showingAllConsumables)
        {
            shopScript.ShowConsumables();
        }
        else if (shopScript.showingAllAmmo)
        {
            shopScript.ShowAmmo();
        }
        else if (shopScript.showingAllGear)
        {
            shopScript.ShowGear();
        }
        else if (shopScript.showingAllMisc)
        {
            shopScript.ShowMisc();
        }
    }
    public void RebuildRepairMenu()
    {
        ClearAllInventories();
        foreach (GameObject item in PlayerInventoryScript.inventory)
        {
            if (item != null && item.GetComponent<Item_Gun>() != null)
            {
                Button btn_New = Instantiate(btn_Template);
                btn_New.transform.SetParent(par_Panel.transform, false);

                for (int i = 0; i < item.GetComponent<Env_Item>().str_ItemName.Length - 1; i++)
                {
                    if (item.GetComponent<Env_Item>().str_ItemName[i] == '_')
                    {
                        item.GetComponent<Env_Item>().hasUnderscore = true;
                        break;
                    }
                }
                if (item.GetComponent<Env_Item>().hasUnderscore)
                {
                    string str_fakeName = item.GetComponent<Env_Item>().str_ItemName.Replace("_", " ");
                    btn_New.GetComponentInChildren<TMP_Text>().text = str_fakeName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                }
                else if (!item.GetComponent<Env_Item>().hasUnderscore)
                {
                    btn_New.GetComponentInChildren<TMP_Text>().text = item.GetComponent<Env_Item>().str_ItemName + " x" + item.GetComponent<Env_Item>().int_itemCount;
                }

                btn_New.onClick.AddListener(item.GetComponent<Env_Item>().ShowStats);

                if (PlayerInventoryScript.Workbench != null)
                {
                    PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().buttons.Add(btn_New.gameObject);
                }
                else if (PlayerInventoryScript.Trader != null)
                {
                    PlayerInventoryScript.Trader.GetComponent<UI_RepairContent>().buttons.Add(btn_New.gameObject);
                }

                item.GetComponent<Env_Item>().isInRepairMenu = true;
            }
        }
    }

    public void ClearStatsUI()
    {
        txt_ItemName.text = "";
        txt_ItemDescription.text = "";
        txt_ItemValue.text = "";
        txt_ItemWeight.text = "";
        txt_WeaponDamage.text = "";
        txt_AmmoCount.text = "";
        txt_CountInfo.text = "";
        txt_CountValue.text = "";
        txt_SliderInfo.text = "";
        txt_ItemDurability.text = "";
        txt_ItemRemainder.text = "";
        txt_protected.gameObject.SetActive(false);
        txt_notStackable.gameObject.SetActive(false);
        txt_tooHeavy.gameObject.SetActive(false);
        txt_tooExpensive.gameObject.SetActive(false);
    }
    public void ClearInventoryUI()
    {
        btn_BuyItem.gameObject.SetActive(false);
        btn_SellItem.gameObject.SetActive(false);
        btn_Take.gameObject.SetActive(false);
        btn_Place.gameObject.SetActive(false);
        btn_Drop.gameObject.SetActive(false);
        btn_Destroy.gameObject.SetActive(false);
        btn_Equip.gameObject.SetActive(false);
        btn_Unequip.gameObject.SetActive(false);
        btn_Consume.gameObject.SetActive(false);
        btn_Repair.gameObject.SetActive(false);
        btn_ShowAll.gameObject.SetActive(false);
        btn_ShowWeapons.gameObject.SetActive(false);
        btn_ShowArmor.gameObject.SetActive(false);
        btn_ShowConsumables.gameObject.SetActive(false);
        btn_ShowAmmo.gameObject.SetActive(false);
        btn_ShowGear.gameObject.SetActive(false);
        btn_ShowMisc.gameObject.SetActive(false);
        btn_DiscardDeadBody.gameObject.SetActive(false);
        btn_AddBattery.gameObject.SetActive(false);
        btn_RemoveBattery.gameObject.SetActive(false);
        txt_InventoryName.text = "";
        txt_PlayerInventorySpace.text = "";
        txt_PlayerMoney.text = "";

        ClearCountSliderUI();
    }
    public void ClearCountSliderUI()
    {
        par_ItemCount.SetActive(false);
        itemCountSlider.value = 1;
        itemCountSlider.maxValue = 1;
        txt_CountInfo.text = "";
        txt_CountValue.text = "";
        txt_CountValue.color = Color.white;
        txt_SliderInfo.text = "";
        btn_ConfirmCount.interactable = true;
    }

    public void ClearTimeSliderUI()
    {
        par_TimeSlider.SetActive(false);
        txt_TimeToWait.text = "";
        txt_CurrentTime.text = "";
        btn_Confirm.onClick.RemoveAllListeners();
        btn_Cancel.onClick.RemoveAllListeners();
    }

    public void EnableInventorySortButtons()
    {
        btn_ShowAll.gameObject.SetActive(true);
        btn_ShowWeapons.gameObject.SetActive(true);
        btn_ShowArmor.gameObject.SetActive(true);
        btn_ShowConsumables.gameObject.SetActive(true);
        btn_ShowAmmo.gameObject.SetActive(true);
        btn_ShowGear.gameObject.SetActive(true);
        btn_ShowMisc.gameObject.SetActive(true);
    }

    public void ClearAllInventories()
    {
        //clear player inv buttons
        foreach (GameObject button in PlayerInventoryScript.buttons)
        {
            Destroy(button);
        }
        PlayerInventoryScript.buttons.Clear();
        //clear container buttons
        if (PlayerInventoryScript.Container != null)
        {
            foreach (GameObject button in PlayerInventoryScript.Container.GetComponent<Inv_Container>().buttons)
            {
                Destroy(button);
            }
            PlayerInventoryScript.Container.GetComponent<Inv_Container>().buttons.Clear();
        }
        //clear trader shop buttons
        if (PlayerInventoryScript.Trader != null
            && !PlayerInventoryScript.isPlayerAndRepairOpen)
        {
            foreach (GameObject button in PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().buttons)
            {
                Destroy(button);
            }
            PlayerInventoryScript.Trader.GetComponent<UI_ShopContent>().buttons.Clear();
        }
        //clear trader repair menu buttons
        if (PlayerInventoryScript.Trader != null
            && PlayerInventoryScript.isPlayerAndRepairOpen)
        {
            foreach (GameObject button in PlayerInventoryScript.Trader.GetComponent<UI_RepairContent>().buttons)
            {
                Destroy(button);
            }
            PlayerInventoryScript.Trader.GetComponent<UI_RepairContent>().buttons.Clear();
        }
        //clear workbench repair menu buttons
        if (PlayerInventoryScript.Workbench != null)
        {
            foreach (GameObject button in PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().buttons)
            {
                Destroy(button);
            }
            PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().buttons.Clear();
        }
    }

    public void UpdatePlayerHealth()
    {
        float healthPercentage = health / maxHealth * 100;

        ClearHealthUI();
        if (healthPercentage > 0)
        {
            bgr_health1.enabled = true;
            if (healthPercentage >= 10)
            {
                bgr_health2.enabled = true;
                if (healthPercentage >= 20)
                {
                    bgr_health3.enabled = true;
                    if (healthPercentage >= 30)
                    {
                        bgr_health4.enabled = true;
                        if (healthPercentage >= 40)
                        {
                            bgr_health5.enabled = true;
                            if (healthPercentage >= 50)
                            {
                                bgr_health6.enabled = true;
                                if (healthPercentage >= 60)
                                {
                                    bgr_health7.enabled = true;
                                    if (healthPercentage >= 70)
                                    {
                                        bgr_health8.enabled = true;
                                        if (healthPercentage >= 80)
                                        {
                                            bgr_health9.enabled = true;
                                            if (healthPercentage >= 90)
                                            {
                                                bgr_health10.enabled = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void UpdatePlayerStamina()
    {
        float staminaPercentage = stamina / maxStamina * 100;

        ClearStaminaUI();
        if (staminaPercentage > 0)
        {
            bgr_stamina1.enabled = true;
            if (staminaPercentage >= 10)
            {
                bgr_stamina2.enabled = true;
                if (staminaPercentage >= 20)
                {
                    bgr_stamina3.enabled = true;
                    if (staminaPercentage >= 30)
                    {
                        bgr_stamina4.enabled = true;
                        if (staminaPercentage >= 40)
                        {
                            bgr_stamina5.enabled = true;
                            if (staminaPercentage >= 50)
                            {
                                bgr_stamina6.enabled = true;
                                if (staminaPercentage >= 60)
                                {
                                    bgr_stamina7.enabled = true;
                                    if (staminaPercentage >= 70)
                                    {
                                        bgr_stamina8.enabled = true;
                                        if (staminaPercentage >= 80)
                                        {
                                            bgr_stamina9.enabled = true;
                                            if (staminaPercentage >= 90)
                                            {
                                                bgr_stamina10.enabled = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void UpdatePlayerMentalState()
    {
        float mentalStatePercentagePercentage = mentalState / maxMentalState * 100;

        ClearMentalStateUI();
        if (mentalStatePercentagePercentage > 0)
        {
            bgr_mentalState1.enabled = true;
            if (mentalStatePercentagePercentage >= 10)
            {
                bgr_mentalState2.enabled = true;
                if (mentalStatePercentagePercentage >= 20)
                {
                    bgr_mentalState3.enabled = true;
                    if (mentalStatePercentagePercentage >= 30)
                    {
                        bgr_mentalState4.enabled = true;
                        if (mentalStatePercentagePercentage >= 40)
                        {
                            bgr_mentalState5.enabled = true;
                            if (mentalStatePercentagePercentage >= 50)
                            {
                                bgr_mentalState6.enabled = true;
                                if (mentalStatePercentagePercentage >= 60)
                                {
                                    bgr_mentalState7.enabled = true;
                                    if (mentalStatePercentagePercentage >= 70)
                                    {
                                        bgr_mentalState8.enabled = true;
                                        if (mentalStatePercentagePercentage >= 80)
                                        {
                                            bgr_mentalState9.enabled = true;
                                            if (mentalStatePercentagePercentage >= 90)
                                            {
                                                bgr_mentalState10.enabled = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void UpdatePlayerRadiation()
    {
        float radiationPercentage = radiation / maxRadiation * 100;

        ClearRadiationUI();
        if (radiationPercentage > 0)
        {
            bgr_radiation1.enabled = true;
            if (radiationPercentage >= 10)
            {
                bgr_radiation2.enabled = true;
                if (radiationPercentage >= 20)
                {
                    bgr_radiation3.enabled = true;
                    if (radiationPercentage >= 30)
                    {
                        bgr_radiation4.enabled = true;
                        if (radiationPercentage >= 40)
                        {
                            bgr_radiation5.enabled = true;
                            if (radiationPercentage >= 50)
                            {
                                bgr_radiation6.enabled = true;
                                if (radiationPercentage >= 60)
                                {
                                    bgr_radiation7.enabled = true;
                                    if (radiationPercentage >= 70)
                                    {
                                        bgr_radiation8.enabled = true;
                                        if (radiationPercentage >= 80)
                                        {
                                            bgr_radiation9.enabled = true;
                                            if (radiationPercentage >= 90)
                                            {
                                                bgr_radiation10.enabled = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void UpdatePlayerFlashlight()
    {
        float flashlightPercentage = flashlightBattery / flashlightMaxBattery * 100;

        ClearFlashlightUI();
        if (flashlightPercentage > 0)
        {
            bgr_flashlight1.enabled = true;
            if (flashlightPercentage >= 10)
            {
                bgr_flashlight2.enabled = true;
                if (flashlightPercentage >= 20)
                {
                    bgr_flashlight3.enabled = true;
                    if (flashlightPercentage >= 30)
                    {
                        bgr_flashlight4.enabled = true;
                        if (flashlightPercentage >= 40)
                        {
                            bgr_flashlight5.enabled = true;
                            if (flashlightPercentage >= 50)
                            {
                                bgr_flashlight6.enabled = true;
                                if (flashlightPercentage >= 60)
                                {
                                    bgr_flashlight7.enabled = true;
                                    if (flashlightPercentage >= 70)
                                    {
                                        bgr_flashlight8.enabled = true;
                                        if (flashlightPercentage >= 80)
                                        {
                                            bgr_flashlight9.enabled = true;
                                            if (flashlightPercentage >= 90)
                                            {
                                                bgr_flashlight10.enabled = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void UpdateWeaponQuality()
    {
        float weaponQualityPercentage = durability / maxDurability * 100;

        ClearWeaponUI();
        if (weaponQualityPercentage > 0)
        {
            bgr_weaponQuality1.enabled = true;
            if (weaponQualityPercentage >= 10)
            {
                bgr_weaponQuality2.enabled = true;
                if (weaponQualityPercentage >= 20)
                {
                    bgr_weaponQuality3.enabled = true;
                    if (weaponQualityPercentage >= 30)
                    {
                        bgr_weaponQuality4.enabled = true;
                        if (weaponQualityPercentage >= 40)
                        {
                            bgr_weaponQuality5.enabled = true;
                            if (weaponQualityPercentage >= 50)
                            {
                                bgr_weaponQuality6.enabled = true;
                                if (weaponQualityPercentage >= 60)
                                {
                                    bgr_weaponQuality7.enabled = true;
                                    if (weaponQualityPercentage >= 70)
                                    {
                                        bgr_weaponQuality8.enabled = true;
                                        if (weaponQualityPercentage >= 80)
                                        {
                                            bgr_weaponQuality9.enabled = true;
                                            if (weaponQualityPercentage >= 90)
                                            {
                                                bgr_weaponQuality10.enabled = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public void ClearHealthUI()
    {
        bgr_health1.enabled = false;
        bgr_health2.enabled = false;
        bgr_health3.enabled = false;
        bgr_health4.enabled = false;
        bgr_health5.enabled = false;
        bgr_health6.enabled = false;
        bgr_health7.enabled = false;
        bgr_health8.enabled = false;
        bgr_health9.enabled = false;
        bgr_health10.enabled = false;
    }
    public void ClearStaminaUI()
    {
        bgr_stamina1.enabled = false;
        bgr_stamina2.enabled = false;
        bgr_stamina3.enabled = false;
        bgr_stamina4.enabled = false;
        bgr_stamina5.enabled = false;
        bgr_stamina6.enabled = false;
        bgr_stamina7.enabled = false;
        bgr_stamina8.enabled = false;
        bgr_stamina9.enabled = false;
        bgr_stamina10.enabled = false;
    }
    public void ClearMentalStateUI()
    {
        bgr_mentalState1.enabled = false;
        bgr_mentalState2.enabled = false;
        bgr_mentalState3.enabled = false;
        bgr_mentalState4.enabled = false;
        bgr_mentalState5.enabled = false;
        bgr_mentalState6.enabled = false;
        bgr_mentalState7.enabled = false;
        bgr_mentalState8.enabled = false;
        bgr_mentalState9.enabled = false;
        bgr_mentalState10.enabled = false;
    }
    public void ClearRadiationUI()
    {
        bgr_radiation1.enabled = false;
        bgr_radiation2.enabled = false;
        bgr_radiation3.enabled = false;
        bgr_radiation4.enabled = false;
        bgr_radiation5.enabled = false;
        bgr_radiation6.enabled = false;
        bgr_radiation7.enabled = false;
        bgr_radiation8.enabled = false;
        bgr_radiation9.enabled = false;
        bgr_radiation10.enabled = false;
    }
    public void ClearFlashlightUI()
    {
        bgr_flashlight1.enabled = false;
        bgr_flashlight2.enabled = false;
        bgr_flashlight3.enabled = false;
        bgr_flashlight4.enabled = false;
        bgr_flashlight5.enabled = false;
        bgr_flashlight6.enabled = false;
        bgr_flashlight7.enabled = false;
        bgr_flashlight8.enabled = false;
        bgr_flashlight9.enabled = false;
        bgr_flashlight10.enabled = false;
    }
    public void ClearWeaponUI()
    {
        if (PlayerInventoryScript.equippedGun == null)
        {
            txt_ammoInClip.text = "";
            txt_ammoForGun.text = "";
        }
        else if (PlayerInventoryScript.equippedGun != null
                 && PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>() != null)
        {
            txt_ammoInClip.text = PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().currentClipSize.ToString();
            if (PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().ammoClip != null)
            {
                GameObject gunAmmo = PlayerInventoryScript.equippedGun.GetComponent<Item_Gun>().ammoClip;
                string gunAmmoCount = gunAmmo.GetComponent<Env_Item>().int_itemCount.ToString();
                txt_ammoForGun.text = gunAmmoCount;
            }
            else
            {
                txt_ammoForGun.text = "0";
            }
        }
        else
        {
            txt_ammoInClip.text = "";
            txt_ammoForGun.text = "0";
        }

        bgr_weaponQuality1.enabled = false;
        bgr_weaponQuality2.enabled = false;
        bgr_weaponQuality3.enabled = false;
        bgr_weaponQuality4.enabled = false;
        bgr_weaponQuality5.enabled = false;
        bgr_weaponQuality6.enabled = false;
        bgr_weaponQuality7.enabled = false;
        bgr_weaponQuality8.enabled = false;
        bgr_weaponQuality9.enabled = false;
        bgr_weaponQuality10.enabled = false;
    }

    public void ClearGrenadeUI()
    {
        bgr_grenadeTimer1.color = new Color32(32, 32, 32, 225);
        bgr_grenadeTimer2.color = new Color32(32, 32, 32, 225);
        bgr_grenadeTimer3.color = new Color32(32, 32, 32, 225);
        bgr_grenadeTimer4.color = new Color32(32, 32, 32, 225);
        bgr_grenadeTimer5.color = new Color32(32, 32, 32, 225);

        bgr_grenadeTimer1.gameObject.SetActive(false);
        bgr_grenadeTimer2.gameObject.SetActive(false);
        bgr_grenadeTimer3.gameObject.SetActive(false);
        bgr_grenadeTimer4.gameObject.SetActive(false);
        bgr_grenadeTimer5.gameObject.SetActive(false);
    }

    public void RebuildAcceptedQuestsList()
    {
        ClearQuestUI();

        for (int i = 0; i < PlayerQuestsScript.acceptedQuests.Count; i++)
        {
            GameObject quest = PlayerQuestsScript.acceptedQuests[i];

            //create new button
            Button btn_New = Instantiate(btn_Template);
            //set parent
            btn_New.transform.SetParent(par_QuestsPanel.transform, false);
            //change button name
            btn_New.GetComponentInChildren<TMP_Text>().text = quest.GetComponent<UI_QuestContent>().str_questTitle;
            //remove any existing tasks
            btn_New.onClick.RemoveAllListeners();
            //add task
            btn_New.onClick.AddListener(quest.GetComponent<UI_QuestContent>().ShowStats);
            //add created button to buttons list
            PlayerQuestsScript.buttons.Add(btn_New.gameObject);
        }
    }
    public void RebuildCompletedQuestsList()
    {
        ClearQuestUI();

        for (int i = 0; i < PlayerQuestsScript.finishedQuests.Count; i++)
        {
            GameObject quest = PlayerQuestsScript.finishedQuests[i];

            //create new button
            Button btn_New = Instantiate(btn_Template);
            //set parent
            btn_New.transform.SetParent(par_QuestsPanel.transform, false);
            //change button name
            btn_New.GetComponentInChildren<TMP_Text>().text = quest.GetComponent<UI_QuestContent>().str_questTitle;
            //remove any existing tasks
            btn_New.onClick.RemoveAllListeners();
            //add task
            btn_New.onClick.AddListener(quest.GetComponent<UI_QuestContent>().ShowStats);
            //add created button to buttons list
            PlayerQuestsScript.buttons.Add(btn_New.gameObject);
        }
    }

    public void ClearQuestUI()
    {
        txt_QuestName.text = "";
        txt_QuestGiver.text = "";
        txt_QuestGiverClan.text = "";
        txt_QuestStatus.text = "";
        txt_QuestRewards.text = "";
        txt_QuestDescription.text = "";

        foreach (GameObject button in PlayerQuestsScript.buttons)
        {
            Destroy(button);
        }
        PlayerQuestsScript.buttons.Clear();
    }

    public IEnumerator StartedQuestUI()
    {
        txt_QuestHoverTitle.text = "";
        bgr_QuestHoverBackground.gameObject.SetActive(false);

        txt_QuestHoverTitle.text = "Started " + questTitle;
        bgr_QuestHoverBackground.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        txt_QuestHoverTitle.text = "";
        bgr_QuestHoverBackground.gameObject.SetActive(false);
    }
    public IEnumerator CompletedQuestUI()
    {
        txt_QuestHoverTitle.text = "";
        bgr_QuestHoverBackground.gameObject.SetActive(false);

        txt_QuestHoverTitle.text = "Completed " + questTitle;
        bgr_QuestHoverBackground.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        txt_QuestHoverTitle.text = "";
        bgr_QuestHoverBackground.gameObject.SetActive(false);
    }

    public void CloseAllDialogueUI()
    {
        CloseDialogueUI();

        txt_NPCName.text = "";
        txt_NPCDialogue.text = "";
        par_Dialogue.SetActive(false);
    }
    public void CloseDialogueUI()
    {
        foreach (Transform child in par_DialoguePanel.transform)
        {
            Destroy(child.gameObject);
        }
        buttons.Clear();
    }

    public void UpdatePlayerFactionUI()
    {
        //clear texts list
        if (texts.Count > 0)
        {
            foreach (TMP_Text text in texts)
            {
                Destroy(text.gameObject);
            }
            texts.Clear();
        }

        //how far down per faction turn
        float down = -56f;
        //how far right per faction value turn
        float right = 145f;
        //reset last height
        float lastHeight;

        //original spawn position
        Vector3 spawnPos = pos_factionRepTemplateSpawn.position;
        //new spawn position
        Vector3 newPos = spawnPos;

        //each turn moves the text down
        foreach (GameObject faction in gameObject.GetComponent<GameManager>().gameFactions)
        {
            //ignores others faction
            if (faction.GetComponent<Manager_FactionReputation>().faction.ToString() != "Others")
            {
                //save last height
                lastHeight = newPos.y;
                //replace last positions x position with original and keep last height
                newPos = new(spawnPos.x, lastHeight, spawnPos.z);
                //move position down
                newPos += new Vector3(0, down, 0);
                //create 9 new texts
                UpdateIndividualFactionUI(faction, newPos, right);
            }
        }
        //updates each texts color according to its value
        foreach (TMP_Text text in texts)
        {
            int value = int.Parse(text.text);

            //under 500 - hates you
            if (value <= -500)
            {
                text.color = Color.red;
            }
            //between -500 and -100 - doesnt like you
            else if (value > -500 && value < -100)
            {
                text.color = new Color32(255, 255, 0, 255);
            }
            //between -100 and 100 - tolerates you
            else if (value >= -100 && value < 100)
            {
                text.color = Color.white;
            }
            //between 100 and 500 - likes you
            else if (value >= 100 && value < 500)
            {
                text.color = new Color32(0, 255, 100, 255);
            }
            //over 500 - idolizes you
            else if (value >= 500)
            {
                text.color = Color.green;
            }
        }
    }
    private void UpdateIndividualFactionUI(GameObject faction, Vector3 pos, float right)
    {
        int i = 1;
        Manager_FactionReputation factionscript = faction.GetComponent<Manager_FactionReputation>();

        //each turn moves the text right,
        //ignores others faction
        while (i < 9)
        {
            //move right
            pos += new Vector3(right, 0, 0);
            //increase right measurement slowly
            right += 0.5f;
            //create new text at pos
            TMP_Text text = Instantiate(txt_factionRepTemplate,
                                        pos,
                                        Quaternion.identity,
                                        par_textSpawn);

            texts.Add(text);

            //add each factions value to new text
            if (i == 1)
            {
                text.text = factionscript.vsPlayer.ToString();
            }
            else if (i == 2)
            {
                text.text = factionscript.vsScientists.ToString();
            }
            else if (i == 3)
            {
                text.text = factionscript.vsGeifers.ToString();
            }
            else if (i == 4)
            {
                text.text = factionscript.vsAnnies.ToString();
            }
            else if (i == 5)
            {
                text.text = factionscript.vsVerbannte.ToString();
            }
            else if (i == 6)
            {
                text.text = factionscript.vsRaiders.ToString();
            }
            else if (i == 7)
            {
                text.text = factionscript.vsMilitary.ToString();
            }
            else if (i == 8)
            {
                text.text = factionscript.vsVerteidiger.ToString();
            }

            i++;
        }
    }
}