using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Main values")]
    [Tooltip("How fast does the player move when walking?")]
    [Range(3, 6)]
    [SerializeField] private float walkSpeed;
    [Tooltip("How fast does the player move when sprinting?")]
    public float sprintSpeed;
    [Tooltip("How fast does the player move when crouching?")]
    [Range(1.5f, 2.9f)]
    [SerializeField] private float crouchSpeed;
    [Tooltip("How much stamina can the player have?")]
    public float maxStamina;
    [Tooltip("How many points does stamina regenerate?")]
    public float staminaRecharge;
    [Tooltip("How long does the player need to wait until stamina can recharge per second?")]
    [Range(1, 5)]
    [SerializeField] private float staminaCooldownLimit;
    [Tooltip("The jump height.")]
    public float jumpHeight;
    [Tooltip("How low does the player go when crouching?")]
    [Range(0.5f, 1.5f)]
    [SerializeField] private float crouchHeight;
    [Tooltip("How high does the player camera go when not crouching?")]
    [SerializeField] private Vector3 cameraFullHeight;
    [Tooltip("How high does the player camera go when crouching?")]
    [SerializeField] private Vector3 cameraCrouchHeight;
    [Tooltip("How close does the player need to be to the ground to be considered grounded?")]
    [SerializeField] private float groundDistance;

    [Header("Assignables")]
    public CharacterController controller;
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private GameObject checkSphere;
    [SerializeField] private AudioSource ladderAudioSource;
    [SerializeField] private AudioClip[] ladderSFX;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Player_Health PlayerHealthScript;
    [SerializeField] private Inv_Player PlayerInventoryScript;
    [SerializeField] private GameObject par_Managers;

    //public but hidden variables
    [HideInInspector] public bool canMove;
    [HideInInspector] public bool isNoclipping;
    [HideInInspector] public bool isStunned;
    [HideInInspector] public bool canSprint;
    [HideInInspector] public bool isSprinting;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isClimbingLadder;
    [HideInInspector] public bool facingOtherLadderSide;
    [HideInInspector] public bool canJump;
    [HideInInspector] public bool isJumping;
    [HideInInspector] public bool canCrouch;
    [HideInInspector] public bool isCrouching;
    [HideInInspector] public float speedIncrease = 1;
    [HideInInspector] public float currentStamina;
    [HideInInspector] public float jumpBuff;
    [HideInInspector] public float sprintBuff;
    [HideInInspector] public float staminaRegenBuff;
    [HideInInspector] public float finalStaminaRegen;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public GameObject ladder;

    //private variables
    private bool startStaminaRechargeCooldown;
    private bool cooldownFinished;
    private bool isPlayingLadderSFX;
    private readonly float gravity = -9.81f;
    private float originalHeight;
    private float currentSpeed;
    private float nc_moveSpeed;
    private float staminaCooldown;
    private float minVelocity;
    private int alphaValue;

    private float closestDistance;
    private string cellName;

    private Manager_UIReuse UIReuseManager;

    private void Awake()
    {
        UIReuseManager = par_Managers.GetComponent<Manager_UIReuse>();
    }

    //load player data at the beginning of the game
    public void LoadPlayer()
    {
        currentSpeed = walkSpeed;
        currentStamina = maxStamina;
        canMove = true;
        canSprint = true;
        canCrouch = true;
        canJump = true;

        originalHeight = controller.height;
        PlayerCamera.transform.localPosition = cameraFullHeight;

        alphaValue = 255;

        UIReuseManager.stamina = currentStamina;
        UIReuseManager.maxStamina = maxStamina;
        UIReuseManager.UpdatePlayerStamina();

        nc_moveSpeed = walkSpeed * 2.5f;
    }

    private void Update()
    {
        if (UIReuseManager.stamina != currentStamina)
        {
            UIReuseManager.stamina = currentStamina;
            UIReuseManager.UpdatePlayerStamina();
        }
        if (UIReuseManager.maxStamina != maxStamina)
        {
            UIReuseManager.maxStamina = maxStamina;
            UIReuseManager.UpdatePlayerStamina();
        }

        if (canMove
            && !par_Managers.GetComponent<Manager_GameSaving>().isLoading)
        {
            if (!isNoclipping)
            {
                //check if player is grounded
                if (Physics.CheckSphere(checkSphere.transform.position, 
                                        groundDistance, 
                                        groundMask))
                {
                    isGrounded = true;
                }
                else
                {
                    isGrounded = false;
                }

                if (!isClimbingLadder)
                {
                    PlayerRegularMovement();
                }
                else if (isClimbingLadder)
                {
                    PlayerLadderMovement();
                }
            }
            else if (isNoclipping)
            {
                PlayerNoclipMovement();
            }
        }
    }

    private void PlayerRegularMovement()
    {
        //gravity if player is grounded
        if (velocity.y < 0 
            && isGrounded)
        {
            //get smallest velocity
            if (velocity.y < minVelocity)
            {
                minVelocity = velocity.y;
            }
            //check if smallest velocity is less than or equal to -25f
            if (minVelocity <= -25f
                && PlayerHealthScript.canTakeDamage)
            {
                ApplyFallDamage();

                //Debug.Log(minVelocity);

                minVelocity = -2f;
            }

            velocity.y = -2f;
        }

        //gravity if player isnt grounded
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime * 4f;
        }

        //movement input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1);

        //first movement update based on speed and input
        controller.Move(currentSpeed * speedIncrease * Time.deltaTime * move);

        //final movement update based on velocity
        controller.Move(velocity * Time.deltaTime);

        //get all velocity of the controller
        Vector3 horizontalVelocity = transform.right * x + transform.forward * z;

        //sprinting
        if (Input.GetKeyDown(KeyCode.LeftShift) 
            && canSprint 
            && currentStamina >= 0.1f)
        {
            staminaCooldown = 0;
            isSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
        }
        if (isSprinting 
            && horizontalVelocity.magnitude > 0.3f)
        {
            //Debug.Log("Player is sprinting!");

            currentSpeed = sprintSpeed + sprintBuff;
            currentStamina -= 8 * Time.deltaTime;

            StaminaCooldownFinished();

            if (isCrouching)
            {
                isCrouching = false;

                controller.height = originalHeight;

                PlayerCamera.transform.localPosition = cameraFullHeight;
            }

            if (currentStamina <= 0.1f)
            {
                isSprinting = false;
            }
        }
        //force-disables sprinting if the player is no longer moving but still holding down sprint key
        else if (isSprinting
                 && horizontalVelocity.magnitude < 0.3f)
        {
            isSprinting = false;
        }
        else if (!isSprinting)
        {
            if (!isCrouching)
            {
                currentSpeed = walkSpeed;
            }

            //recharge stamina
            if (currentStamina < maxStamina)
            {
                startStaminaRechargeCooldown = true;
            }
            if (startStaminaRechargeCooldown)
            {
                if (!cooldownFinished)
                {
                    //Debug.Log("Starting stamina cooldown counter...");

                    staminaCooldown += Time.deltaTime;

                    if (staminaCooldown >= staminaCooldownLimit)
                    {
                        cooldownFinished = true;
                    }
                }
                else if (cooldownFinished)
                {
                    //Debug.Log("Stamina is recharging!");

                    staminaCooldown = 0;

                    currentStamina += (staminaRecharge + staminaRegenBuff) * Time.deltaTime;
                }
            }
            //fix stamina if it goes over the limit
            if (currentStamina > maxStamina)
            {
                //Debug.Log("Stamina is fully recharged!");

                currentStamina = maxStamina;

                StaminaCooldownFinished();
            }
        }

        //jumping
        if (Input.GetKey(KeyCode.Space) 
            && isGrounded 
            && !isJumping 
            && canJump 
            && currentStamina >= 5)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -5.2f * gravity + (jumpBuff * 100));
            controller.stepOffset = 0;
            currentStamina -= 5;
            isJumping = true;
        }
        else if (isGrounded 
                 && isJumping)
        {
            controller.stepOffset = 0.3f;
            isJumping = false;
        }

        //crouching
        if (Input.GetKeyDown(KeyCode.LeftControl) 
            && isGrounded 
            && canCrouch)
        {
            isCrouching = !isCrouching;

            if (isSprinting)
            {
                isSprinting = false;
            }

            if (isCrouching)
            {
                //Debug.Log("Player is crouching!");

                currentSpeed = crouchSpeed;

                controller.height = crouchHeight;

                PlayerCamera.transform.localPosition = cameraCrouchHeight;

                if (currentStamina < maxStamina)
                {
                    currentStamina += staminaRecharge * Time.deltaTime;
                    //Debug.Log("Stamina is recharging!");
                }

                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }
            }
            else if (!isCrouching)
            {
                //Debug.Log("Player is no longer crouching...");

                currentSpeed = walkSpeed;

                controller.height = originalHeight;

                PlayerCamera.transform.localPosition = cameraFullHeight;
            }
        }
    }

    private void PlayerLadderMovement()
    {
        if (!facingOtherLadderSide)
        {
            float y = Input.GetAxis("Vertical");

            Vector3 move = new(0, y, 0);
            move = Vector3.ClampMagnitude(move, 1);

            controller.Move(currentSpeed * speedIncrease * Time.deltaTime * move);
        }
        else if (facingOtherLadderSide)
        {
            float y = Input.GetAxis("VerticalFlipped");

            Vector3 move = new(0, y, 0);
            move = Vector3.ClampMagnitude(move, 1);

            controller.Move(currentSpeed * speedIncrease * Time.deltaTime * move);
        }

        if (!isGrounded)
        {
            //pressing either W or S while the other isnt being pressed and while player is not grounded
            if ((Input.GetKey(KeyCode.W) 
                && !Input.GetKey(KeyCode.S)) 
                || (!Input.GetKey(KeyCode.W) 
                && Input.GetKey(KeyCode.S)))
            {
                //ladder SFX is played while not climbing ladder and while not playing any SFX
                if (!isPlayingLadderSFX 
                    && !ladderAudioSource.isPlaying)
                {
                    PlayLadderSFX();
                }
                //another ladder SFX is played while already climbing ladder and while not playing any SFX
                else if (isPlayingLadderSFX 
                         && !ladderAudioSource.isPlaying)
                {
                    PlayOtherLadderSFX();
                }
            }
            //if player is no longer pressing W and S
            //then ladder SFX is stopped if ladder SFX is currently playing
            else if (!Input.GetKey(KeyCode.W) 
                     && !Input.GetKey(KeyCode.S) 
                     && isPlayingLadderSFX)
            {
                ladderAudioSource.Stop();
                isPlayingLadderSFX = false;
            }

            //if player presses space while not grounded
            //then player has basic jump
            else if (Input.GetKeyDown(KeyCode.Space) 
                && currentStamina > 5)
            {
                //jump up 0.5 meters
                transform.position = new Vector3(transform.position.x, 
                                                 transform.position.y + 0.5f, 
                                                 transform.position.z);
                currentStamina -= 5;
            }
            //if player is not grounded but presses A or D
            //then the ladder SFX will stop and player is no longer climbing ladder
            else if (Input.GetKeyDown(KeyCode.A) 
                     || Input.GetKeyDown(KeyCode.D))
            {
                if (isPlayingLadderSFX)
                {
                    ladderAudioSource.Stop();
                    isPlayingLadderSFX = false;
                }
                isClimbingLadder = false;
                //Debug.Log("Stopped climbing ladder.");
            }
        }
        //if player is grounded and presses space/A/D
        //then the ladder SFX will stop and player is no longer climbing ladder
        else if (isGrounded
                 && !facingOtherLadderSide
                 && (Input.GetKey(KeyCode.Space)
                 || Input.GetKey(KeyCode.A)
                 || Input.GetKey(KeyCode.S)
                 || Input.GetKey(KeyCode.D)))
        {
            if (isPlayingLadderSFX)
            {
                ladderAudioSource.Stop();
                isPlayingLadderSFX = false;
            }
            isClimbingLadder = false;
            //Debug.Log("Stopped climbing ladder.");
        }
        else if (isGrounded
                 && facingOtherLadderSide
                 && (Input.GetKey(KeyCode.Space)
                 || Input.GetKey(KeyCode.A)
                 || Input.GetKey(KeyCode.W)
                 || Input.GetKey(KeyCode.D)))
        {
            if (isPlayingLadderSFX)
            {
                ladderAudioSource.Stop();
                isPlayingLadderSFX = false;
            }
            isClimbingLadder = false;
            //Debug.Log("Stopped climbing ladder.");
        }
    }

    private void PlayerNoclipMovement()
    {
        //noclip movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + PlayerCamera.gameObject.transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1);

        transform.position += nc_moveSpeed * Time.deltaTime * move;

        //start fast move
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            nc_moveSpeed = walkSpeed * 10;
        }
        //stop fast move
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            nc_moveSpeed = walkSpeed * 2.5f;
        }

        //move down
        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position += nc_moveSpeed * Time.deltaTime * new Vector3(0, -1, 0);
        }
        //move up
        else if (Input.GetKey(KeyCode.Space))
        {
            transform.position += nc_moveSpeed * Time.deltaTime * new Vector3(0, 1, 0);
        }
    }

    //deal damage based off of velocity when hitting ground
    private void ApplyFallDamage()
    {
        float damageDealt = Mathf.Round(Mathf.Abs(velocity.y * 1.2f) * 10) / 10;

        PlayerHealthScript.DealDamage("Ground", "gravity", damageDealt);
    }

    //player stun effect, all stun effects last the same amount
    //player can only be stunned once at a time - cannot stun again
    //while one stun effect is already in effect
    public void Stun()
    {
        if (!isStunned)
        {
            gameObject.GetComponent<CharacterController>().Move(new Vector3(0, 0, 0));

            UIReuseManager.bgr_PlayerStun.color = new Color32(255, 255, 255, 255);
            UIReuseManager.bgr_PlayerStun.transform.localPosition = new Vector3(0, 0, 0);

            StartCoroutine(Stunned());
        }
    }
    private IEnumerator Stunned()
    {
        isStunned = true;

        while (alphaValue > 0)
        {
            alphaValue -= 1;
            UIReuseManager.bgr_PlayerStun.color = new Color32(255, 255, 255, (byte)alphaValue);
            //Debug.Log(alphaValue);
            yield return new WaitForSeconds(0.01f);
        }

        UIReuseManager.bgr_PlayerStun.color = new Color32(255, 255, 255, 0);
        UIReuseManager.bgr_PlayerStun.transform.localPosition = new Vector3(0, -1200, 0);

        alphaValue = 255;

        isStunned = false;
    }

    private void StaminaCooldownFinished()
    {
        startStaminaRechargeCooldown = false;
        cooldownFinished = false;
        staminaCooldown = 0;
    }

    private void PlayLadderSFX()
    {
        isPlayingLadderSFX = true;
        int randomSFX = Random.Range(0, ladderSFX.Length);
        ladderAudioSource.clip = ladderSFX[randomSFX];
        ladderAudioSource.Play();
        //Debug.Log("Playing first ladder SFX.");
    }
    private void PlayOtherLadderSFX()
    {
        int randomSFX = Random.Range(0, ladderSFX.Length);
        ladderAudioSource.clip = ladderSFX[randomSFX];
        ladderAudioSource.Play();
        //Debug.Log("Playing another ladder SFX.");
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //only push non-kinematic rigidbodies
        //if theyre not below us
        if (hit.gameObject.GetComponent<Rigidbody>() != null
            && !hit.gameObject.GetComponent<Rigidbody>().isKinematic
            && hit.moveDirection.y > -0.3f)
        {
            //get the target rigidbody
            Rigidbody targetRigidbody = hit.collider.attachedRigidbody;

            //push the object to the side, never up or down
            Vector3 pushDir = new(hit.moveDirection.x, 0, hit.moveDirection.z);
            //apply push strength to the object
            targetRigidbody.velocity = pushDir * currentSpeed / 3;
        }

        //if player collided with world border
        //then look for closest discovered cell spawn point
        //and teleport player there
        else if (hit.gameObject.CompareTag("WorldBlocker"))
        {
            foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
            {
                if (cell.GetComponent<Manager_CurrentCell>().discoveredCell)
                {
                    float distance = Vector3.Distance(gameObject.transform.position, cell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position);

                    if (cell == par_Managers.GetComponent<Manager_Console>().allCells[0])
                    {
                        closestDistance = distance;
                    }
                    else
                    {
                        if (distance < closestDistance)
                        {
                            cellName = cell.GetComponent<Manager_CurrentCell>().str_CellName;
                        }
                    }
                }
            }

            foreach (GameObject cell in par_Managers.GetComponent<Manager_Console>().allCells)
            {
                if (cell.GetComponent<Manager_CurrentCell>().discoveredCell
                    && cell.GetComponent<Manager_CurrentCell>().str_CellName
                    == cellName)
                {
                    gameObject.transform.position = cell.GetComponent<Manager_CurrentCell>().currentCellSpawnpoint.position + new Vector3(0, 0.2f, 0);
                    break;
                }
            }
        }
    }
}