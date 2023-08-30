using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class P0_EnemyBrain : EnemyBrain
{
    [SerializeField] private GameObject trackIt;

     

    public override void Update()
    {
        enemyMovement.SetDestination(trackIt.transform);

        Debug.Log(enemyMovement.ReachedDestination());
    }
}
