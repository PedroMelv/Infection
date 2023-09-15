using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBase : MonoBehaviourPun
{

    public bool canMove = true;
    protected Collider[] cols;

    public virtual void Awake()
    {
        cols = GetComponents<Collider>();
    }


    public void SetCollisions(bool to)
    {
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].isTrigger = to;
        }
    }
}
