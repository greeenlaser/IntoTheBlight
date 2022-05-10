using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_FactionReputation : MonoBehaviour
{
    [Header("Assignables")]
    public Faction faction;
    public enum Faction
    {
        unassigned,
        Scientists,
        Geifers,
        Annies,
        Verbannte,
        Raiders,
        Military,
        Verteidiger,
        Others
    }

    public int vsPlayer;
    public int vsScientists;
    public int vsGeifers;
    public int vsAnnies;
    public int vsVerbannte;
    public int vsRaiders;
    public int vsMilitary;
    public int vsVerteidiger;
    public int vsOthers;
}