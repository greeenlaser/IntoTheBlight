using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Ammo : MonoBehaviour
{
    [Header("Assignables")]
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
        _50BMG_ammo
    }
    public AmmoType ammoType;
    public enum AmmoType
    {
        full_metal_jacket,
        hollow_point,
        armor_piercing,
        incendiary,
        tracer
    }
}