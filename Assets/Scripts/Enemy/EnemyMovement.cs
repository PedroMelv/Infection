using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector3 curTarget = Vector3.zero;

    [SerializeField] private float baseSpeed;
    [SerializeField] private float sprintMultiplier;
    private float speed;

    [SerializeField] private bool sprinting = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        HandleSpeed();
    }
    public void SetDestination(Transform pos)
    {
        SetDestination(pos.position);
    }
    public void SetDestination(Vector3 pos)
    {
        if (curTarget == pos) return;
        
        agent.SetDestination(pos);
        curTarget = pos;
    }
 
    private void HandleSpeed()
    {
        speed = baseSpeed;
        if(sprinting) speed *= sprintMultiplier;

        agent.speed = speed;
    }

    public bool ReachedDestination()
    {
        return agent.remainingDistance <= .1f;
    }
}
