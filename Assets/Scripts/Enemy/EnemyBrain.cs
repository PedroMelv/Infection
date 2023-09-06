using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyBrain : MonoBehaviour
{
    protected EnemyMovement enemyMovement;

    protected List<BaseBehaviour> behaviours = new List<BaseBehaviour>();

    public virtual void Awake()
    {
        enemyMovement = GetComponent<EnemyMovement>();
    }

    public virtual void Start()
    {
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient == false)
        {
            Destroy(this);
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
        if (behaviours.Count == 0) return;


    }

    private BaseBehaviour GetMostPriorityBehaviour()
    {
        BaseBehaviour behaviour = behaviours[0];

        for (int i = 0; i < behaviours.Count; i++)
        {
            if (behaviours[i].priority < behaviour.priority)
            {
                behaviour = behaviours[i];
            }
        }

        return behaviour;
    }

    private int GetNextPriority()
    {
        int priorityCount = 0;
        while(true)
        {
            if(ContainsPriority(priorityCount))
            {
                break;
            }
            else
            {
                priorityCount++;
            }
        }

        return priorityCount;
    }

    private bool ContainsPriority(int priority)
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            if (behaviours[i].priority == priority)
            {
                return true;
            }
        }

        return false;
    }


    protected class BaseBehaviour
    {
        public int priority = 0;

        public virtual bool IsCompleted()
        {
            return true;
        }
    }

    protected class MoveBehaviour : BaseBehaviour
    {
        public override bool IsCompleted()
        {
            return true;
        }
    }

}
