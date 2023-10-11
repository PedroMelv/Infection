using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;

public enum MoveStates
{
    IDLE,
    WALKING,
    RUNNING,
    JUMPING,
    CRAWLING,
    DUCT
}
public class PlayerMovement : MovementBase
{
    [SerializeField] private Transform feetPos;
    [SerializeField] private Transform orientation;

    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private MoveStates curMoveState;
    public MoveStates GetMoveState { get { return curMoveState; } }

    public bool isHidden;

    [Header("Movement")]
    private float speed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float groundDrag;
    [SerializeField] private float tiredMoveMultiplier;
    [SerializeField] private float timeToRegenStamina = 2f;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegen = 1f;
    private float curStamina;
    private float staminaCooldown;
    private Volume staminaVolume;

    private bool tired;


    private Vector3 moveDirection;

    [Header("Crawl")]
    [SerializeField] private float crawlMultiplier;
    [SerializeField] private float normalHeight;
    [SerializeField] private float crawlHeight;
    [SerializeField] private float ductHeight;

    [SerializeField] private LayerMask crouchCeilDetect;
    private bool isCrawling;

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField,Range(0f,1f)] private float jumpCutMultiplier;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask groundLayer;
    private bool grounded;
    public bool IsGrounded { get { return grounded; } }

    [Header("LadderClimb")]
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private float climbSpeed;
    private bool climbingLadder;

    private Vector3 ladderDirection;

    private RaycastHit ladderHit;
    private GameObject currentLadder;

    [Header("Duct")]
    [SerializeField] private Vector3 detectionSize;
    private bool inDuct;
    private float ductLeaveTimer;

    [Header("Slope")]
    [SerializeField, Range(0f, 90f)] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    private PlayerInput pInput;
    private StepSoundEffect stepSoundEffect;

    public override void Awake()
    {
        base.Awake();        
        pInput = GetComponent<PlayerInput>();
        stepSoundEffect = GetComponent<StepSoundEffect>();
    }

    private void Start()
    {
        GameObject sVolumeObj = GameObject.FindGameObjectWithTag("StaminaVolume");
        if(sVolumeObj != null)
            staminaVolume = sVolumeObj.GetComponent<Volume>();

        staminaCooldown = timeToRegenStamina;
        curStamina = maxStamina;
    }

    private void Update()
    {
        if(photonView.IsMine == false) return;

        isHidden = (grounded && isCrawling && !CanGoUp());

        ductLeaveTimer += Time.deltaTime;
        HandleStamina();
        HandleJump();
        HandleCrawl();
        HandleGroundDrag();
        HandleSpeedLimit();
        HandleLadderDetection();
    }

    private void FixedUpdate()
    {
        if(photonView.IsMine == false) 
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            return;
        }
        HandleMovement();
    }

    #region Movement
    private void HandleMovement()
    {

        if (canMove == false)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            stepSoundEffect.playStepTimer = false;
            return;
        }
        rb.useGravity = (!OnSlope());

        if (climbingLadder)
        {
            rb.useGravity = false;

            Vector3 DirectionPartOne = Vector3.up * pInput.move_y_input * climbSpeed;
            Vector3 DirectionPartTwo = orientation.right * pInput.move_x_input * moveSpeed;

            if(Physics.BoxCast(transform.position, Vector3.one, Vector3.up, Quaternion.identity, 1f, playerLayer) && pInput.move_y_input > 0f)
            {
                DirectionPartOne = Vector3.zero;
            }

            if (Physics.BoxCast(transform.position, Vector3.one / 2f, Vector3.down, Quaternion.identity, 1f, playerLayer) && pInput.move_y_input < 0f)
            {
                DirectionPartOne = Vector3.zero;
            }

            moveDirection = DirectionPartOne + DirectionPartTwo ;

            rb.velocity = moveDirection;

            return;
        }

        moveDirection = orientation.forward * pInput.move_y_input + orientation.right * pInput.move_x_input;

        stepSoundEffect.playStepTimer = (moveDirection != Vector3.zero);

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeDirection() * speed * 20f * rb.mass, ForceMode.Force); //Move to the direction of the slope

            if (rb.velocity.y > 0) //Keep it stuck on the slope if it's going up
            {
                Debug.Log("Applying! " + rb.velocity.y);
                rb.AddForce(Vector3.down * 30f, ForceMode.Force);
            }
            return;
        }

        rb.AddForce(moveDirection.normalized * speed * 10f * rb.mass, ForceMode.Force);
    }

    private void HandleSpeedLimit()
    {
        //Change Speed

        if(pInput.sprintInput && grounded)
        {
            speed = moveSpeed * sprintMultiplier;
            stepSoundEffect.playSpeed = 2f;
            curMoveState = MoveStates.RUNNING;
        }
        else
        {
            speed = moveSpeed;
            stepSoundEffect.playSpeed = 1f;
            curMoveState = MoveStates.WALKING;
        }

        if(tired)
        {
            speed = moveSpeed * tiredMoveMultiplier;
            stepSoundEffect.playSpeed = .75f;
            curMoveState = MoveStates.WALKING;
        }

        if((isCrawling && grounded)|| inDuct) 
        {
            speed = moveSpeed * crawlMultiplier;
            stepSoundEffect.playSpeed = .5f;
            curMoveState = MoveStates.CRAWLING;
        }



        //Limit Speed
        Vector3 flatVel = Vector3.zero;

        if (OnSlope() && !exitingSlope)
        {
            flatVel = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
            if(flatVel.magnitude > speed)
            {
                Vector3 limitedVel = flatVel.normalized * speed;
                rb.velocity = new Vector3(limitedVel.x, limitedVel.y, limitedVel.z);
            }

            return;
        }

        flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void HandleGroundDrag()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        float finalDrag = 0f;
        if (moveDirection == Vector3.zero && grounded)
        {
            finalDrag = groundDrag;
        }

        if(moveDirection == Vector3.zero && OnSlope() && !exitingSlope)
        {
            finalDrag = 100;
        }

        rb.drag = finalDrag;
    }

    private void HandleStamina()
    {
        staminaVolume.weight = 1 - Mathf.Clamp01(((curStamina  / maxStamina) - .15f));

        if (curStamina < 0f)
        {
            curStamina = 0f;
            tired = true;
        }

        if(curStamina > maxStamina)
        {
            curStamina = maxStamina;
            tired = false;
        }

        if(pInput.sprintInput && moveDirection != Vector3.zero && !tired)
        {
            curStamina -= 15f * Time.deltaTime;
        }

        if(pInput.sprintInput == false && grounded || tired)
        {
            if(staminaCooldown <= 0f)
            {
                curStamina += staminaRegen * (tired ? 3f : 1f ) * Time.deltaTime;
            }
            else
            {
                staminaCooldown -= Time.deltaTime;
            }
        }
        else
        {
            staminaCooldown = timeToRegenStamina;
        }
    }
    #endregion
    #region Crawl
    public void HandleCrawl()
    {
        if(inDuct)
        {
            Vector3 playerSizeDuct = Vector3.one;
            playerSizeDuct.y = ductHeight;

            transform.localScale = playerSizeDuct;

            if(CanGoUp() && ductLeaveTimer > 1f)
            {
                Debug.Log("Lifting");
                
                inDuct = false;
                isCrawling = false;

                playerSizeDuct.y = normalHeight;
                transform.localScale = playerSizeDuct;
                           
            }
            return;
        }

        #region Start Crawling Input
        if (grounded && pInput.crawlInputPressed && !isCrawling && !climbingLadder)
        {
            isCrawling = true;
            if(!inDuct)rb.AddForce(Vector3.down * 100f, ForceMode.Impulse);
        }
        #endregion

        #region Handle Crawling

        Vector3 playerSize = Vector3.one;
        playerSize.y = normalHeight;

        if(!climbingLadder)
        {
            if(isCrawling && pInput.crawlInput)
            {
                playerSize.y = crawlHeight;
            }else if(!pInput.crawlInput && pInput.crawlInputReleased && CanGoUp())
            {
                rb.AddForce(Vector3.up * 4f, ForceMode.Impulse);
                isCrawling = false;
            }
        }
        else
        {
            playerSize.y = normalHeight;
            isCrawling = false;
        }

        if (isCrawling && CanGoUp() == false) playerSize.y = crawlHeight;

        if (playerSize.y == normalHeight) isCrawling = false;

        transform.localScale = playerSize;
        #endregion
    }

    private bool CanGoUp()
    {
        
        Collider[] cols = Physics.OverlapBox(transform.position + (Vector3.up * detectionSize.y / 2f) , detectionSize / 2f, Quaternion.identity, crouchCeilDetect);
        //Debug.Log(cols.Length);

        for (int i = 0; i < cols.Length; i++)
        {
            //Debug.Log(cols[i].gameObject.name);
        }

        return cols.Length <= 0;
    }
    #endregion
    #region Duct
    public void InteractDuct(DuctTrigger duct)
    {
        if(inDuct) return;

        if(inDuct)
        {
            inDuct = false;
            transform.position = duct.GetOutDuct();
        }else{
            inDuct = true;
            isCrawling = true;
            ductLeaveTimer = 0f;
            transform.position = duct.GetInDuct();
        }
    }
    #endregion
    #region Jump

    private void HandleJump()
    {
        /*
        if(pInput.jumpInputPressed && pInput.jumpInput && grounded && tired == false && curStamina > 10f)
        {
            exitingSlope = true;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            curStamina -= 10f;
            staminaCooldown = timeToRegenStamina;

            Invoke(nameof(ResetJump), 1f);
        }
        
        if(pInput.jumpInputReleased && !pInput.jumpInput && !grounded)
        {
            
            Debug.Log("Jump Release");
            rb.AddForce(Vector3.down * rb.velocity.y * jumpCutMultiplier, ForceMode.Impulse);
        }
        */
    }

    private void ResetJump()
    {
        exitingSlope = false;
    }

    #endregion
    #region Ladder Things

    private void HandleLadderDetection()
    {
        if(climbingLadder)
        {
            if (Physics.Raycast(feetPos.position + Vector3.up * 0.1f, ladderDirection, out ladderHit, .5f, ladderLayer))
            {
                Debug.Log("Detecting Ladder");
                if (grounded && pInput.move_y_input < 0f) DropLadder();
            }
            else
            {
                Debug.Log("Not Detecting Ladder");
                DropLadder();
                rb.AddForce(ladderDirection + Vector3.up * 1.5f, ForceMode.Impulse);
            }

            if(Input.GetKeyDown(KeyCode.Space))
            {
                DropLadder();
                rb.AddForce((orientation.forward * moveSpeed) + Vector3.up * 1.5f, ForceMode.Impulse);
            }
        }
        else
        {
            if(Physics.Raycast(feetPos.position + Vector3.up * 0.1f, orientation.forward, out ladderHit, .5f, ladderLayer))
            {
                if (ladderHit.collider != null && pInput.move_y_input > 0f)
                {
                    ClimbLadder(orientation.forward, ladderHit.collider.gameObject);
                }
            }
        }
    }

    private void ClimbLadder(Vector3 ladderDirection, GameObject ladderObj)
    {
        Debug.Log("Grabbed ladder");
        this.ladderDirection = ladderDirection;
        currentLadder = ladderObj;
        climbingLadder = true;
    }

    private void DropLadder()
    {
        Debug.Log("Released ladder");
        climbingLadder = false;
        currentLadder = null;
    }

    public bool IsOnLadder()
    {
        return climbingLadder;
    }

    #endregion
    #region Slope Things

    public bool OnSlope()
    {
        //if (exitingSlope) return false;

        if(Physics.Raycast(transform.position , Vector3.down, out slopeHit, playerHeight * .5f + 0.3f, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            //Debug.Log(angle);
            return (angle < maxSlopeAngle && angle != 0f);
        }
        return false;
    }

    public Vector3 GetSlopeDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube((feetPos.position + (Vector3.up / 2f) * detectionSize.y), detectionSize);
    }
}
