using UnityEngine;

public class SimpleAnimalWander : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float turnSpeed = 90f;
    [SerializeField] private float wanderRadius = 6f;
    [SerializeField] private float chooseTargetInterval = 3f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float nextChooseTime;

    private void Start()
    {
        startPosition = transform.position;
        ChooseNewTarget();
    }

    private void Update()
    {
        if (Time.time >= nextChooseTime || Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            ChooseNewTarget();
        }

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void ChooseNewTarget()
    {
        Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
        targetPosition = startPosition + new Vector3(randomPoint.x, 0f, randomPoint.y);
        nextChooseTime = Time.time + chooseTargetInterval;
    }
}
