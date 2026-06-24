using UnityEngine;

public class VillageRoutineNPC : MonoBehaviour, IInteractable
{
    [SerializeField] private string npcName = "Dan lang";
    [SerializeField] private string activityName = "Dang lam viec";
    [SerializeField] private Transform[] routinePoints;
    [SerializeField] private bool startRoutineOnAwake;
    [SerializeField] private float moveSpeed = 1.4f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float stopDistance = 0.25f;
    [SerializeField] private float waitAtPoint = 2f;
    [SerializeField] private Animator animator;
    [SerializeField] private string walkingBoolName = "IsWalking";
    [SerializeField] private string workingTriggerName = "Work";

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

    private void Start()
    {
        routineStarted = startRoutineOnAwake;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (simpleMotion == null)
        {
            simpleMotion = GetComponentInChildren<SimpleVillagerWorkMotion>();
        }

        SetSimpleMotion(routineStarted ? movingStyle : beforeRoutineStyle);
    }

    private void Update()
    {
        if (!routineStarted || routinePoints == null || routinePoints.Length == 0)
        {
            SetWalking(false);
            return;
        }

        if (Time.time < waitUntil)
        {
            SetWalking(false);
            SetSimpleMotion(workingStyle);
            isWaitingAtPoint = true;
            return;
        }

        if (isWaitingAtPoint)
        {
            isWaitingAtPoint = false;
            SetSimpleMotion(movingStyle);
        }

        Transform target = routinePoints[pointIndex];
        if (target == null)
        {
            GoToNextPoint();
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= stopDistance)
        {
            waitUntil = Time.time + waitAtPoint;
            SetWalking(false);
            PlayWorkAnimation();
            GoToNextPoint();
            return;
        }

        SetWalking(true);
        SetSimpleMotion(movingStyle);
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
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
        SetSimpleMotion(movingStyle);
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
        pointIndex = (pointIndex + 1) % routinePoints.Length;
    }

    private void SetWalking(bool shouldWalk)
    {
        if (isWalking == shouldWalk)
        {
            return;
        }

        isWalking = shouldWalk;

        if (animator != null && !string.IsNullOrEmpty(walkingBoolName))
        {
            animator.SetBool(walkingBoolName, isWalking);
        }
    }

    private void PlayWorkAnimation()
    {
        SetSimpleMotion(workingStyle);

        if (animator != null && !string.IsNullOrEmpty(workingTriggerName))
        {
            animator.SetTrigger(workingTriggerName);
        }
    }

    private void SetSimpleMotion(SimpleVillagerWorkMotion.WorkStyle style)
    {
        if (simpleMotion != null)
        {
            simpleMotion.SetWorkStyle(style);
        }
    }
}
