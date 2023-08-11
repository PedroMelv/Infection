using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody submarineBody;

    private Vector3 submarineLastPosition;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;

    private Vector3 moveDirection;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask groundLayer;
    private bool grounded;

    [Header("Slope")]
    [SerializeField, Range(0f, 90f)] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool enteredSlope;

    private Rigidbody rb;
    private PlayerInput pInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        HandleGroundDrag();
        HandleSpeedLimit();
    }

    private void FixedUpdate()
    {
        FollowSubmarine();
        HandleMovement();
    }

    private void FollowSubmarine()
    {
        if(submarineBody == null) return;

        if(Vector3.Distance(transform.position, submarineBody.transform.position) > 10f) submarineBody = null;

        if(submarineBody == null) return;

        if(submarineLastPosition != Vector3.zero)
        {
            Vector3 dir = submarineBody.position - submarineLastPosition;
            moveDirection += dir;
            rb.AddForce(dir, ForceMode.Acceleration);
        }

        submarineLastPosition = submarineBody.position;
    }

    #region Movement
    private void HandleMovement()
    {
        moveDirection = orientation.forward * pInput.move_y_input + orientation.right * pInput.move_x_input;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void HandleSpeedLimit()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
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

        rb.drag = finalDrag;
    }

    #endregion
}
