using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Photon.Pun;



public class P0_EnemyBrain : EnemyBrain
{

    [SerializeField] private float idlingMinTime;
    [SerializeField] private float idlingMaxTime;
    private float idlingTime;

    [Space]

    [SerializeField] private float attackMaxCooldown;
    private float attackCooldown;
    
    [SerializeField] private float screamMaxCooldown;
    private float screamCooldown;

    [SerializeField] private AudioClip screamSound;
    [SerializeField] private AudioClip[] attackSound;

    public override void Start()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;

        base.Start();

        screamCooldown = screamMaxCooldown;
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
        if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;

        BehaviourHandling();


        if (enemyMovement.closeToPlayer)
        {
            if(attackCooldown <= 0f)
            {
                if(currentTarget != null)
                {
                    CallPlayAttack();
                    currentTarget.GetComponent<PlayerHealth>().CallTakeDamage(1, Vector3.zero);
                }
                attackCooldown = attackMaxCooldown;
            }
            else
            {
                attackCooldown -= Time.deltaTime;
            }
        }else if(enemyMovement.closeToPlayer == false && enemyMovement.GetMoveStates() == MovementAIStates.CHASING)
        {
            if(screamCooldown <= 0f)
            {
                screamCooldown = UnityEngine.Random.Range(2f, screamMaxCooldown);

                CallPlayScream();
            }
            else
            {
                screamCooldown -= Time.deltaTime;
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
            if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;

            if (brain == null)
            {
                Debug.LogError("Brain não existe!");
                return;
            }
            actionQueue?.Invoke();
        }
        
        public TemplateBehaviour ChangeState(MovementAIStates state)
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient) return this;

            actionQueue += () => brain.AddChangeMoveStateBehaviour(state);

            return this;
        }

        public TemplateBehaviour MoveToRandomPoint()
        {
            if (!PhotonNetwork.LocalPlayer.IsMasterClient) return this;

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
            if (!PhotonNetwork.LocalPlayer.IsMasterClient) return this;

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
            if (!PhotonNetwork.LocalPlayer.IsMasterClient) return this;
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
            if (!PhotonNetwork.LocalPlayer.IsMasterClient) return this;
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
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
                return this;

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

    private void CallPlayAttack()
    {
        photonView.RPC(nameof(RPC_PlayAttackSound), RpcTarget.All);
    }

    private void CallPlayScream()
    {
        photonView.RPC(nameof(RPC_PlayScreamSound), RpcTarget.All);
    }

    #region RPC

    [PunRPC]
    public void RPC_PlayAttackSound()
    {
        AudioClip stepClip = attackSound[UnityEngine.Random.Range(0, attackSound.Length)];

        GameObject stepSound = new GameObject(stepClip.name);

        stepSound.transform.position = transform.position - Vector3.down;

        AudioSource source = stepSound.AddComponent<AudioSource>();

        source.clip = stepClip;
        source.volume = .35f;

        source.maxDistance = 15f;
        source.spatialBlend = 1f;

        source.Play();

        Destroy(stepSound, stepClip.length + .1f);
    }
    [PunRPC]
    public void RPC_PlayScreamSound()
    {
        AudioClip stepClip = screamSound;

        GameObject stepSound = new GameObject(stepClip.name);

        stepSound.transform.position = transform.position - Vector3.down;

        AudioSource source = stepSound.AddComponent<AudioSource>();

        source.clip = stepClip;
        source.volume = .35f;

        source.maxDistance = 15f;
        source.spatialBlend = 1f;

        source.Play();

        Destroy(stepSound, stepClip.length + .1f);
    }

    #endregion
}
