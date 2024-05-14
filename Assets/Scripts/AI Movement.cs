using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class AIMovement : MonoBehaviour
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
    [SerializeField] private float slideSpeedExtra;
    [SerializeField] private float slideDuration;
    [SerializeField] private float slideCooldown;
    [SerializeField] private bool sliding = false;
    [SerializeField] private bool canSlide = true;

    [Header ("Animations")]
    [SerializeField] private Transform playerObj;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem dust;

    [Header ("Bounds")]
    [SerializeField] private Vector3 spawnPointPosition;
    [SerializeField] private Quaternion spawnPointRotation;
    [SerializeField] private float outOfBoundsY;
    [SerializeField] private LayerMask wallMask;

    //Non-serialized
    private Vector3 moveDirection;
    public Rigidbody rb;
    private MovementState movementState;
    private RaycastHit contactPoint;
    public bool canMove = true;

    //For Machine Learning
    public float distanceToLeftWall;
    public float distanceToRightWall;
    public float horizontalInput;
    public float verticalInput;
    public int jumpInput;
    public int slideInput;

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
        if(canMove) ReadInput();
        else moveDirection = Vector3.zero;
        SpeedControl();
        AnimationHandler();
        GetWallsDistances();
        RotatePlayer();
    }

    private void FixedUpdate()
    {
        if(canMove) Move();
        CustomGravity();
        StateHandler();
    }

    private void RotatePlayer()
    {
        Vector3 velDir = rb.velocity;
        orientationForward.forward = Vector3.Slerp(orientationForward.forward, velDir.normalized, Time.deltaTime * (rotationSpeed - 15));
        orientationRight.right = -Vector3.Cross(orientationForward.forward, transform.up);
        orientationForward.forward = Vector3.Cross(orientationRight.right, transform.up);
        playerObj.forward = Vector3.Slerp(playerObj.forward, velDir.normalized, Time.deltaTime * rotationSpeed);
        playerObj.localRotation = Quaternion.Euler(0, playerObj.localEulerAngles.y, 0);
    }

    private void GetWallsDistances()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, -orientationRight.right, out hit, 100f, wallMask))
        {
            distanceToLeftWall = hit.distance;
        }
        if(Physics.Raycast(transform.position, orientationRight.right, out hit, 100f, wallMask))
        {
            distanceToRightWall = hit.distance;
        }
    }

    private void StateHandler()
    {
        if(grounded && moveDirection == Vector3.zero && canJump && !sliding) movementState = MovementState.idle;
        else if(grounded && moveDirection == Vector3.zero && !canJump && !sliding) movementState = MovementState.idleJumping;
        else if(grounded && moveDirection != Vector3.zero && canJump && !sliding) movementState = MovementState.sprinting;
        else if(grounded && moveDirection != Vector3.zero && !canJump && !sliding) movementState = MovementState.sprintingJumping;
        else if(grounded && moveDirection != Vector3.zero && canJump && sliding) movementState = MovementState.sliding;
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
                if(dust.isPlaying) dust.Stop();
                break;
            case MovementState.sprinting:
                animator.SetBool("Running", true);
                if(!dust.isPlaying) dust.Play();
                break;
            case MovementState.idleJumping:
                animator.SetBool("Jumping", true);
                animator.SetBool("Idle", true);
                if(dust.isPlaying) dust.Stop();
                break;
            case MovementState.sprintingJumping:
                animator.SetBool("Jumping", true);
                animator.SetBool("Running", true);
                if(dust.isPlaying) dust.Stop();
                break;
            case MovementState.sliding:
                animator.SetBool("Sliding", true);
                if(!dust.isPlaying) dust.Play();
                break;
        }
    }

    private void ReadInput()
    {
        if(jumpInput == 1 && canJump && grounded)
        {
            canJump = false;
            Jump();
            Invoke("ResetJump", jumpCooldown);
        }
        if(slideInput == 1 && grounded && canSlide)
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

        if(transform.position.y < outOfBoundsY)
        {
            transform.position = spawnPointPosition;
            transform.rotation = spawnPointRotation;
            rb.velocity = Vector3.zero;
        }
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
        Vector3 limitedVel = new Vector3();
        if(grounded)
        {
            if(rb.velocity.magnitude > moveSpeed)
            {
                limitedVel = rb.velocity.normalized * moveSpeed;
                rb.velocity = limitedVel;
            }
        }
        else
        {
            Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            if(horizontalVel.magnitude > moveSpeed)
            {
                limitedVel = horizontalVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
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

    public void SetSpawnPoint()
    {
        if(grounded) spawnPointPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        else
        {
            Physics.Raycast(transform.position, -transform.up, out contactPoint, playerHeight / 2 + groundDistance * 10, surfaceMask);
            spawnPointPosition = contactPoint.point + transform.up * (playerHeight / 2);
        }
        spawnPointRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
    }

    private IEnumerator Slide()
    {
        sliding = true;
        canSlide = false;
        moveSpeed += slideSpeedExtra;
        float timePassed = 0;
        while(timePassed < slideDuration)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }
        moveSpeed -= slideSpeedExtra;
        sliding = false;
        yield return new WaitForSeconds(slideCooldown - slideDuration);
        canSlide = true;
    }
}
