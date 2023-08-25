using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public enum MoveStates
{
    IDLE,
    WALKING,
    RUNNING,
    JUMPING,
    CRAWLING,
    DUCT
}
public class PlayerMovement : MonoBehaviourPun
{
    [SerializeField] private Transform feetPos;
    [SerializeField] private Transform orientation;

    [SerializeField] private LayerMask playerLayer;

    [SerializeField] private MoveStates curMoveState;
    public MoveStates GetMoveState { get { return curMoveState; } }

    public bool canMove = true;

    [Header("Movement")]
    private float speed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float groundDrag;

    private Vector3 moveDirection;

    [Header("Crawl")]
    [SerializeField] private float crawlMultiplier;
    [SerializeField] private float normalHeight;
    [SerializeField] private float crawlHeight;
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
    

    [Header("Slope")]
    [SerializeField, Range(0f, 90f)] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    private Rigidbody rb;
    private Collider[] cols;
    private PlayerInput pInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cols = GetComponents<Collider>();
        pInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if(photonView.IsMine == false) return;
        
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

    public void SetCollisions(bool to)
    {
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].isTrigger = to;
        }
    }

    #region Movement
    private void HandleMovement()
    {
        rb.useGravity = (!OnSlope());

        if (canMove == false)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

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

        if(OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeDirection() * speed * 20f, ForceMode.Force); //Move to the direction of the slope

            if (rb.velocity.y > 0) //Keep it stuck on the slope if it's going up
            {
                //Debug.Log("Applying! " + rb.velocity.y);
                rb.AddForce(Vector3.down * 30f, ForceMode.Force);
            }
            return;
        }

        rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
    }

    private void HandleSpeedLimit()
    {
        //Change Speed

        if(pInput.sprintInput && grounded)
        {
            speed = moveSpeed * sprintMultiplier;
        }
        else
        {
            speed = moveSpeed;
        }

        if(pInput.crawlInput && grounded) 
        {
            speed = moveSpeed * crawlMultiplier;
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

    #endregion
    #region Crawl
    public void HandleCrawl()
    {
        #region Start Crawling Input
        if (grounded && pInput.crawlInputPressed && !isCrawling && !climbingLadder)
        {
            isCrawling = true;
            rb.AddForce(Vector3.down * 100f, ForceMode.Impulse);
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

        transform.localScale = playerSize;
        #endregion
    }

    private bool CanGoUp()
    {
        return !Physics.BoxCast(transform.position, Vector3.one / 2f, Vector3.up, Quaternion.identity, playerHeight + 0.3f, crouchCeilDetect);
    }
    #endregion
    #region Jump

    private void HandleJump()
    {
        if(pInput.jumpInputPressed && pInput.jumpInput && grounded)
        {
            exitingSlope = true;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), 1f);
        }
        
        if(pInput.jumpInputReleased && !pInput.jumpInput && !grounded)
        {
            
            Debug.Log("Jump Release");
            rb.AddForce(Vector3.down * rb.velocity.y * jumpCutMultiplier, ForceMode.Impulse);
        }
        
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

        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * .5f + 0.3f, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return (angle < maxSlopeAngle && angle != 0f);
        }
        return false;
    }

    public Vector3 GetSlopeDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    #endregion
}
