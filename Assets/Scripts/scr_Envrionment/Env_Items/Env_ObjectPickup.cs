using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Env_ObjectPickup : MonoBehaviour
{
    [Tooltip("Max distance from where the object can be picked up.")]
    [Range(1f, 5f)]
    public float maxDistance = 3f;
    [Tooltip("How strong is the throwing force?")]
    [Range(10f, 30f)]
    [SerializeField] private float throwForce = 15f;
    [Range(1f, 10f)]
    public float speedLimit = 5;

    [Header("Assignables")]
    [SerializeField] private GameObject thePlayer;
    [SerializeField] private Transform pos_HoldItem;
    [SerializeField] private Rigidbody RigidBody;
    [SerializeField] private Player_Movement playerMovementScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;

    //public but hidden variables
    [HideInInspector] public bool isHolding;

    //private variables
    private bool isColliding;
    private readonly bool canThrow = true;
    private Collider theCollider;

    private void FixedUpdate()
    {
        if (gameObject.GetComponent<Rigidbody>().velocity.magnitude > speedLimit)
        {
            gameObject.GetComponent<Rigidbody>().velocity = gameObject.GetComponent<Rigidbody>().velocity.normalized * speedLimit;
        }

        if (gameObject.GetComponent<Env_Item>().itemActivated)
        {
            if (isHolding)
            {
                PlayerInventoryScript.heldObject = gameObject;

                if (gameObject.GetComponent<Rigidbody>().useGravity)
                {
                    gameObject.GetComponent<Rigidbody>().useGravity = false;
                }
                if (gameObject.GetComponent<Env_Item>().droppedObject)
                {
                    gameObject.GetComponent<Env_Item>().droppedObject = false;
                    gameObject.GetComponent<Env_Item>().time = 0;
                }

                Vector3 targetPoint = pos_HoldItem.transform.position;
                targetPoint += pos_HoldItem.transform.forward;
                Vector3 force = targetPoint - gameObject.GetComponent<Rigidbody>().transform.position;
                gameObject.GetComponent<Rigidbody>().velocity = force.normalized * gameObject.GetComponent<Rigidbody>().velocity.magnitude;

                if (isColliding && theCollider != null)
                {
                    gameObject.GetComponent<Rigidbody>().freezeRotation = false;
                    //Debug.Log(name + " is colliding with " + theCollider.name + "!");
                }
                else if (!isColliding)
                {
                    gameObject.GetComponent<Rigidbody>().freezeRotation = true;
                }

                gameObject.GetComponent<Rigidbody>().AddForce(force * 5000);

                gameObject.GetComponent<Rigidbody>().velocity *= Mathf.Min(1.0f, force.magnitude / 2);

                //drops held object if player is too far from it
                if (Vector3.Distance(gameObject.transform.position, pos_HoldItem.transform.position) > 3)
                {
                    DropObject();
                    //Debug.Log("Dropped " + gameObject.GetComponent<Env_Item>().str_fakeName + " because player went too far from it!");
                }
                //throws held object if player presses right mouse button
                if (canThrow && Input.GetKeyDown(KeyCode.Mouse1))
                {
                    DropObject();
                    RigidBody.AddForce(200 * throwForce * pos_HoldItem.forward);
                }
            }
            else if (!isHolding)
            {
                RigidBody.useGravity = true;
                RigidBody.freezeRotation = false;
                PlayerInventoryScript.heldObject = null;
            }
        }
        else if (!gameObject.GetComponent<Env_Item>().itemActivated && isHolding)
        {
            DropObject();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isColliding)
        {
            theCollider = collision.collider;
            isColliding = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        theCollider = null;
    }

    public void DropObject()
    {
        RigidBody.useGravity = true;
        isColliding = false;
        isHolding = false;
        gameObject.GetComponent<Env_Item>().droppedObject = true;
        UIReuseScript.InteractUIDisabled();
    }
}