using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AudioFollowsPlayerAndNoclip : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private GameObject theNoclipCamera;
    [SerializeField] private GameObject par_PlayerSFX;
    [SerializeField] private Manager_Console ConsoleScript;

    private void Update()
    {
        if (!ConsoleScript.noclipEnabled)
        {
            gameObject.transform.position = thePlayer.transform.position;
        }
        else if (ConsoleScript.noclipEnabled)
        {
            gameObject.transform.position = theNoclipCamera.transform.position;

            //gets all player SFX and stops them from playing
            AudioSource[] audioSources = par_PlayerSFX.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                    Debug.Log("Stopped " + audioSource.gameObject.name + " because player went into noclip mode.");
                }
            }
        }
    }
}