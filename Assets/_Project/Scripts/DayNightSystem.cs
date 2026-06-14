using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightSystem : MonoBehaviour
{
    public float daySpeed = 10f;

    void Update()
    {
        transform.Rotate(Vector3.right * daySpeed * Time.deltaTime);
    }
}
