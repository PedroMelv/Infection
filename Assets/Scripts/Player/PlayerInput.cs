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

    public Action OnInteractPress;
    public Action OnInteractHold;
    public Action OnInteractRelease;



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

        if (Input.GetKeyDown(KeyCode.E)) OnInteractPress?.Invoke();
        if (Input.GetKey(KeyCode.E))     OnInteractHold?.Invoke();
        if (Input.GetKeyUp(KeyCode.E))   OnInteractRelease?.Invoke();
    }
}
