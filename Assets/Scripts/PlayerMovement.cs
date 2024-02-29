using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private Transform orientation;
    [SerializeField] private float groundDrag;

    [Header ("Ground check")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float playerHeight;
    [SerializeField] private float groundDistance;
    [SerializeField] private bool grounded; //Serialized to help testing

    //Non-serialized
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        CheckGround();
        GetInput();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void Move()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void CheckGround()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + groundDistance, groundMask);
        if(grounded) rb.drag = groundDrag;
        else rb.drag = 0;
    }
}
