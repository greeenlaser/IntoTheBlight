using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_RadioStation : MonoBehaviour
{
    [Header("Assignables")]
    public string radioName;
    [Range(89.0f, 107.0f)]
    public float radioFrequency;
    public AudioSource RadioSource;
    [SerializeField] private UI_RadioManager RadioManagerScript;
    [SerializeField] private List<AudioClip> songs = new List<AudioClip>();

    //public but hidden variables
    [HideInInspector] public bool isPlaying;

    private void Start()
    {
        RadioManagerScript.radioStationNames.Add(radioName);
        RadioManagerScript.radioStationFrequencies.Add(radioFrequency);

        RadioSource.volume = 0;
    }

    private void Update()
    {
        //picks a new song once the old one finishes
        if (!RadioSource.isPlaying)
        {
            PlayRadio();
        }
    }

    public void PlayRadio()
    {
        int radioClip = Random.Range(0, songs.Count);
        AudioClip song = songs[radioClip];
        RadioSource.clip = song;
        RadioSource.Play();
    }
    public void TuneRadio()
    {
        isPlaying = true;

        //all other radiostations are silenced
        foreach (GameObject radio in RadioManagerScript.radioStations)
        {
            if (radio != gameObject)
            {
                radio.GetComponent<UI_RadioStation>().RadioSource.volume = 0;
            }
        }

        //outer frequency range
        if ((RadioManagerScript.currentRadioFrequency >= radioFrequency - 0.9f
            && RadioManagerScript.currentRadioFrequency < radioFrequency - 0.6f)
            || (RadioManagerScript.currentRadioFrequency >= radioFrequency + 0.6f
            && RadioManagerScript.currentRadioFrequency < radioFrequency + 0.9f))
        {
            RadioManagerScript.txt_radioConnection.text = "Connection: Poor";
            RadioManagerScript.txt_radioConnection.color = new Color32(255, 0, 0, 255);

            RadioSource.volume = 0.3f;
            RadioManagerScript.staticAudio.volume = 0.6f;
        }

        //middle frequency range
        else if ((RadioManagerScript.currentRadioFrequency >= radioFrequency - 0.6f
                 && RadioManagerScript.currentRadioFrequency < radioFrequency - 0.3f)
                 || (RadioManagerScript.currentRadioFrequency >= radioFrequency + 0.3f 
                 && RadioManagerScript.currentRadioFrequency < radioFrequency + 0.6f))
        {
            RadioManagerScript.txt_radioConnection.text = "Connection: Average";
            RadioManagerScript.txt_radioConnection.color = new Color32(255, 215, 0, 255);

            RadioSource.volume = 0.6f;
            RadioManagerScript.staticAudio.volume = 0.3f;
        }

        //inner frequency range
        else if (RadioManagerScript.currentRadioFrequency >= radioFrequency - 0.3f
                 && RadioManagerScript.currentRadioFrequency < radioFrequency + 0.3f)
        {
            RadioManagerScript.txt_radioConnection.text = "Connection: Excellent";
            RadioManagerScript.txt_radioConnection.color = new Color32(0, 255, 0, 255);

            RadioSource.volume = 1f;
            RadioManagerScript.staticAudio.volume = 0f;
        }
    }
}