using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuctTrigger : Interactable
{
    [SerializeField] private Vector3 inDuct;
    [SerializeField] private Vector3 outDuct;

    public Vector3 GetInDuct()
    {
        return inDuct + transform.position;
    }

    public Vector3 GetOutDuct()
    {
        return outDuct + transform.position;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetInDuct(),.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GetOutDuct(),.5f);
    }
}
