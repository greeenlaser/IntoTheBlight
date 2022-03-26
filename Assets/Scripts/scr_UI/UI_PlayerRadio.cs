using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayerRadio : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Button btn_UnmuteRadio;
    [SerializeField] private Button btn_MuteRadio;
    [SerializeField] private TMP_Text txt_radioFrequencyValue;
    [SerializeField] private Slider radioFrequencySlider;
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private Manager_Console ConsoleScript;

    [Header("Radio stations")]
    public AudioSource radio1;
    [SerializeField] private List<AudioClip> radio1Songs = new List<AudioClip>();
    public AudioSource radio2;
    [SerializeField] private List<AudioClip> radio2Songs = new List<AudioClip>();
    public AudioSource staticAudio;

    //public but hidden variables
    [HideInInspector] public bool radio1IsPlaying;
    [HideInInspector] public bool radio2IsPlaying;
    [HideInInspector] public List<string> radioStationNames = new List<string>();
    [HideInInspector] public List<float> radioStationFrequencies = new List<float>();

    //private variables
    private bool isRadioMuted;
    private int radio1Clip;
    private int radio2Clip;
    [Range(89.0f, 107.0f)]
    private float currentRadioFrequency = 89f;

    private void Start()
    {
        staticAudio.Play();

        //add radio 1
        radioStationNames.Add("radio1");
        radioStationFrequencies.Add(94.3f);
        radio1.volume = 0;
        PlayRadio1();

        //add radio 2
        radioStationNames.Add("radio2");
        radioStationFrequencies.Add(99.7f);
        radio2.volume = 0;
        PlayRadio2();

        MuteRadio();
    }

    public void UnmuteRadio()
    {
        isRadioMuted = false;
        btn_MuteRadio.interactable = true;
        btn_UnmuteRadio.interactable = false;

        GetRadioStation();

        //Debug.Log("Radio was unmuted!");
    }
    public void MuteRadio()
    {
        isRadioMuted = true;
        btn_MuteRadio.interactable = false;
        btn_UnmuteRadio.interactable = true;

        staticAudio.volume = 0;

        if (radio1IsPlaying)
        {
            radio1.volume = 0;
        }
        if (radio2IsPlaying)
        {
            radio2.volume = 0;
        }

        //Debug.Log("Radio was muted!");
    }

    //radio slider
    public void TuneRadio()
    {
        currentRadioFrequency = radioFrequencySlider.value;
        txt_radioFrequencyValue.text = (Mathf.Round(currentRadioFrequency * 100f) / 100f).ToString();
        GetRadioStation();
    }
    //looks for radios if slider was moved
    private void GetRadioStation()
    {
        if (!isRadioMuted)
        {
            //look for radio1
            if (currentRadioFrequency >= 93.6f
                && currentRadioFrequency < 95f)
            {
                //Debug.Log("In range to play radio 1!");
                radio1IsPlaying = true;
                TuneRadio1();
            }
            //look for radio2
            else if (currentRadioFrequency >= 99f
                     && currentRadioFrequency < 100.3f)
            {
                radio2IsPlaying = true;
                TuneRadio2();
            }
            //out of radio1 and radio2 range for full static sound and no radio volume
            else
            {
                //Debug.Log("Out of range!");
                radio1.volume = 0f;
                radio1IsPlaying = false;
                radio2.volume = 0f;
                radio2IsPlaying = false;
                staticAudio.volume = 1f;
            }
        }
    }

    //radio 1
    private void TuneRadio1()
    {
        //between 93.6f and 93.9f or between 94.7f and 95f for 1/3rds radio1 volume and 2/3rds static volume
        if ((currentRadioFrequency >= 93.6f
                 && currentRadioFrequency < 93.9f)
                 || (currentRadioFrequency >= 94.7f
                 && currentRadioFrequency < 95f))
        {
            radio1.volume = 0.3f;
            radio2.volume = 0f;
            staticAudio.volume = 0.6f;
            //Debug.Log("Tuned 33% to radio1!");
        }
        //between 93.9f and 94.1f or 94.5f and 94.7f for 2/3rds radio1 volume and 1/3rds static volume
        else if ((currentRadioFrequency >= 93.9f
                 && currentRadioFrequency < 94.1f)
                 || (currentRadioFrequency >= 94.5f
                 && currentRadioFrequency < 94.7f))
        {
            radio1.volume = 0.6f;
            radio2.volume = 0f;
            staticAudio.volume = 0.3f;
            //Debug.Log("Tuned 66% to radio1!");
        }
        //between 94.1f and 94.5f for full radio1 volume and no static
        else if (currentRadioFrequency >= 94.1f && currentRadioFrequency < 94.5f)
        {
            radio1.volume = 1f;
            radio2.volume = 0f;
            staticAudio.volume = 0f;
            //Debug.Log("Tuned 100% to radio1!");
        }
    }
    private void PlayRadio1()
    {
        //assigns the first song to the radio
        if (radio1.clip == null)
        {
            radio1Clip = Random.Range(0, radio1Songs.Count);
            AudioClip song = radio1Songs[radio1Clip];
            radio1.clip = song;
            radio1.Play();
        }
        //assigns the next song in list to the radio if the current one has finished playing
        else if (radio1.clip != null && !radio1.isPlaying)
        {
            radio1Clip = Random.Range(0, radio1Songs.Count);
            AudioClip song = radio1Songs[radio1Clip];
            radio1.clip = song;
            radio1.Play();
        }
    }

    //radio 2
    private void TuneRadio2()
    {
        //between 99f and 99.3f or between 100.1f and 100.4f for 1/3rds radio2 volume and 2/3rds static volume
        if ((currentRadioFrequency >= 99f
                 && currentRadioFrequency < 99.3f)
                 || (currentRadioFrequency >= 100.1f
                 && currentRadioFrequency < 100.4f))
        {
            radio1.volume = 0f;
            radio2.volume = 0.3f;
            staticAudio.volume = 0.6f;
            //Debug.Log("Tuned 33% to radio2!");
        }
        //between 99.3f and 99.5f or 99.9f and 100.1f for 2/3rds radio2 volume and 1/3rds static volume
        else if ((currentRadioFrequency >= 99.3f
                 && currentRadioFrequency < 99.5f)
                 || (currentRadioFrequency >= 99.9f
                 && currentRadioFrequency > 100.1f))
        {
            radio1.volume = 0f;
            radio2.volume = 0.6f;
            staticAudio.volume = 0.3f;
            //Debug.Log("Tuned 66% to radio2!");
        }
        //between 99.5f and 99.9f for full radio2 volume and no static
        else if (currentRadioFrequency >= 99.5f && currentRadioFrequency < 99.9f)
        {
            radio1.volume = 0f;
            radio2.volume = 1f;
            staticAudio.volume = 0f;
            //Debug.Log("Tuned 100% to radio2!");
        }
    }
    private void PlayRadio2()
    {
        //assigns the first song to the radio
        if (radio2.clip == null)
        {
            radio2Clip = Random.Range(0, radio2Songs.Count);
            AudioClip song = radio2Songs[radio2Clip];
            radio2.clip = song;
            radio2.Play();
        }
        //assigns the next song in list to the radio if the current one has finished playing
        else if (radio2.clip != null && !radio2.isPlaying)
        {
            radio2Clip = Random.Range(0, radio2Songs.Count);
            AudioClip song = radio2Songs[radio2Clip];
            radio2.clip = song;
            radio2.Play();
        }
    }
}