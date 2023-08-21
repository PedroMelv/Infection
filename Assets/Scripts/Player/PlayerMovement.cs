using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPun
{
    [SerializeField] private Transform feetPos;
    [SerializeField] private Transform orientation;

    [SerializeField] private LayerMask playerLayer;

    public bool canMove = true;

    [Header("Movement")]
    private float speed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float groundDrag;

    private Vector3 moveDirection;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask groundLayer;
    private bool grounded;

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
    private bool enteredSlope;

    private Rigidbody rb;
    private Collider col;
    private PlayerInput pInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        pInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if(photonView.IsMine == false) return;

        if (currentLadder != null) Debug.Log(Vector3.Dot(orientation.transform.forward , currentLadder.transform.forward));

        HandleGroundDrag();
        HandleSpeedLimit();
        HandleLadderDetection();
    }

    private void FixedUpdate()
    {
        if(photonView.IsMine == false) 
        {
            rb.useGravity = false;
            return;
        }
        HandleMovement();
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

            if(Physics.Raycast(transform.position, Vector3.up, 2f, playerLayer) && pInput.move_y_input > 0f)
            {
                DirectionPartOne = Vector3.zero;
            }

            if (Physics.Raycast(transform.position, Vector3.down, 2f, playerLayer) && pInput.move_y_input < 0f)
            {
                DirectionPartOne = Vector3.zero;
            }

            moveDirection = DirectionPartOne + DirectionPartTwo;

            rb.velocity = moveDirection;

            return;
        }

        moveDirection = orientation.forward * pInput.move_y_input + orientation.right * pInput.move_x_input;

        if(OnSlope())
        {
            rb.velocity = GetSlopeDirection() * speed;
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


        //Limit Speed
        Vector3 flatVel = Vector3.zero;

        if (OnSlope())
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

        if(moveDirection == Vector3.zero && OnSlope())
        {
            finalDrag = 100;
        }

        rb.drag = finalDrag;
    }

    #endregion

    #region Ladder Things

    private void HandleLadderDetection()
    {
        if(climbingLadder)
        {
            if (Physics.Raycast(feetPos.position + Vector3.up * 0.1f, ladderDirection, out ladderHit, 1f, ladderLayer))
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
            if(Physics.Raycast(feetPos.position + Vector3.up * 0.1f, orientation.forward, out ladderHit, 1f, ladderLayer))
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
