using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private Transform orientation;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    [SerializeField] private bool canJump = true;              //Serialized to help testing

    [Header ("Ground check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float playerHeight;
    [SerializeField] private float groundDistance;
    [SerializeField] private bool grounded;             //Serialized to help testing

    [Header ("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    [SerializeField] private RaycastHit slopeHit;
    [SerializeField] private bool onSlope;

    [Header ("Keybinds")]
    [SerializeField] private String jumpButton;
    [SerializeField] private String crouchButton;

    //Non-serialized
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private MovementState movementState;

    private enum MovementState
    {
        sprinting,
        air,
        sliding
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        CheckGround();
        GetInput();
        SpeedControl();
    }

    private void FixedUpdate()
    {
        Move();
        if(!CheckSlope())CustomGravity();
        StateHandler();    
    }

    private void StateHandler()
    {
        if(grounded && onSlope) movementState = MovementState.sliding;
        else if (grounded) movementState = MovementState.sprinting;
        else movementState = MovementState.air;
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if(Input.GetButtonDown(jumpButton) && canJump && grounded)
        {
            canJump = false;
            Jump();
            Invoke("ResetJump", jumpCooldown);
        }
    }

    private void Move()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if(CheckSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
        }
        if(grounded) rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void CustomGravity()
    {
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    private void CheckGround()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + groundDistance, groundMask);
        if(grounded) rb.drag = groundDrag;
        else rb.drag = airDrag;
    }

    private void SpeedControl()
    {
        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if(horizontalVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = horizontalVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector2.up * jumpForce, ForceMode.Impulse);
        canJump = false;
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private bool CheckSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + groundDistance, groundMask))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            onSlope = true;
            return angle < maxSlopeAngle && angle != 0;
        }
        else
        {
            onSlope = false;
            return false;
        }
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
