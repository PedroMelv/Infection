using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPun
{
    [SerializeField] private Transform feetPos;
    [SerializeField] private Transform orientation;

    public bool canMove = true;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;

    private Vector3 moveDirection;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask groundLayer;
    private bool grounded;

    [Header("LadderClimb")]
    [SerializeField] private LayerMask ladderLayer;
    [SerializeField] private float climbSpeed;
    private bool ladderDetected;
    private bool exitingLadder;

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
        HandleGroundDrag();
        HandleSpeedLimit();
        HandleLadderDetection();
    }

    private void FixedUpdate()
    {
        if(photonView.IsMine == false) 
        {
            col.isTrigger = true;
            rb.isKinematic = true;
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

        if (currentLadder != null)
        {
            rb.useGravity = false;

            float climbDirectionRaw = pInput.myCamera.transform.forward.y;
            float climbDirection = (Mathf.Abs(climbDirectionRaw) > .25f) ? 1f * Mathf.Sign(climbDirectionRaw) : 0f;

            Vector3 DirectionPartOne = Vector3.up * pInput.move_y_input * climbDirection * climbSpeed;
            Vector3 DirectionPartTwo = orientation.right * pInput.move_x_input * moveSpeed;

            //Se ir para tr�s ele cai da escada ou se n�o estiver olhando para a escada ele cai
            if ((climbDirection < 0f && pInput.move_y_input < 0f) || (ladderDetected == false && pInput.move_y_input > 0f) || (grounded && pInput.move_y_input < 0f))
            {
                currentLadder = null;
                DirectionPartOne = orientation.forward * pInput.move_y_input * moveSpeed;
            }

            moveDirection = DirectionPartOne + DirectionPartTwo;

            rb.velocity = moveDirection;

            return;
        }

        moveDirection = orientation.forward * pInput.move_y_input + orientation.right * pInput.move_x_input;

        if(OnSlope())
        {
            rb.velocity = GetSlopeDirection() * moveSpeed;
            return;
        }

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void HandleSpeedLimit()
    {
        Vector3 flatVel = Vector3.zero;

        if (OnSlope())
        {
            flatVel = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, limitedVel.y, limitedVel.z);
            }

            return;
        }

        flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
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
        Ray mouseRay = pInput.myCamera.ScreenPointToRay(Input.mousePosition);

        //mouseRay.origin = transform.position - Vector3.up * (playerHeight / 2f);

        ladderDetected = Physics.Raycast(feetPos.position, orientation.forward, out ladderHit, 1f, ladderLayer);

        if (ladderHit.collider != null && pInput.move_y_input != 0f) currentLadder = ladderHit.collider.gameObject;
    }

    private bool IsOnLadder()
    {
        if (currentLadder == null) return false;

        Vector3 ladderPos = currentLadder.transform.position;
        ladderPos.y = 0f;
        Vector3 playerPos = transform.position;
        playerPos.y = 0f;

        return Vector3.Distance(ladderPos, playerPos) <= 1f;
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
