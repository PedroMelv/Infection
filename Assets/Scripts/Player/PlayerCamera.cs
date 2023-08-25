using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInput pInput;
    [SerializeField] private PlayerMovement pMove;
    [Header("Camera Parameters")]
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    [SerializeField] private Transform cameraHandler;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform orientation;

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

        pInput.myCamera = GetComponent<Camera>();

        orientation = pInput.orientation;
        playerBody = pInput.playerBody;
        cameraPos = pInput.cameraPos;
    }

    private void LateUpdate()
    {
        if (pInput == null) return;

        HandleHeadbob();
        HandleCamera();
    }

    private void HandleCamera()
    {
        float xInput = pInput.mouse_x_input * (sensX * sensMult);
        float yInput = pInput.mouse_y_input * (sensY * sensMult);

        yRotation += xInput;

        xRotation -= yInput;
        xRotation = Mathf.Clamp(xRotation, minAngle, maxAngle);


        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);

        cameraHandler.position = (cameraPos.position + headbobOffset);
    }

    private float LerpPos(float camPos, float camTarget, float speedTimer)
    {
        return Mathf.Lerp(camPos, camTarget, speedTimer * Time.deltaTime);
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
                headbobSpeed = walkBobSpeed;
                headbobAmplitude = walkBobAmount;
                if (pInput.sprintInput)
                {
                    headbobSpeed = sprintBobSpeed;
                    headbobAmplitude = sprintBobAmount;
                }
                if(pInput.crawlInput)
                {
                    headbobSpeed = crawlBobSpeed;
                    headbobAmplitude = crawlBobAmount;
                }
            }

            
            
            YheadbobTimer += Time.deltaTime * headbobSpeed;
            XheadbobTimer += Time.deltaTime * headbobSpeed / 2f;


            headbobOffset = (Vector3.up * Mathf.Sin(YheadbobTimer) * headbobAmplitude * 1.4f) + orientation.right * Mathf.Cos(XheadbobTimer) * headbobAmplitude * 1.6f;
        }
    }
}
