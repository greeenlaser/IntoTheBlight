using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player_Camera : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Camera cam_Main;
    [SerializeField] private TMP_Text txt_mouseSpeed;
    public TMP_Text txt_fov;
    [SerializeField] private Slider MouseSpeedSlider;
    public Slider FOVSlider;
    [SerializeField] private Player_Movement PlayerMovementScript;

    //public but hidden variables
    [HideInInspector] public bool isCamEnabled;
    [HideInInspector] public bool isAimingDownSights;
    [HideInInspector] public float mouseSpeed = 40;
    [HideInInspector] public int fov = 90;

    //private variables
    private float sensX;
    private float sensY;
    private float xRot;
    private GameObject ThePlayer;

    private void Awake()
    {
        cam_Main.fieldOfView = fov;
        sensX = mouseSpeed;
        sensY = mouseSpeed;
        StartCoroutine(Wait());
    }

    private void Start()
    {
        ThePlayer = GameObject.Find("Player");
        txt_mouseSpeed.text = mouseSpeed.ToString();
    }

    private void Update()
    {
        if (isCamEnabled 
            && !PlayerMovementScript.isStunned)
        {
            if (!isAimingDownSights)
            {
                float mouseX = Input.GetAxis("Mouse X") * sensX * 5 * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * sensY * 5 * Time.deltaTime;

                xRot -= mouseY;

                xRot = Mathf.Clamp(xRot, -90f, 90f);
                transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

                ThePlayer.transform.Rotate(Vector3.up * mouseX);
            }
            else if (isAimingDownSights)
            {
                float mouseX = Input.GetAxis("Mouse X") * sensX * 2.5f * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * sensY * 2.5f * Time.deltaTime;

                xRot -= mouseY;

                xRot = Mathf.Clamp(xRot, -90f, 90f);
                transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

                ThePlayer.transform.Rotate(Vector3.up * mouseX);
            }
        }    
    }

    public void SetMouseSpeed()
    {
        mouseSpeed = MouseSpeedSlider.value;

        sensX = MouseSpeedSlider.value;
        sensY = MouseSpeedSlider.value;

        txt_mouseSpeed.text = MouseSpeedSlider.value.ToString();
    }

    public void SetFOV()
    {
        fov = Mathf.FloorToInt(FOVSlider.value);
        txt_fov.text = FOVSlider.value.ToString();

        cam_Main.fieldOfView = fov;
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.2f);
        isCamEnabled = true;
    }
}