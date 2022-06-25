using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player_Camera : MonoBehaviour
{
    [Header("Assignables")]
    [SerializeField] private Camera cam_Main;
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool isCamEnabled;
    [HideInInspector] public bool isAimingDownSights;
    [HideInInspector] public float mouseSpeed = 50;
    [HideInInspector] public float sensX;
    [HideInInspector] public float sensY;

    //private variables
    private float mouseX;
    private float mouseY;
    private float xRot;

    private void Awake()
    {
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
                mouseX = Input.GetAxis("Mouse X") * sensX * 6 * Time.deltaTime;
                mouseY = Input.GetAxis("Mouse Y") * sensY * 6 * Time.deltaTime;

            }
            else if (isAimingDownSights)
            {
                mouseX = Input.GetAxis("Mouse X") * sensX * 3f * Time.deltaTime;
                mouseY = Input.GetAxis("Mouse Y") * sensY * 3f * Time.deltaTime;
            }

            xRot -= mouseY;

            xRot = Mathf.Clamp(xRot, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

            transform.parent.Rotate(Vector3.up * mouseX);
        }
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.2f);
        isCamEnabled = true;
    }
}