using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_NoclipCamera : MonoBehaviour
{
    public float flySpeed;
    [SerializeField] private Player_Camera PlayerCameraScript;
    [SerializeField] private GameObject par_PlayerNoclip;

    //public but hidden variables
    [HideInInspector] public float mouseSpeed;
    [HideInInspector] public int fov;

    //private variables
    private float sensX;
    private float sensY;
    private float xRot;
    private float moveSpeed;
    private float sprintSpeed;

    private void Awake()
    {
        mouseSpeed = PlayerCameraScript.mouseSpeed;
        sensX = mouseSpeed;
        sensY = mouseSpeed;

        moveSpeed = flySpeed;
        sprintSpeed = flySpeed * 2;
    }

    private void Update()
    {
        //camera movement
        float mouseX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime * 10;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime * 10;

        xRot -= mouseY;

        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        par_PlayerNoclip.transform.Rotate(Vector3.up * mouseX);

        //noclip movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1);

        par_PlayerNoclip.transform.position += moveSpeed * Time.deltaTime * move;

        if (Input.GetKey(KeyCode.Q))
        {
            par_PlayerNoclip.transform.position -= moveSpeed * Time.deltaTime * Vector3.up;
        }
        if (Input.GetKey(KeyCode.E))
        {
            par_PlayerNoclip.transform.position += moveSpeed * Time.deltaTime * Vector3.up;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            moveSpeed = sprintSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed = flySpeed;
        }
    }

    public void SetMouseSpeed()
    {
        mouseSpeed = PlayerCameraScript.mouseSpeed;
    }
    public void SetFOV()
    {
        fov = PlayerCameraScript.fov;
    }
}