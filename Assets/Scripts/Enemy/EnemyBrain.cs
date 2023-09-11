using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyBrain : MonoBehaviour
{
    protected EnemyMovement enemyMovement;

    protected Queue<BaseBehaviour> behaviours = new Queue<BaseBehaviour>();

    protected BaseBehaviour curBehaviour;

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

    }

    public virtual void TriggerVision(Transform pos)
    {

    }

    public virtual void TriggerVision(Vector3 pos)
    {

    }

    public virtual void HandleBehaviors()
    {
        if(curBehaviour == null && behaviours.TryDequeue(out BaseBehaviour b))
        {
            curBehaviour = b;
        }
        else if(curBehaviour != null && curBehaviour.behaviourApplied == false)
        {
            if(curBehaviour is WaitBehaviour)
            {
                curBehaviour.behaviourApplied = true;

                return;
            }
            
            if(curBehaviour is MoveBehaviour)
            {
                if(enemyMovement.SetDestination( (curBehaviour as MoveBehaviour).targetPos ))
                {
                    Debug.Log( "Going to: " + (curBehaviour as MoveBehaviour).targetPos );
                    curBehaviour.behaviourApplied = true;
                }
            }
        }

        if(curBehaviour != null)
        {
            if (curBehaviour is WaitBehaviour && (curBehaviour as WaitBehaviour).timeIdling > 0f)
            {
                (curBehaviour as WaitBehaviour).timeIdling -= Time.deltaTime;
                Debug.Log((curBehaviour as WaitBehaviour).timeIdling);
                return;
            }

            if (curBehaviour.IsCompleted())
            {
                Debug.Log("Finished");
                curBehaviour = null;
            }
        }

        //TODO: Testar esse sistema e adicionar a leitura dele
    }
 
    public void AddMoveBehaviour(Vector3 target)
    {
        MoveBehaviour behaviour = new MoveBehaviour();

        behaviour.targetPos = target;
        behaviour.enemyPos = transform;

        behaviours.Enqueue(behaviour);
    }

    public void AddWaitBehaviour(float waitTime)
    {
        WaitBehaviour behaviour = new WaitBehaviour();

        behaviour.timeIdling = waitTime;

        behaviours.Enqueue(behaviour);
    }


    protected class BaseBehaviour
    {
        public bool behaviourApplied;

        public virtual bool IsCompleted()
        {
            return true;
        }
    }

    protected class MoveBehaviour : BaseBehaviour
    {
        public Transform enemyPos;
        public Vector3 targetPos;

        public virtual bool ReachedDestination()
        {
            Debug.Log(Vector3.Distance(enemyPos.transform.position, targetPos));
            return Vector3.Distance(enemyPos.transform.position, targetPos) < 1.5f;
        }

        public override bool IsCompleted()
        {
            return ReachedDestination();
        }
    }

    protected class WaitBehaviour : BaseBehaviour
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

}
