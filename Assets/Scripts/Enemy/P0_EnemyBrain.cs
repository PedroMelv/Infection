using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class P0_EnemyBrain : EnemyBrain
{
    [SerializeField] private GameObject trackIt;

    Vector3 rotateDir;


    public override void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            RoomPoint point = GameDirector.instance.GetRandomPointOnMap();

            AddMoveBehaviour(point.pos);
            AddWaitBehaviour(Random.Range(1f, 2.5f));
            AddChangeMoveStateBehaviour(MovementAIStates.SEARCHING);
        }

        HandleBehaviors();
    }

    public override void HandleBehaviors()
    {
        if (enemyMovement.GetMoveStates() == MovementAIStates.CHASING) 
        {
            RemoveBehaviours();
            return;
        }
        base.HandleBehaviors();
    }


    public override void TriggerVision(Transform pos)
    {
        enemyMovement.SetTarget(pos);
    }
    public override void TriggerVision(Vector3 pos)
    {
    }
}
