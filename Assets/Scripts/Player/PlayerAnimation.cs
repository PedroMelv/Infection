using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private GameObject playerBody;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void SetBodyActive(bool value)
    {
        playerBody.SetActive(value);
    }

    public void SetWalking(bool walking)
    {
        anim.SetBool(Animator.StringToHash("IsWalking"), walking);
    }    
    
    public void SetRunning(bool running)
    {
        anim.SetBool(Animator.StringToHash("IsRunning"), running);
    }
}
