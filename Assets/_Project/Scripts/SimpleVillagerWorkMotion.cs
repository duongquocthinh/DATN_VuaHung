using UnityEngine;

public class SimpleVillagerWorkMotion : MonoBehaviour
{
    public enum WorkStyle
    {
        Idle,
        Listen,
        GatherLeaves,
        SitByFire,
        Cook,
        PoundRice,
        Carry
    }

    [SerializeField] private WorkStyle workStyle = WorkStyle.Idle;
    [SerializeField] private bool lookAtPlayerWhenNear = true;
    [SerializeField] private Transform player;
    [SerializeField] private float lookDistance = 5f;
    [SerializeField] private float lookTurnSpeed = 3f;
    [SerializeField] private float motionStrength = 1f;
    [SerializeField] private float randomTimeOffset;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    public void SetWorkStyle(WorkStyle newStyle)
    {
        workStyle = newStyle;
    }

    private void Start()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;

        if (Mathf.Approximately(randomTimeOffset, 0f))
        {
            randomTimeOffset = Random.Range(0f, 10f);
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        ApplyWorkMotion();
        LookAtPlayerIfNear();
    }

    private void ApplyWorkMotion()
    {
        float time = Time.time + randomTimeOffset;
        float slow = Mathf.Sin(time * 1.4f);
        float fast = Mathf.Sin(time * 3.2f);

        Vector3 positionOffset = Vector3.zero;
        Vector3 rotationOffset = Vector3.zero;

        switch (workStyle)
        {
            case WorkStyle.Listen:
                positionOffset = Vector3.up * (slow * 0.015f);
                rotationOffset = new Vector3(0f, slow * 2f, 0f);
                break;

            case WorkStyle.GatherLeaves:
                positionOffset = Vector3.up * (Mathf.Abs(fast) * -0.035f);
                rotationOffset = new Vector3(18f + Mathf.Abs(fast) * 10f, slow * 3f, 0f);
                break;

            case WorkStyle.SitByFire:
                positionOffset = Vector3.down * 0.22f + Vector3.up * (slow * 0.01f);
                rotationOffset = new Vector3(10f + slow * 2f, 0f, 0f);
                break;

            case WorkStyle.Cook:
                positionOffset = Vector3.up * (fast * 0.015f);
                rotationOffset = new Vector3(6f + Mathf.Abs(fast) * 7f, slow * 4f, fast * 2f);
                break;

            case WorkStyle.PoundRice:
                positionOffset = Vector3.up * (Mathf.Abs(fast) * 0.035f);
                rotationOffset = new Vector3(-8f + fast * 7f, 0f, slow * 2f);
                break;

            case WorkStyle.Carry:
                positionOffset = Vector3.up * (fast * 0.018f);
                rotationOffset = new Vector3(0f, slow * 2f, fast * 3f);
                break;

            default:
                positionOffset = Vector3.up * (slow * 0.018f);
                rotationOffset = new Vector3(0f, 0f, slow * 1.8f);
                break;
        }

        transform.localPosition = startLocalPosition + positionOffset * motionStrength;
        transform.localRotation = startLocalRotation * Quaternion.Euler(rotationOffset * motionStrength);
    }

    private void LookAtPlayerIfNear()
    {
        if (!lookAtPlayerWhenNear || player == null)
        {
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > lookDistance * lookDistance || direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookTurnSpeed * Time.deltaTime);
    }
}
