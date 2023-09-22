using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using static EnemyBrain;
public enum MovementAIStates
{
    NONE,
    SEARCHING,
    CHASING,
    FLEEING
}

public class EnemyMovement : MovementBase
{
    #region Variables

    [SerializeField] private MovementAIStates moveStates;
    [SerializeField] private float maxDetectPointDistance;

    //Target
    private Transform target;
    [SerializeField] private LayerMask visionBlockerLayer;
    private bool lostVision;
    private float lostVisionTimer;

    //Chase
    [SerializeField] private float minSearchDuration = 2.5f;
    [SerializeField] private float maxSearchDuration = 10f;
    private float searchTimer;

    //Flee
    [SerializeField] private float timeStunnedOnFlee;
    private float timeStunned;

    [Header("Movement")]   
    [SerializeField] private float baseSpeed;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private bool sprinting = false;
    [SerializeField] private LayerMask groundLayer;

    private Vector3 moveDirection;

    private float speed;
    private bool hasPath = false;

    private Rigidbody rb;
    
    //Pathfinding

    public struct PathNode
    {
        public enum PathNodeType
        {
            MOVE,
            WALLHOLE
        }

        public PathNodeType type;

        public Vector3 moveTo;
        public WallHole wallHole;

        public PathNode(Vector3 moveTo)
        {
            type = PathNodeType.MOVE;
            this.moveTo = moveTo;
            wallHole = null;
        }

        public PathNode(WallHole hole, Vector3 basePos)
        {
            this.type = PathNodeType.WALLHOLE;
            moveTo = hole.GetClosestSide(basePos).position;
            wallHole = hole;
        }
    }

    [SerializeField] private LayerMask wallHoleLayer;

    private Queue<PathNode> corners = new Queue<PathNode>();
    private PathNode[] pathStored = new PathNode[0];
    private NavMeshPath path;
    private PathNode curTarget;
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

    public override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        timeStunned = timeStunnedOnFlee;
        path = new NavMeshPath();
    }
    private void Update()
    {
        if (!canMove)
        {
            Debug.Log("Cant move");
            return;
        }

        HandleSpeed();
        HandleRotation();
        HandleMovement();
        HandleDrag();
        LimitSpeed();


        switch (moveStates)
        {
            case MovementAIStates.NONE:
                sprinting = false;
                break;
            case MovementAIStates.SEARCHING:
                sprinting = false;
                SearchState();
                break;
            case MovementAIStates.CHASING:
                ChaseState();
                break;
            case MovementAIStates.FLEEING:
                FleeState();
                break;
            default:
                break;
        }
    }
    private void FixedUpdate()
    {
        if(!canMove)
        {
            rb.velocity = Vector3.zero;
            return;
        }
        rb.useGravity = !OnSlope();

        HandlePhysicMovement();

    }

    private void HandlePhysicMovement()
    {
        if (hasPath && !isLock)
        {
            moveDirection = curTarget.moveTo - transform.position;
            Vector3 rotatePos = curTarget.moveTo;


            if (OnSlope())
            {
                rb.AddForce(GetSlopeDirection() * speed * 15f, ForceMode.Force);
                SetRotation(rotatePos, 7.5f);

                if (rb.velocity.y > 0)
                {
                    rb.AddForce(Vector3.down * 30f, ForceMode.Force);
                }

            }
            else
            {
                rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
                SetRotation(rotatePos, 7.5f);
            }
        }
        else if (hasPath == false)
        {
            rb.drag = 40f;
        }
    }


    #region States Logic

    public void ChangeState(MovementAIStates state)
    {
        moveStates = state;
    }

    private void ChaseState()
    {
        sprinting = true;

        if (target != null)
        {
            SetRotation(curTarget.moveTo, 15f);

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

    private void SearchState()
    {
        if(searchTimer <= 0f)
        {
            if(Random.value > .75f)
            {
                SetDestination(transform.position + new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f)));
            }
            else
            {
                SetRotation(Quaternion.Euler(0f, Random.Range(0f, 360f), 0f).eulerAngles, 10f);
            }
            
            searchTimer = Random.Range(minSearchDuration, maxSearchDuration);
        }
        else if(pathStored.Length == 0)
        {
            searchTimer -= Time.deltaTime;
        }
    }

    private void FleeState()
    {
        bool pathSet = false;
        timeStunned = timeStunnedOnFlee;

        Vector3 furthestPoint = GameDirector.instance.GetFurthestPoints(transform.position, 1, 1, false, 0, 2, 1)[0].pos;

        while (true)
        {
            if(pathSet == false && SetDestination(furthestPoint))
            {
                Debug.Log("Setted");
                pathSet = true;
                return;
            }else if(pathSet == true && ReachedDestination())
            {

                if(timeStunned < 0f)
                {
                    ChangeState(MovementAIStates.NONE);
                    break;
                }
                else
                {
                    timeStunned -= Time.deltaTime;
                }
            }
            
        }
    }

    #endregion

    #region SetDestination

    public void SetTarget(Transform target)
    {
        if (target == null) return;

        ChangeState(MovementAIStates.CHASING);
        this.target = target;
    }

    public bool SetDestination(Vector3 pos, System.Action<bool, bool> pathCallback = null)
    {
        bool couldGo = true;

        if(!isLock)
        {
            _ = InternalSetDestination(pos, true, pathCallback);
            couldGo = true;
        }
        else
        {
            pathCallback?.Invoke(false, false);
            couldGo = false;
        }

        return couldGo;
    }

    public bool SetDestination(MoveBehaviour moveBehaviour, System.Action<bool, bool> pathCallback = null)
    {
        bool couldGo = true;

        if (!isLock)
        {
            _ = InternalSetDestination(moveBehaviour.targetPos, false, pathCallback);
            couldGo = true;
        }
        else
        {
            pathCallback?.Invoke(false, false);
            couldGo = false;
        }

        return couldGo;
    }

    public bool SetDestination(Transform pos, System.Action<bool, bool> pathCallback = null)
    {
        return SetDestination(pos.position, pathCallback);
    }
    private async Task InternalSetDestination(Vector3 pos, bool castTarget, System.Action<bool, bool> pathCallback)
    {
        isLock = true;
        if (finalTarget == pos || !canMove)
        {
            isLock = false;
            pathCallback?.Invoke(true, true);
            return;
        }

        if(castTarget)
        {
            Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 100f, groundLayer);

            NavMesh.CalculatePath(transform.position, hit.point, NavMesh.AllAreas, path);
        }
        else
        {
            NavMesh.CalculatePath(transform.position, pos, NavMesh.AllAreas, path);
        }


        if(path.corners.Length == 0)
        {
            isLock = false;
            Debug.Log("Impossible path");
            pathCallback?.Invoke(false, true);
            return;
        }

        corners = new Queue<PathNode>();

        if(path.corners.Length == 1)
        {
            Vector3 pointA = path.corners[0] + Vector3.up;
            Vector3 pointB = transform.position;

            float dist = Vector3.Distance(pointA, pointB);
            Vector3 dir = (pointA - pointB).normalized;

            if (Physics.Raycast(pointB, dir, out RaycastHit hit, dist, wallHoleLayer) && hit.collider.TryGetComponent(out WallHole wallHole))
            {
                PathNode node = new PathNode(wallHole, pointB);

                corners.Enqueue(node);
            }

            PathNode nodeBase = new PathNode(pointA);

            corners.Enqueue(nodeBase);


        }
        else
        {
            for (int i = 1; i < path.corners.Length; i++)
            {
                Vector3 pointA = path.corners[i];
                Vector3 pointB = path.corners[i - 1];

                pointA += Vector3.up;
                pointB += Vector3.up;

                float dist = Vector3.Distance(pointA, pointB);
                Vector3 dir = (pointA - pointB).normalized;

                if (Physics.Raycast(pointB, dir, out RaycastHit hit, dist, wallHoleLayer) && hit.collider.TryGetComponent(out WallHole wallHole))
                {
                    Debug.DrawLine(pointA, pointB, Color.yellow, 1000f);

                    PathNode node = new PathNode(wallHole, pointB);

                    corners.Enqueue(node);
                }
                else
                {
                    
                    PathNode node = new PathNode(pointA);

                    corners.Enqueue(node);
                }
            }
        }
        
        

        pathStored = corners.ToArray();

        curTarget = corners.Dequeue();
        finalTarget = pos;

        hasPath = true;
        isLock = false;

        Debug.Log("Returning");

        //if (moveStates != MovementAIStates.CHASING) ChangeState(MovementAIStates.WANDERING);

        pathCallback?.Invoke(true, true);
    }
    #endregion

    #region SetRotation

    public void SetRotation(Vector3 targetPos, float rotateSpeed = 500f)
    {
        if (rotating) return;

        rotating = true;
        this.rotateSpeed = rotateSpeed;
        targetRotation = targetPos;
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

        if (hasPath)
        {
            switch (curTarget.type)
            {
                case PathNode.PathNodeType.MOVE:

                    if (Vector3.Distance(transform.position, curTarget.moveTo) <= maxDetectPointDistance * (GetCurVelocity() / (baseSpeed * sprintMultiplier)))
                    {
                        Debug.Log("Reached point");
                        if (corners.Count == 0)
                        {
                            pathStored = new PathNode[0];
                            hasPath = false;
                        }
                        else
                        {
                            curTarget = corners.Dequeue();
                        }
                    }

                    break;
                case PathNode.PathNodeType.WALLHOLE:

                    if (Vector3.Distance(transform.position, curTarget.moveTo) <= 2f)
                    {
                        if(curTarget.wallHole.WallHoleInteract(this.gameObject, Vector3.up))
                        {
                            Debug.Log("Reached point");
                            if (corners.Count == 0)
                            {
                                pathStored = new PathNode[0];
                                hasPath = false;
                            }
                            else
                            {
                                curTarget = corners.Dequeue();
                                while(Vector3.Distance(curTarget.moveTo, transform.position) <= 2f)
                                {
                                    curTarget = corners.Dequeue();
                                }
                            }
                        }
                    }

                    break;
                default:
                    break;
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
        return (corners.Count == 0 && Vector3.Distance(transform.position, finalTarget) < .25f);
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

    public MovementAIStates GetMoveStates()
    {
        return moveStates;
    }

    public PathNode[] GetPathStored()
    {
        if (pathStored == null) return new PathNode[0];
        return pathStored;
    }

    #endregion

    private void OnDrawGizmos()
    {
        for (int i = 0; i < pathStored.Length; i++)
        {
            switch (pathStored[i].type)
            {
                case PathNode.PathNodeType.MOVE:
                    Gizmos.color = Color.red;

                    if(i > 0 && pathStored[i - 1].wallHole != null)
                    {
                        Gizmos.DrawLine(pathStored[i - 1].wallHole.GetClosestSide(pathStored[i].moveTo).position, pathStored[i].moveTo);
                    }
                    else if (i > 0) Gizmos.DrawLine(pathStored[i - 1].moveTo, pathStored[i].moveTo);


                    Gizmos.DrawSphere(pathStored[i].moveTo, maxDetectPointDistance * (GetCurVelocity() / (baseSpeed * sprintMultiplier)));

                    break;
                case PathNode.PathNodeType.WALLHOLE:
                    Gizmos.color = Color.blue;

                    if (i > 0) Gizmos.DrawLine(pathStored[i].moveTo, pathStored[i].wallHole.GetOpositeSide(pathStored[i].moveTo).position);
                    Gizmos.DrawSphere(pathStored[i].moveTo, .35f);
                    Gizmos.DrawSphere(pathStored[i].wallHole.GetOpositeSide(pathStored[i].moveTo).position, .35f);

                    break;
            }
            
        }
        
    }
}
