using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_RaycastSystem : MonoBehaviour
{
    [SerializeField] private Player_Movement PlayerMovementScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private Manager_UIReuse UIReuseScript;
    [SerializeField] private UI_PauseMenu PauseMenuScript;
    [SerializeField] private GameManager GameManagerScript;

    //public but hidden variables
    [HideInInspector] public GameObject heldObject;
    [HideInInspector] public bool isColliding;
    [HideInInspector] public List<GameObject> targets = new List<GameObject>();

    //private variables
    private bool canInteract;
    private float timer;
    private GameObject target;
    private GameObject deadAIContainer;
    private LayerMask IgnoredLayermask;

    private void Start()
    {
        IgnoredLayermask = LayerMask.NameToLayer("Player");
    }

    //checking what collides with the visionCone mesh
    private void OnTriggerEnter(Collider other)
    {
        if (!targets.Contains(other.gameObject)
            && (other.GetComponent<UI_AIContent>() != null
            || other.GetComponent<Env_Item>() != null))
        {
            targets.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (targets.Contains(other.gameObject))
        {
            targets.Remove(other.gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (heldObject == null)
        {
            if (!PauseMenuScript.isGamePaused
                && transform.parent.GetComponent<Player_Health>().isPlayerAlive
                && !PlayerMovementScript.isStunned)
            {
                //all other layers except this one
                IgnoredLayermask = ~IgnoredLayermask;

                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out RaycastHit hitTarget, 3, IgnoredLayermask, QueryTriggerInteraction.Ignore))
                {
                    //Debug.Log("Hit target is " + hitTarget.transform.name);

                    //if the target is AI and it doesn't have a health script
                    //or it has a health script and it is alive
                    if (hitTarget.transform.GetComponent<UI_AIContent>() != null
                        && hitTarget.transform.GetComponent<AI_Health>() == null
                        || (hitTarget.transform.GetComponent<AI_Health>() != null
                        && hitTarget.transform.GetComponent<AI_Health>().isAlive))
                    {
                        if (target != hitTarget.transform.gameObject)
                        {
                            target = hitTarget.transform.gameObject;
                            //Debug.Log(target.name);
                        }

                        UIReuseScript.txt_HoverItemCount.text = "";
                        UIReuseScript.bgr_HoverItemCountBackground.gameObject.SetActive(false);

                        timer = 0;
                        canInteract = true;
                    }
                    //if the target is an AI but it has a health script and it is dead
                    else if (hitTarget.transform.GetComponent<UI_AIContent>() != null
                             && hitTarget.transform.GetComponent<AI_Health>() != null
                             && !hitTarget.transform.GetComponent<AI_Health>().isAlive)
                    {
                        target = hitTarget.transform.gameObject;

                        //gets the parent of the target and looks for the correct child
                        //which has the dead AI loot script
                        GameObject par = target.transform.parent.gameObject;
                        foreach (Transform child in par.transform)
                        {
                            if (child.name == "par_DeadAILoot"
                                && child.GetComponent<Inv_Container>() != null)
                            {
                                deadAIContainer = child.transform.gameObject;
                                //Debug.Log(deadAIContainer.name);

                                UIReuseScript.txt_HoverItemCount.text = "";
                                UIReuseScript.bgr_HoverItemCountBackground.gameObject.SetActive(false);

                                timer = 0;
                                canInteract = true;
                            }
                        }
                    }

                    //if the target is an item
                    else if ((hitTarget.transform.GetComponent<Env_Item>() != null
                             && hitTarget.transform.GetComponent<Item_Grenade>() != null
                             && !hitTarget.transform.GetComponent<Item_Grenade>().isThrownGrenade)
                             || (hitTarget.transform.GetComponent<Env_Item>() != null
                             && hitTarget.transform.GetComponent<Item_Grenade>() == null))
                    {
                        target = hitTarget.transform.gameObject;

                        if (target.GetComponent<Env_Item>().int_itemCount > 1)
                        {
                            UIReuseScript.txt_HoverItemCount.text = "x" + target.GetComponent<Env_Item>().int_itemCount.ToString();
                            UIReuseScript.bgr_HoverItemCountBackground.gameObject.SetActive(true);
                        }
                        else
                        {
                            UIReuseScript.txt_HoverItemCount.text = "";
                            UIReuseScript.bgr_HoverItemCountBackground.gameObject.SetActive(false);
                        }

                        //Debug.Log(target.name);

                        timer = 0;
                        canInteract = true;
                    }

                    //if the target is a container
                    else if (hitTarget.transform.GetComponent<Inv_Container>() != null
                             && hitTarget.transform.GetComponent<Env_Follow>() == null)
                    {
                        if (target != hitTarget.transform.gameObject)
                        {
                            target = hitTarget.transform.gameObject;
                            //Debug.Log(target.name);
                        }

                        UIReuseScript.txt_HoverItemCount.text = "";
                        UIReuseScript.bgr_HoverItemCountBackground.gameObject.SetActive(false);

                        timer = 0;
                        canInteract = true;
                    }

                    //if the target is a workbench
                    else if (hitTarget.transform.GetComponent<Env_Workbench>() != null)
                    {
                        if (target != hitTarget.transform.gameObject)
                        {
                            target = hitTarget.transform.gameObject;
                            //Debug.Log(target.name);
                        }

                        UIReuseScript.txt_HoverItemCount.text = "";
                        UIReuseScript.bgr_HoverItemCountBackground.gameObject.SetActive(false);

                        timer = 0;
                        canInteract = true;
                    }

                    //if the target is a waitable
                    else if (hitTarget.transform.GetComponent<Env_Wait>() != null)
                    {
                        if (target != hitTarget.transform.gameObject)
                        {
                            target = hitTarget.transform.gameObject;
                            //Debug.Log(target.name);
                        }

                        UIReuseScript.txt_HoverItemCount.text = "";
                        UIReuseScript.bgr_HoverItemCountBackground.gameObject.SetActive(false);

                        timer = 0;
                        canInteract = true;
                    }

                    //special case where we need to get the locked doors trigger
                    //which the raycast is actually ignoring by default
                    //to allow interacting with other gameobjects
                    //if theyre inside a trigger we dont want to interact with
                    else if (hitTarget.transform.name == "door_scifi02"
                             || hitTarget.transform.name == "door_scifi03")
                    {
                        Transform doorParent = hitTarget.transform.parent.parent;
                        foreach (Transform child in doorParent)
                        {
                            if (child.GetComponent<Env_Door>() != null
                                && child.GetComponent<Env_Door>().isLocked)
                            {
                                if (target != child.gameObject)
                                {
                                    target = child.gameObject;
                                    //Debug.Log(target.name);
                                }

                                UIReuseScript.txt_HoverItemCount.text = "";
                                UIReuseScript.bgr_HoverItemCountBackground.gameObject.SetActive(false);

                                timer = 0;
                                canInteract = true;
                            }
                        }
                    }
                }
            }
        }

        //simple override to always show interact UI if player is currently holding an object
        //no matter if the raycast is hitting the held object or not
        else if (heldObject != null)
        {
            timer = 0;
            canInteract = true;
        }
    }

    private void Update()
    {
        if (canInteract)
        {
            timer += Time.deltaTime;
            if (timer > 0.05f)
            {
                canInteract = false;
            }

            if (!UIReuseScript.img_Interact.isActiveAndEnabled)
            {
                UIReuseScript.InteractUIEnabled();
            }

            if (!PauseMenuScript.isGamePaused
                && transform.parent.GetComponent<Player_Health>().isPlayerAlive)
            {
                //interacting with an object
                if (Input.GetKeyDown(KeyCode.E))
                {
                    //hit alive npc
                    if (target.GetComponent<UI_AIContent>() != null
                        && target.GetComponent<UI_AIContent>().AIActivated
                        && target.GetComponent<UI_AIContent>().hasDialogue)
                    {
                        target.GetComponent<UI_AIContent>().CheckIfAnyQuestIsCompleted();
                    }
                    //hit dead npc
                    else if (deadAIContainer != null
                             && deadAIContainer.GetComponent<Inv_Container>() != null
                             && deadAIContainer.GetComponent<Env_Follow>() != null
                             && deadAIContainer.GetComponent<Inv_Container>().containerActivated)
                    {
                        deadAIContainer.GetComponent<Inv_Container>().CheckIfLocked();
                    }
                    //hit item
                    else if (target.GetComponent<Env_Item>() != null
                             && target.GetComponent<Env_Item>().itemActivated)
                    {
                        target.GetComponent<Env_Item>().PickUp();
                    }
                    //hit container
                    else if (target.GetComponent<Inv_Container>() != null
                             && target.GetComponent<Env_Follow>() == null
                             && target.GetComponent<Inv_Container>().containerActivated)
                    {
                        target.GetComponent<Inv_Container>().CheckIfLocked();
                    }
                    //hit workbench
                    else if (target.GetComponent<Env_Workbench>() != null
                             && target.GetComponent<Env_Workbench>().isActive)
                    {
                        target.GetComponent<Env_Workbench>().OpenRepairUI();
                    }
                    //hit waitable
                    else if (target.GetComponent<Env_Wait>() != null
                             && target.GetComponent<Env_Wait>().isActivated)
                    {
                        target.GetComponent<Env_Wait>().OpenTimeSlider();
                    }
                    //hit door
                    else if (target.GetComponent<Env_Door>() != null
                             && target.GetComponent<Env_Door>().isActive
                             && target.GetComponent<Env_Door>().isLocked)
                    {
                        target.GetComponent<Env_Door>().CheckIfKeyIsNeeded();
                    }
                }

                //holding an item
                if (Input.GetKeyDown(KeyCode.Mouse0)
                    && target.GetComponent<Env_Item>() != null
                    && target.GetComponent<Env_Item>().itemActivated
                    && PlayerInventoryScript.equippedGun == null
                    && heldObject == null)
                {
                    target.GetComponent<Env_ObjectPickup>().isHolding = true;
                    target.GetComponent<Rigidbody>().useGravity = false;
                    heldObject = target;
                }
                //dropping the held item
                if ((Input.GetKeyUp(KeyCode.Mouse0)
                    && heldObject != null
                    && heldObject.GetComponent<Env_ObjectPickup>().isHolding)
                    || (heldObject != null
                    && !heldObject.GetComponent<Env_ObjectPickup>().isHolding))
                {
                    DropHeldObject();
                }
            }
        }
        else if (!canInteract && (timer > 0 || UIReuseScript.img_Interact.isActiveAndEnabled))
        {
            UIReuseScript.InteractUIDisabled();

            UIReuseScript.txt_HoverItemCount.text = "";
            UIReuseScript.bgr_HoverItemCountBackground.gameObject.SetActive(false);

            timer = 0;
        }
    }

    public void DropHeldObject()
    {
        heldObject.GetComponent<Env_ObjectPickup>().DropObject();
        heldObject = null;
    }
}