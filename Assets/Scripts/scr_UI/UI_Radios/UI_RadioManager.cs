using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_RadioManager : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Button btn_UnmuteRadio;
    [SerializeField] private Button btn_MuteRadio;
    [SerializeField] private TMP_Text txt_radioFrequencyValue;
    public TMP_Text txt_radioConnection;
    [SerializeField] private Slider radioFrequencySlider;
    public AudioSource staticAudio;
    public List<GameObject> radioStations = new List<GameObject>();

    //public but hidden variables
    [HideInInspector] public float currentRadioFrequency = 89f;
    [HideInInspector] public List<string> radioStationNames = new List<string>();
    [HideInInspector] public List<float> radioStationFrequencies = new List<float>();

    //private variables
    private bool isRadioMuted;

    private void Start()
    {
        staticAudio.Play();

        foreach (GameObject radio in radioStations)
        {
            radio.GetComponent<UI_RadioStation>().PlayRadio();
        }

        txt_radioConnection.text = "Connection: Not connected";
        txt_radioConnection.color = new Color32(0, 0, 0, 255);

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

        foreach (GameObject radio in radioStations)
        {
            if (radio.GetComponent<UI_RadioStation>().isPlaying)
            {
                radio.GetComponent<UI_RadioStation>().RadioSource.volume = 0;
            }
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
            foreach (GameObject radio in radioStations)
            {
                float radioFrequency = radio.GetComponent<UI_RadioStation>().radioFrequency;

                //tuning to a radiostation
                if (currentRadioFrequency >= radioFrequency - 0.9f
                    && currentRadioFrequency < radioFrequency + 0.9f)
                {
                    radio.GetComponent<UI_RadioStation>().TuneRadio();
                    break;
                }
                //no sound for any radios, full static volume
                else
                {
                    //Debug.Log("Out of range!");

                    foreach (GameObject radioStation in radioStations)
                    {
                        radioStation.GetComponent<UI_RadioStation>().RadioSource.volume = 0;
                    }

                    txt_radioConnection.text = "Connection: Not connected";
                    txt_radioConnection.color = new Color32(0, 0, 0, 255);

                    staticAudio.volume = 1f;
                }
            }
        }
    }
}