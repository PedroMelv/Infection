using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    
    private Vector3 curTarget = Vector3.zero;

    private NavMeshPath path;
    [SerializeField] private Vector3[] corners;

    [SerializeField] private float baseSpeed;
    [SerializeField] private float sprintMultiplier;
    private float speed;
    private bool canMove = false;

    [SerializeField] private bool sprinting = false;

    [SerializeField] private LayerMask groundLayer;
    private RaycastHit slopeHit;

    private Vector3 moveDirection;

    private int index = 0;
    bool isLock = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        path = new NavMeshPath();
    }
    private void Update()
    {
        HandleSpeed();

        if(canMove)
        {
            if (Vector3.Distance(transform.position, corners[index]) <= .1f)
            {
                index++;
                if (index >= corners.Length) canMove = false;
                
            }
        }
    }
    private void FixedUpdate()
    {
        if (canMove && !isLock)
        {
            moveDirection = corners[index] - transform.position;

            
        }
    }

    #region SetDestination
    private void SetDestination(Vector3 pos, System.Action<NavMeshPath> callback)
    {
        if(!isLock)
            _ = InternalSetDestination(pos, callback);
        else
        {
            callback?.Invoke(null);
        }
    }
    public void SetDestination(Transform pos, System.Action<NavMeshPath> callback = null)
    {
        SetDestination(pos.position, callback);
    }
    private async Task InternalSetDestination(Vector3 pos, System.Action<NavMeshPath> callback)
    {
        isLock = true;
        if (curTarget == pos)
        {
            isLock = false;

            callback?.Invoke(path);
            return;
        }

        // TODO: Garanir por role que executa so no server

        Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 100f);

        NavMesh.CalculatePath(transform.position, hit.point, NavMesh.AllAreas, path);

        corners = path.corners;

        for (int i = 0; i < corners.Length; i++)
        {
            corners[i].y += 1f;
        }

        curTarget = pos;

        index = 0;
        canMove = true;

        isLock = false;
        callback?.Invoke(path);
    }
    #endregion

    #region Logic

    private void HandleSpeed()
    {
        speed = baseSpeed;
        if(sprinting) speed *= sprintMultiplier;

    }
    public bool ReachedDestination()
    {
        if (corners == null || corners.Length == 0) return false;

        return Vector3.Distance(transform.position, corners[corners.Length - 1]) <= .1f;
    }
   
    public bool OnSlope()
    {
        //if (exitingSlope) return false;

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2f * .5f + 0.3f, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            Debug.Log(angle);
            return (angle < 75f && angle != 0f);
        }
        return false;
    }

    public Vector3 GetSlopeDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    #endregion
}
