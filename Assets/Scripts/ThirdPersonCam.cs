using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Player references")]
    [SerializeField] private Transform orientationForward;
    [SerializeField] private Transform orientationRight;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerObj;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform FollowDamper;

    [Header("Camera settings")]
    [SerializeField] private float rotationSpeed;

    //Non-serialized
    private CinemachineFreeLook vcam;
    private PlayerMovement playerMovement;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        vcam = GetComponent<CinemachineFreeLook>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        // rotate orientation
        Vector3 viewDir = player.position - transform.position;
        orientationForward.forward = viewDir.normalized;
        orientationRight.right = -Vector3.Cross(orientationForward.forward, player.up);
        orientationForward.forward = Vector3.Cross(orientationRight.right, player.up);


        // roate player object
        if(playerMovement.canMove)
        {
            vcam.m_XAxis.m_InputAxisName = "Mouse X";
            vcam.m_YAxis.m_InputAxisName = "Mouse Y";
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir;
            FollowDamper.transform.position = player.position;
            inputDir = orientationForward.forward * verticalInput + orientationRight.right * horizontalInput;
            if (inputDir != Vector3.zero)
            {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
                playerObj.transform.localRotation = Quaternion.Euler(0, playerObj.localEulerAngles.y, 0);
                vcam.m_RecenterToTargetHeading.m_enabled = true;
                FollowDamper.transform.up = Vector3.Slerp(FollowDamper.transform.up, player.up, Time.deltaTime * rotationSpeed / 4);
            }
            else vcam.m_RecenterToTargetHeading.m_enabled = false;
        }
        else
        {
            vcam.m_XAxis.m_InputAxisName = "";
            vcam.m_YAxis.m_InputAxisName = "";
        }
    }
}