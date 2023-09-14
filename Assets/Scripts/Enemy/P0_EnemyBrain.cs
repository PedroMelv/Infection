using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;



public class P0_EnemyBrain : EnemyBrain
{

    [SerializeField] private float idlingMinTime;
    [SerializeField] private float idlingMaxTime;
    private float idlingTime;

    Vector3 rotateDir;


    public override void Update()
    {
        
        if(behaviours.Count == 0)
        {
            //Sem a��es para fazer

            if(idlingTime <= 0f)
            {
                idlingTime = UnityEngine.Random.Range(idlingMinTime, idlingMaxTime);

                if(UnityEngine.Random.value < .75f)
                {
                    float b = UnityEngine.Random.value;

                    if (b >= 0f && b < .25f)
                    {
                        MoveToRandomPointAndSearch();
                    }else if(b >= .25f && b < .35f)
                    {
                        MoveToClosestRoomAndSearch(20f);
                    }
                    else if(b >= .35f && b < .66f)
                    {
                        Behave().ChangeState(MovementAIStates.SEARCHING).Run();
                    }
                    else
                    {
                        Behave().MoveToRandomRoom().Wait(1f).ChangeState(MovementAIStates.SEARCHING).Run();
                    }
                }
            }
            else
            {
                idlingTime -= Time.deltaTime;
            }

        }
        else
        {
            //Executando a��es


            HandleBehaviors();
        }
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

    #region TemplateBehaviours

    private class TemplateBehaviour
    {
        public Action actionQueue;

        public EnemyBrain brain;

        public void Run()
        {
            if(brain == null)
            {
                Debug.LogError("Brain n�o existe!");
                return;
            }
            actionQueue?.Invoke();
        }
        
        public TemplateBehaviour ChangeState(MovementAIStates state)
        {
            actionQueue += () => brain.AddChangeMoveStateBehaviour(state);

            return this;
        }

        public TemplateBehaviour MoveToRandomPoint()
        {
            RoomPoint point = GameDirector.instance.GetRandomPointOnMap();

            actionQueue += () =>
            {
                brain.AddMoveBehaviour(point.pos);
                brain.AddWaitBehaviour(1f);
            };

            return this;
        }

        public TemplateBehaviour MoveToClosestRoom(float waitTimePerPoints = 1f)
        {
            RoomPoint[] points = GameDirector.instance.GetClosestPoints(brain.transform.position, 5, 4f, true, 0, 3, 1f);

            actionQueue += () =>
            {
                for (int i = 0; i < points.Length; i++)
                {
                    brain.AddMoveBehaviour(points[i].pos);
                    brain.AddWaitBehaviour(waitTimePerPoints);
                }
            };

            return this;
        }
        public TemplateBehaviour MoveToRandomRoom(float waitTimePerPoints = 1f)
        {
            RoomPoint[] points = GameDirector.instance.GetRandomPoints(5, 4f, true, 0, 3, 1f);

            actionQueue += () =>
            {
                for (int i = 0; i < points.Length; i++)
                {
                    brain.AddMoveBehaviour(points[i].pos);
                    brain.AddWaitBehaviour(waitTimePerPoints);
                }
            };

            return this;
        }

        public TemplateBehaviour Wait(float waitTime)
        {
            actionQueue += () =>
            {
                brain.AddWaitBehaviour(waitTime);
            };

            return this;
        }
    }

    private TemplateBehaviour Behave()
    {
        TemplateBehaviour t = new TemplateBehaviour();

        t.brain = this;

        return t;
    }

    private void MoveToClosestRoom()
    {
        Behave().MoveToClosestRoom().ChangeState(MovementAIStates.NONE).Run();
    }
    private void MoveToClosestRoomAndSearch(float timeSearching = 10f)
    {
        Behave().MoveToClosestRoom().Wait(1f).ChangeState(MovementAIStates.SEARCHING).Wait(timeSearching).ChangeState(MovementAIStates.NONE).Run();
    }
    
    private void MoveToRandomPoint()
    {
        Behave().MoveToRandomPoint().ChangeState(MovementAIStates.NONE).Run();
    }
    
    private void MoveToRandomPointAndSearch(float timeSearching = 10f)
    {
        Behave().MoveToRandomPoint().Wait(1f).ChangeState(MovementAIStates.SEARCHING).Wait(timeSearching).ChangeState(MovementAIStates.NONE).Run();
    }

    #endregion

    public override void TriggerVision(Transform pos)
    {
        enemyMovement.SetTarget(pos);
    }
    public override void TriggerVision(Vector3 pos)
    {
    }
}