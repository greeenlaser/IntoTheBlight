using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Ammo : MonoBehaviour
{
    [Header("Stats")]
    [Range(5, 2000)]
    [Tooltip("The default damage this bullet deals to everything.")]
    public int ammoDefaultDamage;
    [Range(1, 5)]
    [Tooltip("The extra adder how much this bullet deals to an unprotected head.")]
    public int headshotBonusDamage;
    [Range(25, 100)]
    [Tooltip("How far is this bullet fully effective. Notice: Must be set below maxMinDamageRange.")]
    public int maxFullDamageRange;
    [Range(50, 200)]
    [Tooltip("How far can this bullet shoot before it stops dealing any damage. Notice: Must be set over maxFullDamageRange.")]
    public int maxMinDamageRange;

    [Header("Assignables")]
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
}