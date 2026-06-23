using UnityEngine;

public class NPCIdleMotion : MonoBehaviour
{
    [SerializeField] private bool lookAtPlayer = true;
    [SerializeField] private Transform player;
    [SerializeField] private float lookDistance = 7f;
    [SerializeField] private float turnSpeed = 4f;
    [SerializeField] private float swayAngle = 2f;
    [SerializeField] private float swaySpeed = 1.5f;
    [SerializeField] private float bobHeight = 0.025f;
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    private void Start()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;

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
        ApplyIdleMotion();
        LookAtPlayerIfNear();
    }

    private void ApplyIdleMotion()
    {
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        transform.localPosition = startLocalPosition + Vector3.up * bob;
        transform.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, sway);
    }

    private void LookAtPlayerIfNear()
    {
        if (!lookAtPlayer || player == null)
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
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}
