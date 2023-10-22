using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class SubmarineTest : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10.0f, NavMesh.AllAreas))
        {
            Gizmos.DrawWireSphere(hit.position, 1f);
        }
    }
}
