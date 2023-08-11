using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float move_x_input;
    public float move_y_input;

    public float mouse_x_input;
    public float mouse_y_input;

    private void Update()
    {
        move_x_input = Input.GetAxisRaw("Horizontal");
        move_y_input = Input.GetAxisRaw("Vertical");

        mouse_x_input = Input.GetAxisRaw("Mouse X");
        mouse_y_input = Input.GetAxisRaw("Mouse Y");
    }
}
