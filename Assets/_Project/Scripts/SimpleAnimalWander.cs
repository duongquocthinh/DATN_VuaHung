using UnityEngine;

public class SimpleAnimalWander : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float turnSpeed = 7f;
    [SerializeField] private float wanderRadius = 12f;
    [SerializeField] private float stopDistance = 0.45f;
    [SerializeField] private float minIdleTime = 1.2f;
    [SerializeField] private float maxIdleTime = 3f;
    [SerializeField] private float minWalkTime = 2.5f;
    [SerializeField] private float maxWalkTime = 5f;
    [SerializeField] private float groundRayHeight = 3f;
    [SerializeField] private float groundRayDistance = 8f;
    [SerializeField] private float groundOffset = 0.05f;
    [SerializeField] private float maxGroundStep = 0.7f;
    [SerializeField] private float minGroundNormalY = 0.55f;
    [SerializeField] private float modelForwardOffsetY = 180f;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Obstacle avoidance")]
    [SerializeField] private float obstacleCheckHeight = 0.7f;
    [SerializeField] private float obstacleCheckRadius = 0.45f;
    [SerializeField] private float obstacleCheckDistance = 1.2f;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private bool autoFitBoxCollider = true;
    [SerializeField] private Vector3 colliderPadding = new Vector3(0.15f, 0.1f, 0.15f);

    private Vector3 homePosition;
    private Vector3 targetPosition;
    private float stateEndTime;
    private bool isMoving;
    private bool isStopped;
    private Animator animator;
    private Collider bodyCollider;
    private Rigidbody animalRigidbody;
    private string currentAnimation;

    private int GroundMaskValue
    {
        get { return groundMask.value == 0 ? ~0 : groundMask.value; }
    }

    private int ObstacleMaskValue
    {
        get { return obstacleMask.value == 0 ? ~0 : obstacleMask.value; }
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        bodyCollider = GetBodyCollider();
        animalRigidbody = GetComponent<Rigidbody>();

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        if (animalRigidbody != null)
        {
            animalRigidbody.isKinematic = true;
            animalRigidbody.useGravity = false;
            animalRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        if (visualRoot == null && transform.childCount > 0)
        {
            visualRoot = transform.GetChild(0);
        }
    }

    private void Start()
    {
        homePosition = transform.position;
        SnapToGround();
        ApplyVisualForwardOffset();
        FitBoxColliderToVisual();
        StartIdle();
    }

    private void Update()
    {
        if (isStopped)
        {
            return;
        }

        SnapToGround();
        ApplyVisualForwardOffset();

        if (isMoving)
        {
            MoveToTarget();
            return;
        }

        if (Time.time >= stateEndTime)
        {
            StartMoving();
        }
    }

    public void StopWandering()
    {
        isStopped = true;
        isMoving = false;
        PlayAnimation("Idle");
    }

    private void MoveToTarget()
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= stopDistance || Time.time >= stateEndTime)
        {
            StartIdle();
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        Vector3 moveDirection = direction.normalized;

        float moveStep = moveSpeed * Time.deltaTime;
        float distanceToTarget = direction.magnitude;
        float stepDistance = Mathf.Min(moveStep, distanceToTarget);

        if (IsPathBlocked(moveDirection, obstacleCheckDistance + moveStep)
            || !CanMoveTo(transform.position + moveDirection * stepDistance))
        {
            StartIdle();
            return;
        }

        transform.position += moveDirection * stepDistance;

        SnapToGround();
        ApplyVisualForwardOffset();
        PlayAnimation("Walk");
    }

    private void StartIdle()
    {
        isMoving = false;
        stateEndTime = Time.time + Random.Range(minIdleTime, maxIdleTime);
        PlayAnimation("Idle");
    }

    private void StartMoving()
    {
        if (!TryChooseTarget())
        {
            StartIdle();
            return;
        }

        isMoving = true;
        stateEndTime = Time.time + Random.Range(minWalkTime, maxWalkTime);
        PlayAnimation("Walk");
    }

    private bool TryChooseTarget()
    {
        for (int i = 0; i < 18; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
            Vector3 rayStart = homePosition + new Vector3(randomPoint.x, groundRayHeight, randomPoint.y);

            if (TryGetGroundPoint(rayStart, out Vector3 groundPoint))
            {
                Vector3 candidate = groundPoint + Vector3.up * groundOffset;

                if (IsReachableTarget(candidate))
                {
                    targetPosition = candidate;
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsReachableTarget(Vector3 candidate)
    {
        Vector3 direction = candidate - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= stopDistance * stopDistance)
        {
            return false;
        }

        float distance = Mathf.Max(0f, direction.magnitude - stopDistance);
        return !IsPathBlocked(direction.normalized, distance);
    }

    private void SnapToGround()
    {
        Vector3 rayStart = transform.position + Vector3.up * groundRayHeight;

        if (TryGetGroundPoint(rayStart, out Vector3 groundPoint))
        {
            transform.position = new Vector3(transform.position.x, groundPoint.y + groundOffset, transform.position.z);
        }

        Vector3 euler = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
    }

    private bool TryGetGroundPoint(Vector3 rayStart, out Vector3 groundPoint)
    {
        RaycastHit[] hits = Physics.RaycastAll(
            rayStart,
            Vector3.down,
            groundRayDistance,
            GroundMaskValue,
            QueryTriggerInteraction.Ignore
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hit.normal.y < minGroundNormalY)
            {
                continue;
            }

            if (Mathf.Abs(hit.point.y - transform.position.y) > maxGroundStep
                && !(hit.collider is TerrainCollider))
            {
                continue;
            }

            groundPoint = hit.point;
            return true;
        }

        groundPoint = transform.position;
        return false;
    }

    private bool IsPathBlocked(Vector3 direction, float distance)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f || distance <= 0f)
        {
            return false;
        }

        Vector3 rayStart = transform.position + Vector3.up * obstacleCheckHeight;
        float checkRadius = GetObstacleCheckRadius();
        RaycastHit[] hits = Physics.SphereCastAll(
            rayStart,
            checkRadius,
            direction.normalized,
            distance,
            ObstacleMaskValue,
            QueryTriggerInteraction.Ignore
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null)
            {
                continue;
            }

            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hit.collider is TerrainCollider || hit.normal.y > 0.6f)
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private bool CanMoveTo(Vector3 nextPosition)
    {
        if (bodyCollider == null)
        {
            return true;
        }

        Vector3 checkCenter = nextPosition + Vector3.up * obstacleCheckHeight;
        float checkRadius = GetObstacleCheckRadius();
        Collider[] hits = Physics.OverlapSphere(
            checkCenter,
            checkRadius,
            ObstacleMaskValue,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hit is TerrainCollider)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private float GetObstacleCheckRadius()
    {
        if (bodyCollider == null)
        {
            return obstacleCheckRadius;
        }

        Vector3 extents = bodyCollider.bounds.extents;
        float bodyRadius = Mathf.Min(extents.x, extents.z) * 0.6f;
        return Mathf.Max(obstacleCheckRadius, bodyRadius);
    }

    private Collider GetBodyCollider()
    {
        Collider[] allColliders = GetComponentsInChildren<Collider>();

        foreach (Collider animalCollider in allColliders)
        {
            if (animalCollider != null && animalCollider.enabled && !animalCollider.isTrigger)
            {
                return animalCollider;
            }
        }

        return null;
    }

    private void ApplyVisualForwardOffset()
    {
        if (visualRoot != null)
        {
            visualRoot.localRotation = Quaternion.Euler(0f, modelForwardOffsetY, 0f);
        }
    }

    private void FitBoxColliderToVisual()
    {
        if (!autoFitBoxCollider)
        {
            return;
        }

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        bool hasBounds = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.one);

        foreach (Renderer visualRenderer in renderers)
        {
            if (visualRenderer == null || !visualRenderer.enabled)
            {
                continue;
            }

            Bounds rendererBounds = visualRenderer.bounds;
            Vector3 min = transform.InverseTransformPoint(rendererBounds.min);
            Vector3 max = transform.InverseTransformPoint(rendererBounds.max);

            Vector3 localMin = Vector3.Min(min, max);
            Vector3 localMax = Vector3.Max(min, max);

            Vector3[] corners =
            {
                transform.InverseTransformPoint(new Vector3(rendererBounds.min.x, rendererBounds.min.y, rendererBounds.min.z)),
                transform.InverseTransformPoint(new Vector3(rendererBounds.min.x, rendererBounds.min.y, rendererBounds.max.z)),
                transform.InverseTransformPoint(new Vector3(rendererBounds.min.x, rendererBounds.max.y, rendererBounds.min.z)),
                transform.InverseTransformPoint(new Vector3(rendererBounds.min.x, rendererBounds.max.y, rendererBounds.max.z)),
                transform.InverseTransformPoint(new Vector3(rendererBounds.max.x, rendererBounds.min.y, rendererBounds.min.z)),
                transform.InverseTransformPoint(new Vector3(rendererBounds.max.x, rendererBounds.min.y, rendererBounds.max.z)),
                transform.InverseTransformPoint(new Vector3(rendererBounds.max.x, rendererBounds.max.y, rendererBounds.min.z)),
                transform.InverseTransformPoint(new Vector3(rendererBounds.max.x, rendererBounds.max.y, rendererBounds.max.z))
            };

            foreach (Vector3 corner in corners)
            {
                localMin = Vector3.Min(localMin, corner);
                localMax = Vector3.Max(localMax, corner);
            }

            Bounds rendererLocalBounds = new Bounds((localMin + localMax) * 0.5f, localMax - localMin);

            if (!hasBounds)
            {
                localBounds = rendererLocalBounds;
                hasBounds = true;
            }
            else
            {
                localBounds.Encapsulate(rendererLocalBounds);
            }
        }

        if (!hasBounds)
        {
            return;
        }

        boxCollider.center = localBounds.center;
        boxCollider.size = localBounds.size + colliderPadding;
        bodyCollider = boxCollider;
    }

    private void PlayAnimation(string animationName)
    {
        if (animator == null || currentAnimation == animationName)
        {
            return;
        }

        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Run");
        animator.SetTrigger(animationName);
        currentAnimation = animationName;
    }
}
