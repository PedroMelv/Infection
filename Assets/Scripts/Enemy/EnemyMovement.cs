using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public enum MovementAIStates
{
    IDLE,
    WANDERING,
    SEARCHING,
    CHASING
}

public class EnemyMovement : MonoBehaviour
{
    #region Variables

    [SerializeField] private MovementAIStates moveStates;

    //Target
    private Transform target;
    [SerializeField] private LayerMask visionBlockerLayer;
    private bool lostVision;
    private float lostVisionTimer;

    [Header("Movement")]   
    [SerializeField] private float baseSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private bool sprinting = false;
    [SerializeField] private LayerMask groundLayer;
    private Vector3 moveDirection;

    private float speed;
    private bool canMove = false;

    private Rigidbody rb;
    
    //Pathfinding
    private Queue<Vector3> corners = new Queue<Vector3>();
    private Vector3[] pathStored = new Vector3[0];
    private NavMeshPath path;
    private Vector3 curTarget = Vector3.zero;
    private Vector3 finalTarget = new Vector3(-1000f,1000f,100000f);
    bool isLock = false;

    [Header("Slope")]
    private RaycastHit slopeHit;


    [Header("Rotation")]
    [SerializeField] private float rotationOffset;
    private Vector3 targetRotation;
    private float rotateSpeed;
    private bool rotating;

    #endregion

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
        HandleRotation();
        HandleMovement();
        HandleDrag();
        LimitSpeed();

        Debug.Log("Magnitude: " + GetCurVelocity());

        switch (moveStates)
        {
            case MovementAIStates.IDLE:
                sprinting = false;
                break;
            case MovementAIStates.WANDERING:
                sprinting = false;
                break;
            case MovementAIStates.SEARCHING:
                sprinting = false;
                break;
            case MovementAIStates.CHASING:
                ChaseState();
                break;
            default:
                break;
        }
    }
    private void FixedUpdate()
    {
        rb.useGravity = !OnSlope();

        if (canMove && !isLock)
        {
            moveDirection = curTarget - transform.position;
            moveDirection.y = 0f;

            if(OnSlope())
            {
                rb.AddForce(GetSlopeDirection() * speed * 15f, ForceMode.Force);

                if (rb.velocity.y > 0)
                {
                    rb.AddForce(Vector3.down * 30f, ForceMode.Force);
                }
            
            }else{
                rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
            }
        }
    }

    #region States Logic

    private void ChangeState(MovementAIStates state)
    {
        moveStates = state;
    }

    private void ChaseState()
    {
        sprinting = true;



        if (target != null)
        {
            SetRotation(curTarget, 15f);

            Vector3 dirToTarget = target.position - transform.position;
            float targetDst = Vector3.Distance(target.position, transform.position);

            lostVision = Physics.Raycast(transform.position, dirToTarget, targetDst, visionBlockerLayer);

            if (lostVision) lostVisionTimer -= Time.deltaTime; else lostVisionTimer = .25f;

            if (!lostVision || lostVisionTimer > 0f) SetDestination(target.position, null);
        }

        if (ReachedDestination())
        {
            if(target == null)
            {
                //TODO: Mudar o estado para o estado de procura
                //Criar o estado de procura
            }
            else
            {
                moveDirection = Vector3.zero;
                //Inimigo está perto do alvo e o alvo continua visível
                //TODO: Condição perfeita para atacar / Criar a condição de ataque 
            }
        }
    }

    #endregion

    #region SetDestination

    public void SetTarget(Transform target)
    {
        if (target == null) return;

        moveStates = MovementAIStates.CHASING;
        this.target = target;
    }

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
        if (finalTarget == pos)
        {
            isLock = false;
            callback?.Invoke(path);
            return;
        }

        // TODO: Garanir por role que executa so no server


        Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 100f, groundLayer);


        NavMesh.CalculatePath(transform.position, hit.point, NavMesh.AllAreas, path);
        

        if(path.corners.Length == 0)
        {
            isLock = false;
            Debug.Log("Impossible path");
            return;
        }

        pathStored = path.corners;

        corners = new Queue<Vector3>();

        for(int i = 1; i < path.corners.Length; i++)
        {
            Vector3 point = path.corners[i];
            point.y += 1f;

            Debug.Log("Adding " + point);

            corners.Enqueue(point);
        }

        Debug.Log(corners.Peek());

        curTarget = corners.Dequeue();
        finalTarget = pos;

        canMove = true;
        isLock = false;

        Debug.Log("Returning");

        callback?.Invoke(path);
    }
    #endregion

    #region SetRotation

    public void SetRotation(Vector3 dir, float rotateSpeed = 500f)
    {
        if (rotating) return;

        rotating = true;
        this.rotateSpeed = rotateSpeed;
        targetRotation = dir;
    }

    private void HandleRotation()
    {
        Vector3 dir = (targetRotation - transform.position).normalized;

        Quaternion dirRotation = Quaternion.LookRotation(dir);

        dirRotation.eulerAngles = new Vector3(0, dirRotation.eulerAngles.y, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, dirRotation, rotateSpeed * Time.deltaTime);


        rotating = false;
    }

    #endregion

    #region Logic

    private void HandleDrag()
    {
        float drag = 0f;

        if (moveDirection == Vector3.zero) drag = 5f;

        rb.drag = drag;
    }

    private void HandleMovement()
    {
        if (canMove)
        {

            if (Vector3.Distance(transform.position, curTarget) <= .75f * (GetCurVelocity() / (baseSpeed * sprintMultiplier)))
            {
                if (corners.Count == 0)
                {
                    canMove = false;
                }
                else
                {
                    curTarget = corners.Dequeue();
                }
            }
        }
    }
    private void HandleSpeed()
    {
        speed = baseSpeed;
        if(sprinting) speed *= sprintMultiplier;
    }

    private void LimitSpeed()
    {
        if (OnSlope())
        {
            Vector3 slopeVel = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
            if (slopeVel.magnitude > speed)
            {
                Vector3 speedVel = slopeVel.normalized * speed;
                rb.velocity = speedVel;
            }

        }
        else
        {

            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > speed)
            {
                Vector3 speedVel = flatVel.normalized * speed;
                speedVel.y = rb.velocity.y;
                rb.velocity = speedVel;
            }
        }
    }

    public bool ReachedDestination()
    {
        return corners.Count == 0;
    }

    public float GetCurVelocity()
    {
        if (rb == null) return 0f;

        if(OnSlope())
        {
            return Mathf.Max(1,rb.velocity.magnitude);
        }

        return Mathf.Max(1, new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude);
    }
   
    public bool OnSlope()
    {
        //if (exitingSlope) return false;

        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 2f * .5f + 0.3f, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
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
        if(corners.Count > 0)
        {
            Gizmos.color = Color.red;

            for (int i = 0; i < pathStored.Length; i++)
            {
                if(i > 0) Gizmos.DrawLine(pathStored[i-1], pathStored[i]);
                Gizmos.DrawSphere(pathStored[i], .75f * (GetCurVelocity() / (baseSpeed * sprintMultiplier)));
            }
        }
    }
}
