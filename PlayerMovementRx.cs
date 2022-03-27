using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class PlayerMovementRx : MonoBehaviour
{
    public CharacterController controller;

    [Header("Physics")]
    public float normalSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    Vector3 velocity;
    Vector3 moveDirection;

    [Header("Ground")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private bool isGrounded;
    private float jumpsLeft = 2;
    private bool isCrouched = false;

    [Header("Dash")]
    public float dashDuration = 0.2f;
    public float dashCooldown = 3;
    private float nextDash = 0;

    [Header("Flamethrower")]
    public ParticleSystem flamethrower;

    void Start() 
    {
        IObservable<Unit> update = this.UpdateAsObservable();

        //Movimiento
        update.Subscribe(_ => HandleMovement());

        //Saltar
        update
        .Where(_ => canJump())
        .Subscribe(_ => Jump());

        //Agacharse
        update
        .Where(_ => Input.GetButtonDown("Crouch"))
        .Subscribe(_ => Crouch());

        //Dash
        update
        .Where(_ => canDash())
        .Subscribe(_ => Dash());

        //Lanzallamas (No hace nada pero mola)
        update
        .Where(_ => Input.GetButtonDown("Flamethrower"))
        .Subscribe(_ => Flamethrower());
    }

    bool canJump() => Input.GetButtonDown("Jump") && (isGrounded || jumpsLeft > 0);

    bool canDash() => Input.GetButtonDown("Dash") && Time.time > nextDash + dashCooldown;

    public bool isPlayerGrounded() => isGrounded;

    void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position, 
            groundDistance, 
            groundMask
        );

        float speed = isCrouched ? normalSpeed/2 : normalSpeed;

        if ( isGrounded && velocity.y < 0 ) {
            velocity.y = -2f;
            jumpsLeft = 2;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        moveDirection = transform.right * x + transform.forward * z;

        controller.Move(moveDirection * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        
        controller.Move(velocity * Time.deltaTime);
    }

    void Jump() 
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        jumpsLeft--;
    }

    void Crouch() 
    {
        isCrouched = !isCrouched;
        controller.height = isCrouched ? 1 : 2;
    }

    void Dash() 
    {
        nextDash = Time.time;
        MainThreadDispatcher.StartUpdateMicroCoroutine(DashCoroutine());
    }

    void Flamethrower()
    {
        flamethrower.Play(true);
    }

    IEnumerator DashCoroutine()
    {
        float startTime = Time.time;

        while( Time.time < startTime + dashDuration )
        {
            velocity.y = 0;
            controller.Move(moveDirection * 40 * Time.deltaTime);
            yield return null;
        }
    }
}
