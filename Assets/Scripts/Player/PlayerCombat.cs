using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerInput pInput;

    private bool isAiming = false;

    public bool IsAiming { get { return isAiming; } private set { } }

    private void Awake()
    {
        pInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        isAiming = pInput.rightMouseInput;

       
    }
}
