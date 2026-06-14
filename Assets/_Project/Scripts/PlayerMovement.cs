using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;

    bool isGrounded;

    void Update()
    {
        //Kiem tra nguoi choi co dang dung tren mat dat hay khong
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //Tao vector di chuyen theo huong nhin cua nhan vat
        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        //Kiem tra nguoi choi dang dung tren mat dat de co the nhay
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //Cong thuc tinh van toc nhay ban dau
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //Ap dung trong luc theo thoi gian
        velocity.y += gravity * Time.deltaTime;

        //Di chuyen nguoi choi theo phuong thang dung
        controller.Move(velocity * Time.deltaTime);
    }
}
