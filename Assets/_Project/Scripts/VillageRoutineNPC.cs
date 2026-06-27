using UnityEngine;
using UnityEngine.AI;

public class VillageRoutineNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "DÃ¢n lÃ ng";
    [SerializeField] private string activityName = "Äang lÃ m viá»‡c";
    [SerializeField] private Transform[] routinePoints;
    [SerializeField] private bool startRoutineOnAwake;
    [SerializeField] private float moveSpeed = 1.4f;
    [SerializeField] private float cutsceneMoveSpeed = 2.2f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float stopDistance = 0.25f;
    [SerializeField] private float waitAtPoint = 2f;

    [Header("Navigation")]
    [SerializeField] private bool useNavMeshAgent = false;
    [SerializeField] private bool autoUseNavMeshWhenAvailable = true;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float navMeshSampleDistance = 1.5f;
    [SerializeField] private float navMeshAgentRadius = 0.35f;
    [SerializeField] private float navMeshAgentHeight = 1.8f;

    [Header("Cutscene movement safety")]
    [SerializeField] private bool snapToWorkPointAfterShortMove = true;
    [SerializeField] private float maxCutsceneWalkSeconds = 2.0f;
    [SerializeField] private bool stayAtFirstWorkPoint = true;
    [SerializeField] private bool stopAtLastRoutinePoint = false;

    [Header("Animation")]
    [SerializeField] private bool useAnimator = true;
    [SerializeField] private bool useWalkingAnimation = false;
    [SerializeField] private float forcedWalkingAnimationSeconds = 1.4f;
    [SerializeField] private bool keepOriginalTilt = true;
    [SerializeField] private Animator animator;
    [SerializeField] private string walkingBoolName = "IsWalking";
    [SerializeField] private string walkingStateName = "Walk";
    [SerializeField] private float walkingStateCrossFade = 0.08f;
    [SerializeField] private float minimumVisibleWalkSeconds = 2.2f;
    [SerializeField] private string workingTriggerName = "Work";
    [SerializeField] private string workingStateName = "Gather";
    [SerializeField] private float workingStateCrossFade = 0.12f;

    [Header("Simple motion when no animation clip")]
    [SerializeField] private SimpleVillagerWorkMotion simpleMotion;
    [SerializeField] private SimpleVillagerWorkMotion.WorkStyle beforeRoutineStyle = SimpleVillagerWorkMotion.WorkStyle.Listen;
    [SerializeField] private SimpleVillagerWorkMotion.WorkStyle movingStyle = SimpleVillagerWorkMotion.WorkStyle.Carry;
    [SerializeField] private SimpleVillagerWorkMotion.WorkStyle workingStyle = SimpleVillagerWorkMotion.WorkStyle.GatherLeaves;

    private int pointIndex;
    private float waitUntil;
    private bool routineStarted;
    private bool isWalking;
    private bool isWaitingAtPoint;
    private bool playedWorkWithoutPoint;
    private bool waitingAtFinalWorkPoint;
    private bool shouldSnapAfterShortMove;
    private float snapMoveUntil;
    private float forceWalkingAnimationUntil;
    private float nextWalkingStateRefresh;
    private bool lastWalkingAnimatorValue;
    private Vector3 startEulerAngles;
    private float currentMoveSpeed;

    private void OnValidate()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.stoppingDistance = stopDistance;
        }
    }

    private void Start()
    {
        routineStarted = startRoutineOnAwake;
        startEulerAngles = transform.eulerAngles;
        currentMoveSpeed = moveSpeed;

        if (navMeshAgent == null && ShouldPrepareNavMeshAgent())
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (navMeshAgent == null && ShouldPrepareNavMeshAgent())
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        ConfigureNavMeshAgent();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
            if (!useAnimator || animator.runtimeAnimatorController == null)
            {
                animator.enabled = false;
                animator = null;
            }
            else
            {
                animator.enabled = true;
            }
        }

        if (simpleMotion == null)
        {
            simpleMotion = GetComponentInChildren<SimpleVillagerWorkMotion>();
        }

        SetSimpleMotion(routineStarted ? movingStyle : beforeRoutineStyle);
    }

    private void Update()
    {
        UpdateWalkingAnimationParameter();

        if (!routineStarted)
        {
            SetWalking(false);
            StopNavMeshAgent();
            return;
        }

        if (waitingAtFinalWorkPoint)
        {
            SetWalking(false);
            StopNavMeshAgent();
            SetSimpleMotion(workingStyle);
            EnsureWorkAnimation();
            return;
        }

        if (!HasValidRoutinePoint())
        {
            SetWalking(false);
            StopNavMeshAgent();

            if (!playedWorkWithoutPoint)
            {
                playedWorkWithoutPoint = true;
                PlayWorkAnimation();
            }

            return;
        }

        if (Time.time < waitUntil)
        {
            SetWalking(false);
            StopNavMeshAgent();
            SetSimpleMotion(workingStyle);
            EnsureWorkAnimation();
            isWaitingAtPoint = true;
            return;
        }

        if (isWaitingAtPoint)
        {
            isWaitingAtPoint = false;
            SetSimpleMotion(movingStyle);
        }

        Transform target = GetCurrentRoutinePoint();
        if (target == null)
        {
            GoToNextPoint();
            return;
        }

        if (shouldSnapAfterShortMove && Time.time >= snapMoveUntil)
        {
            FinishCurrentMove();
            return;
        }

        if (CanUseNavMeshAgent())
        {
            MoveWithNavMeshAgent(target);
            return;
        }

        MoveManually(target);
    }

    private void MoveManually(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= stopDistance)
        {
            waitUntil = Time.time + waitAtPoint;
            SetWalking(false);
            PlayWorkAnimation();
            FinishOrAdvanceAfterArrival();
            return;
        }

        SetWalking(true);
        SetSimpleMotion(movingStyle);
        Quaternion targetRotation = GetFacingRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        transform.position += direction.normalized * currentMoveSpeed * Time.deltaTime;
    }

    private void MoveWithNavMeshAgent(Transform target)
    {
        ConfigureNavMeshAgent();
        Vector3 destination = GetNavMeshDestination(target.position);

        if (!navMeshAgent.hasPath || (navMeshAgent.destination - destination).sqrMagnitude > 0.04f)
        {
            navMeshAgent.SetDestination(destination);
        }

        Vector3 velocity = navMeshAgent.desiredVelocity;
        velocity.y = 0f;
        if (velocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = GetFacingRotation(velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= Mathf.Max(navMeshAgent.stoppingDistance, stopDistance))
        {
            ArriveAtCurrentPoint();
            return;
        }

        SetWalking(true);
        SetSimpleMotion(movingStyle);
    }

    private void ArriveAtCurrentPoint()
    {
        waitUntil = Time.time + waitAtPoint;
        SetWalking(false);
        StopNavMeshAgent();
        PlayWorkAnimation();
        FinishOrAdvanceAfterArrival();
    }

    private void LateUpdate()
    {
        if (!keepOriginalTilt)
        {
            return;
        }

        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.x = startEulerAngles.x;
        eulerAngles.z = startEulerAngles.z;
        transform.eulerAngles = eulerAngles;
    }

    public string GetInteractionText()
    {
        return npcName;
    }

    public void Interact()
    {
        NotificationUI.ShowMessage(npcName + ": " + activityName, 3f);
    }

    public void BeginRoutine()
    {
        routineStarted = true;
        playedWorkWithoutPoint = false;
        waitingAtFinalWorkPoint = false;
        shouldSnapAfterShortMove = snapToWorkPointAfterShortMove && (!stopAtLastRoutinePoint || GetValidPointCount() <= 1);
        snapMoveUntil = Time.time + Mathf.Max(0.2f, maxCutsceneWalkSeconds);
        pointIndex = GetFirstValidPointIndex();
        currentMoveSpeed = Mathf.Clamp(cutsceneMoveSpeed > 0f ? cutsceneMoveSpeed : moveSpeed, 0.5f, 2.4f);
        ConfigureNavMeshAgent();
        forceWalkingAnimationUntil = Time.time + Mathf.Max(minimumVisibleWalkSeconds, forcedWalkingAnimationSeconds);
        SetWalking(true);
        SetSimpleMotion(movingStyle);
    }

    public void FinishCurrentMove()
    {
        if (!HasValidRoutinePoint())
        {
            PlayWorkAnimation();
            waitingAtFinalWorkPoint = true;
            return;
        }

        Transform target = GetCurrentRoutinePoint();
        if (target == null)
        {
            PlayWorkAnimation();
            return;
        }

        bool canUseAgent = CanUseNavMeshAgent();
        Vector3 targetPosition = canUseAgent
            ? GetNavMeshDestination(target.position)
            : target.position;
        if (!canUseAgent)
        {
            targetPosition.y = transform.position.y;
        }
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (canUseAgent)
        {
            navMeshAgent.Warp(targetPosition);
            navMeshAgent.ResetPath();
        }
        else
        {
            transform.position = targetPosition;
        }

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = GetFacingRotation(direction.normalized);
        }

        waitUntil = Time.time + waitAtPoint;
        SetWalking(false);
        PlayWorkAnimation();
        FinishOrAdvanceAfterArrival();
    }

    private void FinishOrAdvanceAfterArrival()
    {
        shouldSnapAfterShortMove = false;

        if (stayAtFirstWorkPoint || (stopAtLastRoutinePoint && IsCurrentPointLastValidPoint()))
        {
            waitingAtFinalWorkPoint = true;
            waitUntil = 0f;
            return;
        }

        GoToNextPoint();
    }

    private bool CanUseNavMeshAgent()
    {
        if (!ShouldPrepareNavMeshAgent())
        {
            return false;
        }

        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (navMeshAgent == null)
        {
            return false;
        }

        if (!navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }

        if (!navMeshAgent.isOnNavMesh && NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
        }

        return navMeshAgent.isOnNavMesh;
    }

    private void ConfigureNavMeshAgent()
    {
        if (!ShouldPrepareNavMeshAgent())
        {
            return;
        }

        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        if (navMeshAgent == null)
        {
            return;
        }

        navMeshAgent.speed = currentMoveSpeed;
        navMeshAgent.stoppingDistance = stopDistance;
        navMeshAgent.radius = Mathf.Max(0.05f, navMeshAgentRadius);
        navMeshAgent.height = Mathf.Max(0.2f, navMeshAgentHeight);
        navMeshAgent.updateRotation = false;
    }

    private bool ShouldPrepareNavMeshAgent()
    {
        return useNavMeshAgent || autoUseNavMeshWhenAvailable;
    }

    private void StopNavMeshAgent()
    {
        if (CanUseNavMeshAgent())
        {
            navMeshAgent.ResetPath();
        }
    }

    private Vector3 GetNavMeshDestination(Vector3 requestedPosition)
    {
        if (NavMesh.SamplePosition(requestedPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return requestedPosition;
    }

    public void ConfigureSimpleRoutine(
        string displayName,
        string activity,
        Transform[] points,
        SimpleVillagerWorkMotion.WorkStyle workStyle,
        SimpleVillagerWorkMotion motion
    )
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            npcName = displayName;
        }

        if (!string.IsNullOrWhiteSpace(activity))
        {
            activityName = activity;
        }

        routinePoints = points;
        workingStyle = workStyle;
        simpleMotion = motion;
        startRoutineOnAwake = false;

        if (simpleMotion != null)
        {
            SetSimpleMotion(beforeRoutineStyle);
        }
    }

    private void GoToNextPoint()
    {
        if (routinePoints == null || routinePoints.Length == 0)
        {
            pointIndex = 0;
            return;
        }

        int startIndex = pointIndex;
        do
        {
            pointIndex = (pointIndex + 1) % routinePoints.Length;
        }
        while (routinePoints[pointIndex] == null && pointIndex != startIndex);
    }

    private Transform GetCurrentRoutinePoint()
    {
        if (routinePoints == null || routinePoints.Length == 0)
        {
            return null;
        }

        if (pointIndex < 0 || pointIndex >= routinePoints.Length || routinePoints[pointIndex] == null)
        {
            pointIndex = GetFirstValidPointIndex();
        }

        if (pointIndex < 0 || pointIndex >= routinePoints.Length)
        {
            return null;
        }

        return routinePoints[pointIndex];
    }

    private int GetFirstValidPointIndex()
    {
        if (routinePoints == null)
        {
            return -1;
        }

        for (int i = 0; i < routinePoints.Length; i++)
        {
            if (routinePoints[i] != null)
            {
                return i;
            }
        }

        return -1;
    }

    private bool HasValidRoutinePoint()
    {
        return GetFirstValidPointIndex() >= 0;
    }

    private bool IsCurrentPointLastValidPoint()
    {
        return pointIndex >= 0 && pointIndex == GetLastValidPointIndex();
    }

    private int GetLastValidPointIndex()
    {
        if (routinePoints == null)
        {
            return -1;
        }

        for (int i = routinePoints.Length - 1; i >= 0; i--)
        {
            if (routinePoints[i] != null)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetValidPointCount()
    {
        if (routinePoints == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < routinePoints.Length; i++)
        {
            if (routinePoints[i] != null)
            {
                count++;
            }
        }

        return count;
    }

    private void SetWalking(bool shouldWalk)
    {
        if (isWalking != shouldWalk)
        {
            nextWalkingStateRefresh = 0f;
        }

        isWalking = shouldWalk;
        UpdateWalkingAnimationParameter();
    }

    private void UpdateWalkingAnimationParameter()
    {
        if (animator == null || string.IsNullOrEmpty(walkingBoolName))
        {
            return;
        }

        if (lastWalkingAnimatorValue != isWalking)
        {
            lastWalkingAnimatorValue = isWalking;
            animator.SetBool(walkingBoolName, isWalking);
        }

        bool shouldForceWalkingState = isWalking && (useWalkingAnimation || Time.time < forceWalkingAnimationUntil);
        if (!shouldForceWalkingState || Time.time < nextWalkingStateRefresh)
        {
            return;
        }

        nextWalkingStateRefresh = Time.time + 0.25f;

        if (!IsInAnimatorState(walkingStateName))
        {
            PlayAnimatorState(walkingStateName, walkingStateCrossFade);
        }
    }

    private bool IsInAnimatorState(string stateName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
        {
            return false;
        }

        int stateHash = Animator.StringToHash(stateName);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.shortNameHash == stateHash;
    }

    private void PlayWorkAnimation()
    {
        SetSimpleMotion(workingStyle);

        if (animator == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(workingTriggerName))
        {
            animator.SetTrigger(workingTriggerName);
        }

        PlayAnimatorState(workingStateName, workingStateCrossFade);
    }

    private void EnsureWorkAnimation()
    {
        if (animator == null || string.IsNullOrWhiteSpace(workingStateName))
        {
            return;
        }

        int stateHash = Animator.StringToHash(workingStateName);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash != stateHash)
        {
            PlayAnimatorState(workingStateName, workingStateCrossFade);
        }
    }

    private void PlayAnimatorState(string stateName, float crossFadeDuration)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
        {
            return;
        }

        int stateHash = Animator.StringToHash(stateName);
        if (animator.HasState(0, stateHash))
        {
            animator.CrossFadeInFixedTime(stateHash, Mathf.Max(0.01f, crossFadeDuration), 0);
        }
    }

    private void SetSimpleMotion(SimpleVillagerWorkMotion.WorkStyle style)
    {
        if (simpleMotion != null)
        {
            simpleMotion.SetWorkStyle(style);
        }
    }

    private Quaternion GetFacingRotation(Vector3 direction)
    {
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        return Quaternion.Euler(startEulerAngles.x, lookRotation.eulerAngles.y, startEulerAngles.z);
    }
}
