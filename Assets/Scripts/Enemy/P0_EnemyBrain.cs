using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class P0_EnemyBrain : EnemyBrain
{
    [SerializeField] private GameObject trackIt;

    Vector3 rotateDir;


    public override void Update()
    {
        Debug.Log(behaviours.Count);
        if(Input.GetKeyDown(KeyCode.G))
        {
            RoomPoint[] points = GameDirector.instance.GetRandomPoints(5,2f, true, 0, 4, 1.5f);

            for (int i = 0; i < points.Length; i++)
            {
                
                AddMoveBehaviour(points[i].pos);
                if(points[i].isExtra) AddWaitBehaviour(Random.Range(.75f, 1.5f)); else AddWaitBehaviour(Random.Range(1f, 2.5f));

            }
        }

        HandleBehaviors();
    }

    public override void HandleBehaviors()
    {
        if (enemyMovement.GetMoveStates() == MovementAIStates.CHASING) return;
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
