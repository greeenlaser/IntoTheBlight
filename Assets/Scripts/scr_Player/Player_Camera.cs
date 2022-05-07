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
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isCamEnabled;
    [HideInInspector] public bool isAimingDownSights;
    [HideInInspector] public float mouseSpeed = 40;
    [HideInInspector] public int fov = 90;

    //private variables
    private float sensX;
    private float sensY;
    private float mouseX;
    private float mouseY;
    private float xRot;

    private void Awake()
    {
        cam_Main.fieldOfView = fov;
        sensX = mouseSpeed;
        sensY = mouseSpeed;
        txt_mouseSpeed.text = mouseSpeed.ToString();

        StartCoroutine(Wait());
    }

    private void Update()
    {
        if (isCamEnabled 
            && !PlayerMovementScript.isStunned
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            if (!isAimingDownSights)
            {
                mouseX = Input.GetAxis("Mouse X") * sensX * 5 * Time.deltaTime;
                mouseY = Input.GetAxis("Mouse Y") * sensY * 5 * Time.deltaTime;

            }
            else if (isAimingDownSights)
            {
                mouseX = Input.GetAxis("Mouse X") * sensX * 2.5f * Time.deltaTime;
                mouseY = Input.GetAxis("Mouse Y") * sensY * 2.5f * Time.deltaTime;
            }

            xRot -= mouseY;

            xRot = Mathf.Clamp(xRot, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

            transform.parent.Rotate(Vector3.up * mouseX);
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