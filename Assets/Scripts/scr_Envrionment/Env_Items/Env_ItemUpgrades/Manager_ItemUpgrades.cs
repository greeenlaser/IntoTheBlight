using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Manager_ItemUpgrades : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private string str_ItemTypeName;
    [SerializeField] private ItemType itemType;
    [SerializeField] private enum ItemType
    {
        gun,
        melee,
        armor,
        gear
    }

    [SerializeField] private List<Vector3> buttonPositions;
    [SerializeField] private List<string> buttonNames;
    [SerializeField] private RawImage bgr_UpgradeBackground;

    [Header("Scripts")]
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    //private variables
    private Manager_UIReuse UIReuseScript;

    private void Awake()
    {
        UIReuseScript = par_Managers.GetComponent<Manager_UIReuse>();
    }

    //this function is called when this item is upgraded
    public void ShowUpgradeButtons()
    {
        //enable the right background image,
        //set correct button positions,
        //set correct button names
        UIReuseScript.ShowUpgradeUI(str_ItemTypeName, bgr_UpgradeBackground,
                                    buttonPositions[0], buttonNames[0],
                                    buttonPositions[1], buttonNames[1],
                                    buttonPositions[2], buttonNames[2],
                                    buttonPositions[3], buttonNames[3],
                                    buttonPositions[4], buttonNames[4],
                                    buttonPositions[5], buttonNames[5]);

        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.onClick.AddListener(CloseUI);

        //if we are upgrading a gun
        if (itemType == ItemType.gun)
        {
            UpdateWeaponUpgradeButtons();
        }
        //if we are upgrading a melee weapon
        else if (itemType == ItemType.melee)
        {
            //no function added yet
        }
        //if we are upgrading an armor
        else if (itemType == ItemType.armor)
        {
            //no function added yet
        }
        //if we are upgrading a gear item
        else if (itemType == ItemType.gear)
        {
            //no function added yet
        }
    }

    public void UpdateWeaponUpgradeButtons()
    {
        UIReuseScript.DisableItemUpgradeButtons();

        //get each button
        foreach (Button btn in UIReuseScript.upgradeButtons)
        {
            //get each script
            foreach (Transform script in transform)
            {
                if (script.GetComponent<Upgrade_Gun>() != null)
                {
                    Upgrade_Gun gunUpradeScript = script.GetComponent<Upgrade_Gun>();
                    int tier = gunUpradeScript.upgradeTier;

                    if (tier < gunUpradeScript.maxTier)
                    {
                        //look for upgrade type
                        if ((btn.GetComponentInChildren<TMP_Text>().text == "Fire type"
                            && script.name.Contains("FireType"))

                            || (btn.GetComponentInChildren<TMP_Text>().text == "Firerate"
                            && script.name.Contains("Firerate"))

                            || (btn.GetComponentInChildren<TMP_Text>().text == "Accuracy"
                            && script.name.Contains("Accuracy"))

                            || (btn.GetComponentInChildren<TMP_Text>().text == "Bullet drop"
                            && script.name.Contains("BulletDrop"))

                            || (btn.GetComponentInChildren<TMP_Text>().text == "Clip size"
                            && script.name.Contains("ClipSize"))

                            || (btn.GetComponentInChildren<TMP_Text>().text == "Ammo type"
                            && script.name.Contains("AmmoType")))
                        {
                            btn.GetComponent<UI_UpgradeButtonTooltip>().target = script.gameObject;

                            btn.interactable = true;

                            if (PlayerInventoryScript.equippedGun != script.parent.gameObject)
                            {
                                script.parent.gameObject.SetActive(true);
                            }

                            btn.onClick.AddListener(script.GetComponent<Upgrade_Gun>().UpgradeGun);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void CloseUI()
    {
        UIReuseScript.btn_CloseUI.onClick.RemoveAllListeners();
        UIReuseScript.btn_CloseUI.onClick.AddListener(PlayerInventoryScript.Workbench.GetComponent<Env_Workbench>().CloseWorkbenchUI);

        foreach (Button btn in UIReuseScript.upgradeButtons)
        {
            btn.GetComponent<UI_UpgradeButtonTooltip>().target = null;
        }

        UIReuseScript.HideItemUpgradeUI();
    }
}