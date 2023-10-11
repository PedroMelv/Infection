using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBase : MonoBehaviourPun
{

    public bool canMove = true;
    protected Collider[] cols;

    protected Rigidbody rb;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cols = GetComponents<Collider>();
    }


    public void SetCollisions(bool to)
    {
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].isTrigger = to;
        }
    }

    public virtual void LockMovement()
    {
        canMove = false;
        SetCollisions(true);
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public virtual void UnlockMovement()
    {
        canMove = true;
        SetCollisions(false);
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}
