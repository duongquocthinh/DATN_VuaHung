using UnityEngine;

public class SimpleAnimalWander : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float turnSpeed = 7f;
    [SerializeField] private float wanderRadius = 6f;
    [SerializeField] private float stopDistance = 0.45f;
    [SerializeField] private float minIdleTime = 1.2f;
    [SerializeField] private float maxIdleTime = 3f;
    [SerializeField] private float minWalkTime = 2.5f;
    [SerializeField] private float maxWalkTime = 5f;
    [SerializeField] private float groundRayHeight = 3f;
    [SerializeField] private float groundRayDistance = 8f;
    [SerializeField] private float groundOffset = 0.05f;
    [SerializeField] private float modelForwardOffsetY = 180f;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private LayerMask groundMask = ~0;

    private Vector3 homePosition;
    private Vector3 targetPosition;
    private float stateEndTime;
    private bool isMoving;
    private bool isStopped;
    private Animator animator;
    private string currentAnimation;

    private int GroundMaskValue
    {
        get { return groundMask.value == 0 ? ~0 : groundMask.value; }
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.applyRootMotion = false;
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

        Vector3 moveDirection = transform.forward;
        moveDirection.y = 0f;

        if (moveDirection.sqrMagnitude < 0.001f)
        {
            moveDirection = direction.normalized;
        }
        else
        {
            moveDirection.Normalize();
        }

        float moveStep = moveSpeed * Time.deltaTime;
        float distanceToTarget = direction.magnitude;
        transform.position += moveDirection * Mathf.Min(moveStep, distanceToTarget);

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
        for (int i = 0; i < 12; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
            Vector3 rayStart = homePosition + new Vector3(randomPoint.x, groundRayHeight, randomPoint.y);

            if (TryGetGroundPoint(rayStart, out Vector3 groundPoint))
            {
                targetPosition = groundPoint + Vector3.up * groundOffset;
                return true;
            }
        }

        return false;
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

            groundPoint = hit.point;
            return true;
        }

        groundPoint = transform.position;
        return false;
    }

    private void ApplyVisualForwardOffset()
    {
        if (visualRoot != null)
        {
            visualRoot.localRotation = Quaternion.Euler(0f, modelForwardOffsetY, 0f);
        }
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
