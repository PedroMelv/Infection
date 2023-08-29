using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviourPun
{
    public Camera myCamera;
    public Transform cameraPos;
    public Transform playerBody;
    public Transform orientation;

    public float move_x_input;
    public float move_y_input;

    public float mouse_x_input;
    public float mouse_y_input;

    public bool sprintInput;

    public Action OnInteractPress;
    public Action OnInteractHold;
    public Action OnInteractRelease;

    public bool strafeRightInput;
    public bool strafeLeftInput;

    public bool jumpInput;
    public bool jumpInputPressed;
    public bool jumpInputReleased;
    
    public bool crawlInput;
    public bool crawlInputPressed;
    public bool crawlInputReleased;

    public bool leftMouseInput;
    public bool leftMouseInputPressed;
    public bool leftMouseInputReleased;

    public bool rightMouseInput;
    public bool rightMouseInputPressed;
    public bool rightMouseInputReleased;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (photonView.IsMine == false) return;

        move_x_input = Input.GetAxisRaw("Horizontal");
        move_y_input = Input.GetAxisRaw("Vertical");

        mouse_x_input = Input.GetAxisRaw("Mouse X");
        mouse_y_input = Input.GetAxisRaw("Mouse Y");

        sprintInput = Input.GetKey(KeyCode.LeftShift);

        jumpInput = Input.GetKey(KeyCode.Space);
        jumpInputPressed = Input.GetKeyDown(KeyCode.Space);
        jumpInputReleased = Input.GetKeyUp(KeyCode.Space);

        crawlInput = Input.GetKey(KeyCode.LeftControl);
        crawlInputPressed = Input.GetKeyDown(KeyCode.LeftControl);
        crawlInputReleased = Input.GetKeyUp(KeyCode.LeftControl);

        strafeLeftInput = Input.GetKey(KeyCode.Q);
        strafeRightInput = Input.GetKey(KeyCode.E);

        leftMouseInput = Input.GetKey(KeyCode.Mouse0);
        leftMouseInputPressed = Input.GetKeyDown(KeyCode.Mouse0);
        leftMouseInputReleased = Input.GetKeyUp(KeyCode.Mouse0);

        rightMouseInput = Input.GetKey(KeyCode.Mouse1);
        rightMouseInputPressed = Input.GetKeyDown(KeyCode.Mouse1);
        rightMouseInputReleased = Input.GetKeyUp(KeyCode.Mouse1);

        if (Input.GetKeyDown(KeyCode.F)) OnInteractPress?.Invoke();
        if (Input.GetKey(KeyCode.F))     OnInteractHold?.Invoke();
        if (Input.GetKeyUp(KeyCode.F))   OnInteractRelease?.Invoke();
    }

    public Vector2 MoveInput()
    {
        return new Vector2(move_x_input, move_y_input);
    }
}
