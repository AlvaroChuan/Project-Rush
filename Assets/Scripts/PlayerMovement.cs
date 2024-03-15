using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private Transform orientationForward;
    [SerializeField] private Transform orientationRight;
    [SerializeField] private float groundDrag;
    [SerializeField] private float airDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    [SerializeField] private bool canJump = true;              //Serialized to help testing

    [Header ("Surface alignment")]
    [SerializeField] private bool stickToSurfaces = true;
    [SerializeField] private LayerMask surfaceMask;
    [SerializeField] private float surfaceAlignSpeed;

    [Header ("Ground check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float playerHeight;
    [SerializeField] private float groundDistance;
    [SerializeField] private bool grounded;                     //Serialized to help testing

    [Header ("Slide")]
    [SerializeField] private float slideForce;
    [SerializeField] private bool sliding = false;
    [SerializeField] private bool canSlide = true;
    [SerializeField] private float slideDuration;
    [SerializeField] private float slideCooldown;

    [Header ("Keybinds")]
    [SerializeField] private String jumpButton;
    [SerializeField] private String slideButton;

    [Header ("Animations")]
    [SerializeField] private Animator animator;

    //Non-serialized
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private MovementState movementState;
    private RaycastHit contactPoint;

    private enum MovementState
    {
        idle,
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
        AnimationHandler();
    }

    private void FixedUpdate()
    {
        Move();
        CustomGravity();
        StateHandler();
    }

    private void StateHandler()
    {
        if(grounded && sliding) movementState = MovementState.sliding;
        else if (grounded && moveDirection == Vector3.zero) movementState = MovementState.idle;
        else if (grounded) movementState = MovementState.sprinting;
        else movementState = MovementState.air;
    }

    private void AnimationHandler()
    {
        foreach(AnimatorControllerParameter parameter in animator.parameters)
        {
            animator.SetBool(parameter.name, false);
        }

        switch(movementState)
        {
            case MovementState.idle:
                animator.SetBool("Idle", true);
                break;
            case MovementState.sprinting:
                animator.SetBool("Running", true);
                break;
            case MovementState.air:
                if(sliding) animator.SetBool("Sliding", true);
                break;
            case MovementState.sliding:
                animator.SetBool("Sliding", true);
                break;
        }
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
        if(Input.GetButton(slideButton) && grounded && canSlide)
        {
            StartCoroutine(Slide());
        }
    }

    private void Move()
    {
        if(stickToSurfaces)
        {
            if(Physics.Raycast(transform.position, -transform.up, out contactPoint, playerHeight / 2 + groundDistance, surfaceMask))
            {
                if(canJump) transform.position = contactPoint.point + transform.up * (playerHeight / 2);
                transform.up = Vector3.Slerp(transform.up, contactPoint.normal, surfaceAlignSpeed * Time.deltaTime);
            }
            else transform.up = Vector3.Slerp(transform.up, Vector3.up, surfaceAlignSpeed * Time.deltaTime);
        }
        moveDirection = orientationForward.forward * verticalInput + orientationRight.right * horizontalInput;
        if(grounded) rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void CustomGravity()
    {
        rb.AddForce(-transform.up * gravity, ForceMode.Acceleration);
    }

    private void CheckGround()
    {
        grounded = Physics.Raycast(transform.position, -transform.up, playerHeight / 2 + groundDistance, groundMask);
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
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        canJump = false;
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private IEnumerator Slide()
    {
        sliding = true;
        canSlide = false;
        float timePassed = 0;
        while(timePassed < slideDuration)
        {
            Vector3 slideDirection;
            if(moveDirection != Vector3.zero) slideDirection = moveDirection;
            else slideDirection = orientationForward.forward;
            rb.AddForce(slideDirection.normalized * slideForce * 10f, ForceMode.Force);

            timePassed += Time.deltaTime;
            yield return null;
        }
        sliding = false;
        yield return new WaitForSeconds(slideCooldown - slideDuration);
        canSlide = true;
    }
}
