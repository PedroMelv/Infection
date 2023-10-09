using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInput pInput;
    [SerializeField] private PlayerMovement pMove;
    [SerializeField] private PlayerCombat pCombat;
    [SerializeField] private PlayerInventoryVisual pInvVisual;


    [Header("Camera Parameters")]
    [SerializeField] private float defaultFov;
    [SerializeField] private float aimingFov;
    [Space]
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;
    [Space]
    [SerializeField] private LayerMask blockStrafeLayer;
    [SerializeField] private float strafeDistance;
    [SerializeField] private float strafeAngle;
    [Space]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [Space]
    [SerializeField] private Transform cameraHandler;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform[] hands;

    private Camera camera;

    private float strafeRotation;
    private float strafePos;

    private float xRotation;
    private float yRotation;
    private float sensMult = 1f;

    [Header("HeadBob Parameters")]
    [SerializeField] private bool headBobEnabled = true;

    [SerializeField] private float walkBobAmount;
    [SerializeField] private float walkBobSpeed;

    [SerializeField] private float sprintBobAmount;
    [SerializeField] private float sprintBobSpeed;
    
    [SerializeField] private float crawlBobAmount;
    [SerializeField] private float crawlBobSpeed;

    private Vector3 headbobOffset;
    private float YheadbobTimer;
    private float XheadbobTimer;

    public void SetOwner(PlayerInput input)
    {
        pInput = input;

        pMove = pInput.gameObject.GetComponent<PlayerMovement>();
        pMove = pInput.gameObject.GetComponent<PlayerMovement>();
        pCombat = pInput.gameObject.GetComponent<PlayerCombat>();
        pInvVisual = pInput.gameObject.GetComponent<PlayerInventoryVisual>();

        pInput.myCamera = GetComponent<Camera>();
        camera = pInput.myCamera;

        pInvVisual.SetHands(hands);

        orientation = pInput.orientation;
        playerBody = pInput.playerBody;
        cameraPos = pInput.cameraPos;
    }

    private void LateUpdate()
    {
        if (pInput == null) return;

        HandleHeadbob();
        HandleFov();
        HandleStrafe();
        HandleCamera();
    }

    private void HandleCamera()
    {
        float xInput = pInput.mouse_x_input * (sensX * sensMult);
        float yInput = pInput.mouse_y_input * (sensY * sensMult);

        yRotation += xInput;

        xRotation -= yInput;
        xRotation = Mathf.Clamp(xRotation, minAngle, maxAngle);
        
        Quaternion cameraRotationTarget = Quaternion.Euler(xRotation, yRotation, strafeRotation);

        transform.rotation = Quaternion.Lerp(transform.rotation, cameraRotationTarget, 30f * Time.deltaTime); 
        
        
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);

        Vector3 dir = orientation.right;


        cameraHandler.position = Vector3.Lerp(cameraHandler.position, cameraPos.position + headbobOffset + dir * strafePos, 30f * Time.deltaTime);
    }

    private void HandleFov()
    {
        float fov = defaultFov;

        if (pCombat.IsAiming) fov = aimingFov;

        if (Mathf.Abs(fov - camera.fieldOfView) > 0.1f) camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, fov, 40f * Time.deltaTime);
    }

    private void HandleStrafe()
    {
        strafeRotation = -GetStrafeDir() * strafeAngle;
        strafePos = GetStrafeDir() * strafeDistance;
    }

    private int GetStrafeDir()
    {
        int dir = 0;

        if (pInput.strafeLeftInput) dir -= 1;
        if (pInput.strafeRightInput) dir += 1;

        Vector3 dirRay = orientation.right;


        if (dir != 0 && Physics.Raycast(cameraPos.position, dirRay * dir, 1f, blockStrafeLayer)) dir = 0;

        return dir;
    }
    private void HandleHeadbob()
    {
        if(headBobEnabled == false || pMove.IsGrounded == false)
        {
            headbobOffset = Vector3.zero;
        }
        else if(headBobEnabled == true)
        {
            float headbobSpeed = 0f;
            float headbobAmplitude = 0f;

            if(pInput.MoveInput() != Vector2.zero && !pMove.IsOnLadder())
            {
                switch (pMove.GetMoveState)
                {
                    case MoveStates.IDLE:
                        break;
                    case MoveStates.WALKING:
                        headbobSpeed = walkBobSpeed;
                        headbobAmplitude = walkBobAmount;
                        break;
                    case MoveStates.RUNNING:
                        headbobSpeed = sprintBobSpeed;
                        headbobAmplitude = sprintBobAmount;
                        break;
                    case MoveStates.JUMPING:
                        break;
                    case MoveStates.CRAWLING:
                        headbobSpeed = crawlBobSpeed;
                        headbobAmplitude = crawlBobAmount;
                        break;
                    case MoveStates.DUCT:
                        break;
                }

            }


            YheadbobTimer += Time.deltaTime * headbobSpeed;
            XheadbobTimer += Time.deltaTime * headbobSpeed / 2f;


            headbobOffset = (Vector3.up * Mathf.Sin(YheadbobTimer) * headbobAmplitude * 1.4f) + orientation.right * Mathf.Cos(XheadbobTimer) * headbobAmplitude * 1.6f;
        }
    }
}
