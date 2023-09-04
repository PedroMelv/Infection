using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class P0_EnemyBrain : EnemyBrain
{
    [SerializeField] private GameObject trackIt;

    Vector3 rotateDir;



    public override void Update()
    {
        //Debug.Log(enemyMovement.SetRotation(rotateDir, 100f));
        //enemyMovement.SetDestination(trackIt.transform);
    }


    public override void TriggerVision(Transform pos)
    {
        enemyMovement.SetTarget(pos);
    }
    public override void TriggerVision(Vector3 pos)
    {
    }
}
