using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    float xRotation = 0f;
    float YRotation = 0f;

    void Start()
    {
        //Khoa chuot vao giua man hinh
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //Xu ly camera xoay theo truc X
        xRotation -= mouseY;

        //Gioi han goc quay tren duoi de nguoi choi khong the lat nguoc dau
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Xu ly camera xoay theo truc Y
        YRotation += mouseX;


        //Ket hop xoay len xuong va trai phai
        transform.localRotation = Quaternion.Euler(xRotation, YRotation, 0f);

    }
}
