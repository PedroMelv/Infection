using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyBrain : MonoBehaviourPun
{
    protected EnemyMovement enemyMovement;

    protected Queue<BaseBehaviour> behaviours = new Queue<BaseBehaviour>();

    protected BaseBehaviour[] listBehaviours;

    protected BaseBehaviour curBehaviour;

    protected Transform currentTarget;

    [SerializeField] protected LayerMask groundLayer;

    public virtual void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    public virtual void Start()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient == false)
        {
            Destroy(this);

            return;
        }

        
    }

    public virtual void Update()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient == false)
        {
            return;
        }
        listBehaviours = behaviours.ToArray();
    }

    public virtual void TriggerVision(Transform pos)
    {

    }

    public virtual void TriggerVision(Vector3 pos)
    {

    }

    public virtual void HandleBehaviors()
    {
        if (curBehaviour == null && behaviours.TryDequeue(out BaseBehaviour b))
        {
            Debug.Log("Setting cur behaviour");
            curBehaviour = b;
        }
        else if(curBehaviour != null && curBehaviour.behaviourApplied == false)
        {
            if(curBehaviour is WaitBehaviour)
            {
                Debug.Log("Action: Wait");
                curBehaviour.behaviourApplied = true;

                return;
            }
            
            if(curBehaviour is MoveBehaviour)
            {
                Debug.Log("Action: Move");
                enemyMovement.SetDestination(curBehaviour as MoveBehaviour, (bool pathIsPossible, bool pathWasMade) =>
                {
                    if(pathIsPossible)
                    {
                        if(pathWasMade)
                        {
                            Debug.Log("Target Applied");
                            curBehaviour.behaviourApplied = true;
                        }
                    }
                    else
                    {
                        if (pathWasMade)
                        {
                            Debug.Log("Target Applied");
                            curBehaviour.behaviourApplied = true;
                        }
                    }
                });
                return;
            }
            
            if(curBehaviour is MoveStateChangeBehaviour)
            {
                Debug.Log("Action: Change");
                Debug.Log((curBehaviour as MoveStateChangeBehaviour).setTo.ToString());
                enemyMovement.ChangeState((curBehaviour as MoveStateChangeBehaviour).setTo);
                curBehaviour.behaviourApplied = true;
                return;
            }
        }

        if(curBehaviour != null)
        {
            if (curBehaviour is WaitBehaviour && (curBehaviour as WaitBehaviour).timeIdling > 0f)
            {
                (curBehaviour as WaitBehaviour).timeIdling -= Time.deltaTime;
                //Debug.Log((curBehaviour as WaitBehaviour).timeIdling);
                return;
            }

            if (curBehaviour.IsCompleted() && curBehaviour.behaviourApplied)
            {
                //Debug.Log("Finished");
                curBehaviour = null;
            }
        }

        //TODO: Testar esse sistema e adicionar a leitura dele
    }
 
    public virtual void RemoveBehaviours()
    {
        behaviours.Clear();
        curBehaviour = null;
    }
    public void AddMoveBehaviour(Vector3 target)
    {
        MoveBehaviour behaviour = new MoveBehaviour();

        if(Physics.Raycast(target, Vector3.down, out RaycastHit hit, 100f, groundLayer))
        {
            behaviour.targetPos = hit.point;
            behaviour.enemyPos = transform;

            behaviours.Enqueue(behaviour);

            return;
        }

        
    }

    public void AddWaitBehaviour(float waitTime)
    {
        WaitBehaviour behaviour = new WaitBehaviour();

        behaviour.timeIdling = waitTime;

        behaviours.Enqueue(behaviour);
    }

    public void AddChangeMoveStateBehaviour(MovementAIStates moveState)
    {
        MoveStateChangeBehaviour behaviour = new MoveStateChangeBehaviour();

        behaviour.setTo = moveState;

        behaviours.Enqueue(behaviour);
    }

    public class BaseBehaviour
    {
        public bool behaviourApplied;

        public virtual bool IsCompleted()
        {
            return true;
        }
    }

    public class MoveBehaviour : BaseBehaviour
    {
        public Transform enemyPos;
        public Vector3 targetPos;

        public virtual bool ReachedDestination()
        {
            //Debug.Log(Vector3.Distance(enemyPos.transform.position, targetPos));
            return Vector3.Distance(enemyPos.transform.position, targetPos) < 1.5f || enemyPos.GetComponent<EnemyMovement>().GetPathStored().Length == 0;
        }

        public override bool IsCompleted()
        {
            return ReachedDestination();
        }


    }


    public class WaitBehaviour : BaseBehaviour
    {
        public float timeIdling;

        public override bool IsCompleted()
        {
            if(timeIdling > 0f)
            {
                return false;
            }

            return true;
        }
    }

    public class MoveStateChangeBehaviour : BaseBehaviour
    {
        public MovementAIStates setTo;
        public override bool IsCompleted()
        {
            return true;
        }
    }
}
