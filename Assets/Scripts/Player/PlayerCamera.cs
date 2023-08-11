using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Transform submarine;
    [SerializeField] private PlayerInput pInput;
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

    [SerializeField] private float walkAmount;
    [SerializeField] private float walkBobSpeed;

    private Vector3 headbobOffset;
    private float headbobTimer;

    private void LateUpdate()
    {
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

        transform.rotation = Quaternion.Euler(xRotation, yRotation + submarine.transform.eulerAngles.y, 0f);
        orientation.rotation = Quaternion.Euler(0f, yRotation + submarine.transform.eulerAngles.y, 0f);
        playerBody.rotation = Quaternion.Euler(0f, yRotation + submarine.transform.eulerAngles.y, 0f);

        cameraHandler.position = cameraPos.position + headbobOffset;
    }

    private void HandleHeadbob()
    {
        if(headBobEnabled == false)
        {
            headbobOffset = Vector3.zero;
        }
        else
        {
            headbobTimer += Time.deltaTime * walkBobSpeed;
            headbobOffset = new Vector3(0f, Mathf.Sin(headbobTimer) * walkAmount, 0f);
        }
    }
}
