using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float baseSpeed;
    [SerializeField] private bool sprinting = false;
    private float speed;
    private bool canMove = false;

    [SerializeField] private Vector3[] corners;
    private NavMeshPath path;
    private Vector3 curTarget = Vector3.zero;


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
            if (Vector3.Distance(transform.position, corners[index]) <= .25f)
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
            moveDirection.y = 0f;
            
            if(OnSlope())
            {
                rb.AddForce(GetSlopeDirection() * speed * 20f, ForceMode.Force);

                if (rb.velocity.y > 0) //Keep it stuck on the slope if it's going up
                {
                    Debug.Log("Applying! " + rb.velocity.y);
                    rb.AddForce(Vector3.down * 30f, ForceMode.Force);
                }

                
            }else{

                rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
            }

        }

        if(OnSlope())
        {
            Vector3 slopeVel = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
            if(slopeVel.magnitude > speed)
            {
                Vector3 speedVel = slopeVel.normalized * speed;
                rb.velocity = speedVel;
            }
            
        }else{

            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if(flatVel.magnitude > speed)
            {
                Vector3 speedVel = flatVel.normalized * speed;
                speedVel.y = rb.velocity.y;
                rb.velocity = speedVel;
            }
        }
        
    }

    #region SetDestination
    private void SetDestination(Vector3 pos, System.Action<NavMeshPath> callback)
    {
        if(!isLock)
            _ = InternalSetDestination(pos, callback);
        else
        {
            Debug.Log("Cant Return");
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

            Debug.Log("Exiting");

            callback?.Invoke(path);
            return;
        }

        // TODO: Garanir por role que executa so no server

        Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 100f);

        NavMesh.CalculatePath(transform.position, hit.point, NavMesh.AllAreas, path);

        if(path.corners.Length == 0)
        {
            isLock = false;
            Debug.Log("Impossible path");
            return;
        }

        corners = new Vector3[path.corners.Length - 1];

        for(int i = 0; i < corners.Length; i++)
        {
            corners[i] = path.corners[i + 1];
        }

        for (int i = 0; i < corners.Length; i++)
        {
            corners[i].y += 1f;
        }

        curTarget = pos;

        index = 0;
        canMove = true;

        isLock = false;

        Debug.Log("Returning");

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

    private void OnDrawGizmos()
    {
        if(corners.Length > 0)
        {
            Gizmos.color = Color.red;
            for(int i = 0; i < corners.Length; i++)
            {
                if(i > 0) Gizmos.DrawLine(corners[i-1],corners[i]);
                Gizmos.DrawSphere(corners[i], .25f);
            }
        }
    } 
}
