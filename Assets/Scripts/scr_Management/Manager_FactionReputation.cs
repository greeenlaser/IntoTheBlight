using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager_FactionReputation : MonoBehaviour
{
    [Tooltip("What faction is the player in?")]
    public PlayerFaction playerFaction;
    public enum PlayerFaction
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

    //player vs others
    public int pv1;
    public int pv2;
    public int pv3;
    public int pv4;
    public int pv5;
    public int pv6;
    public int pv7;
    public int pv8;

    //scientists vs others
    public int rep1v1;
    public int rep1v2;
    public int rep1v3;
    public int rep1v4;
    public int rep1v5;
    public int rep1v6;
    public int rep1v7;
    public int rep1v8;
    //geifers vs others
    public int rep2v1;
    public int rep2v2;
    public int rep2v3;
    public int rep2v4;
    public int rep2v5;
    public int rep2v6;
    public int rep2v7;
    public int rep2v8;
    //annies vs others
    public int rep3v1;
    public int rep3v2;
    public int rep3v3;
    public int rep3v4;
    public int rep3v5;
    public int rep3v6;
    public int rep3v7;
    public int rep3v8;
    //verbannte vs others
    public int rep4v1;
    public int rep4v2;
    public int rep4v3;
    public int rep4v4;
    public int rep4v5;
    public int rep4v6;
    public int rep4v7;
    public int rep4v8;
    //raiders vs others
    public int rep5v1;
    public int rep5v2;
    public int rep5v3;
    public int rep5v4;
    public int rep5v5;
    public int rep5v6;
    public int rep5v7;
    public int rep5v8;
    //military vs others
    public int rep6v1;
    public int rep6v2;
    public int rep6v3;
    public int rep6v4;
    public int rep6v5;
    public int rep6v6;
    public int rep6v7;
    public int rep6v8;
    //verteidiger vs others
    public int rep7v1;
    public int rep7v2;
    public int rep7v3;
    public int rep7v4;
    public int rep7v5;
    public int rep7v6;
    public int rep7v7;
    public int rep7v8;
    //others vs others
    public int rep8v1;
    public int rep8v2;
    public int rep8v3;
    public int rep8v4;
    public int rep8v5;
    public int rep8v6;
    public int rep8v7;
    public int rep8v8;
}