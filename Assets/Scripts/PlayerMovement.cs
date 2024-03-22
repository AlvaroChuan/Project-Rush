using System;
using System.Collections;
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
    [SerializeField] private float slideDuration;
    [SerializeField] private float slideCooldown;
    [SerializeField] private bool sliding = false;
    [SerializeField] private bool canSlide = true;

    [Header ("Grappling hook")]
    [SerializeField] private Grapplinghook grapplingHook;
    [SerializeField] private string grappleTag;
    [SerializeField] private float grappleForce;
    [SerializeField] private float grappleMaxDistance;
    [SerializeField] private float grappleCooldown;
    [SerializeField] private bool grappling = false;

    [Header ("Keybinds")]
    [SerializeField] private String jumpButton;
    [SerializeField] private String slideButton;
    [SerializeField] private String grappleButton;

    [Header ("Animations")]
    [SerializeField] private Animator animator;

    [Header ("Only for test build")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float outOfBoundsY;

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
        idleJumping,
        sprintingJumping,
        sliding,
        grappling
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
        if(grounded && moveDirection == Vector3.zero && canJump && !sliding) movementState = MovementState.idle;
        else if (grounded && moveDirection == Vector3.zero && !canJump && !sliding) movementState = MovementState.idleJumping;
        else if(grounded && moveDirection != Vector3.zero && canJump && !sliding) movementState = MovementState.sprinting;
        else if(grounded && moveDirection != Vector3.zero && !canJump && !sliding) movementState = MovementState.sprintingJumping;
        else if(grounded && moveDirection != Vector3.zero && canJump && sliding) movementState = MovementState.sliding;
        else if(!grounded && moveDirection != Vector3.zero && !canJump && !sliding && grappling) movementState = MovementState.grappling;
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
            case MovementState.idleJumping:
                animator.SetBool("Jumping", true);
                animator.SetBool("Idle", true);
                break;
            case MovementState.sprintingJumping:
                animator.SetBool("Jumping", true);
                animator.SetBool("Running", true);
                break;
            case MovementState.sliding:
                animator.SetBool("Sliding", true);
                break;
            case MovementState.grappling:
                animator.SetBool("Grappling", true);
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
        if(Input.GetButton(grappleButton) && grapplingHook.GetGrapplePoint() != Vector3.zero && !grappling)
        {
            StartCoroutine(Grapple());
        }
    }

    private void Move()
    {
        if(stickToSurfaces)
        {
            if(Physics.Raycast(transform.position, -transform.up, out contactPoint, playerHeight / 2 + groundDistance, surfaceMask))
            {
                if(canJump && !grappling) transform.position = contactPoint.point + transform.up * (playerHeight / 2);
                transform.up = Vector3.Slerp(transform.up, contactPoint.normal, surfaceAlignSpeed * Time.deltaTime);
            }
            else transform.up = Vector3.Slerp(transform.up, Vector3.up, surfaceAlignSpeed * Time.deltaTime);
        }
        moveDirection = orientationForward.forward * verticalInput + orientationRight.right * horizontalInput;
        if(grounded) rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if(transform.position.y < outOfBoundsY) transform.position = spawnPoint.position; //Only for test build
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

    private IEnumerator Grapple()
    {
        grapplingHook.Setup(grappleTag, grappleMaxDistance);
        grappling = true;
        rb.velocity = Vector3.zero;
        rb.AddForce((grapplingHook.GetGrapplePoint() - transform.position).normalized * grappleForce * 10, ForceMode.Impulse);
        float timePassed = 0;
        while(timePassed < grappleCooldown)
        {
            grapplingHook.DrawRope();
            timePassed += Time.deltaTime;
            yield return null;
        }
        grappling = false;
        grapplingHook.ClearRope();
    }
}
