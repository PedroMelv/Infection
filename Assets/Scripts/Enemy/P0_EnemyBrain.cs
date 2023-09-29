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

    [Space]

    [SerializeField] private float attackMaxCooldown;
    private float attackCooldown;


    public override void Start()
    {
        base.Start();

        attackCooldown = attackMaxCooldown;

        GetComponent<EnemyHealth>().OnDie += () =>
        {
            enemyMovement.ChangeState(MovementAIStates.FLEEING);
        };
        
        GetComponent<EnemyHealth>().OnTakeDamage += (Vector3 pos) =>
        {
            enemyMovement.ChangeState(MovementAIStates.NONE);
            enemyMovement.SetDestination(pos);
        };

        GetComponent<EnemyHealth>().OnFullHealth += () =>
        {
            enemyMovement.ChangeState(MovementAIStates.NONE);
        };
    }

    public override void Update()
    {
        BehaviourHandling();


        if (enemyMovement.closeToPlayer)
        {
            if(attackCooldown <= 0f)
            {
                currentTarget.GetComponent<PlayerHealth>().CallTakeDamage(1, Vector3.zero);
                attackCooldown = attackMaxCooldown;
            }
            else
            {
                attackCooldown -= Time.deltaTime;
            }
        }
    }

    private void BehaviourHandling()
    {
        if (behaviours.Count == 0)
        {
            //Sem ações para fazer

            if (enemyMovement.GetMoveStates() == MovementAIStates.FLEEING || enemyMovement.GetMoveStates() == MovementAIStates.CHASING) return;

            if (idlingTime <= 0f)
            {
                idlingTime = UnityEngine.Random.Range(idlingMinTime, idlingMaxTime);

                if (UnityEngine.Random.value < .75f)
                {
                    float b = UnityEngine.Random.value;

                    if (b >= 0f && b < .25f)
                    {
                        MoveToRandomPointAndSearch();
                    }
                    else if (b >= .25f && b < .35f)
                    {
                        MoveToClosestRoomAndSearch(20f);
                    }
                    else if (b >= .35f && b < .66f)
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
            //Executando ações


            HandleBehaviors();
        }
    }

    public override void HandleBehaviors()
    {
        if (enemyMovement.GetMoveStates() == MovementAIStates.FLEEING) return;

        if (enemyMovement.GetMoveStates() == MovementAIStates.CHASING || enemyMovement.GetMoveStates() == MovementAIStates.FLEEING)
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
                Debug.LogError("Brain não existe!");
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
        
        public TemplateBehaviour MoveToFurthestRoom(float waitTimePerPoints = 1f)
        {
            RoomPoint[] points = GameDirector.instance.GetFurthestPoints(brain.transform.position, 5, 4f, true, 0, 3, 1f);

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
        if (enemyMovement.GetMoveStates() == MovementAIStates.FLEEING) return;

        enemyMovement.SetTarget(pos);
        currentTarget = pos;
    }
    public override void TriggerVision(Vector3 pos)
    {
    }
}
