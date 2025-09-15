using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Camera playerCamera;
    [SerializeField] float speed = 6f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float gravity = 10f;
    [SerializeField] float cameraSensitivity = 2f;
    [SerializeField] float cameraPitchLimit = 90f;

    private Vector3 moveDirection = Vector3.zero;
    private float cameraRotationX = 0;
    private bool canMove = true;

    private CharacterController characterController;
    //public string GameEndedName;

   
    private MovingPlat currentPlatform;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleCameraRotation();
    }

    private void HandleMovement()
    {
        Vector3 forwardMovement = transform.TransformDirection(Vector3.forward);
        Vector3 rightMovement = transform.TransformDirection(Vector3.right);

        float currentSpeedX = canMove ? speed * Input.GetAxis("Vertical") : 0;
        float currentSpeedZ = canMove ? speed * Input.GetAxis("Horizontal") : 0;

        moveDirection.x = (forwardMovement * currentSpeedX).x + (rightMovement * currentSpeedZ).x;
        moveDirection.z = (forwardMovement * currentSpeedX).z + (rightMovement * currentSpeedZ).z;

        if (characterController.isGrounded)
        {
            moveDirection.y = -2f; 

            if (Input.GetButton("Jump") && canMove)
            {
                moveDirection.y = jumpForce;
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

     
        ApplyPlatformMovement();

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void ApplyPlatformMovement()
    {
        currentPlatform = null;

        
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, characterController.height / 2f + 0.3f))
        {
            MovingPlat plat = hit.collider.GetComponent<MovingPlat>();
            if (plat != null)
            {
               
                if (Vector3.Angle(hit.normal, Vector3.up) <= characterController.slopeLimit)
                {
                    currentPlatform = plat;
                }
            }
        }

        if (currentPlatform != null && currentPlatform.Delta != Vector3.zero)
        {
           
            characterController.Move(currentPlatform.Delta);
        }
    }

    private void HandleCameraRotation()
    {
        cameraRotationX += -Input.GetAxis("Mouse Y") * cameraSensitivity;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -cameraPitchLimit, cameraPitchLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * cameraSensitivity, 0);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Death")
        {
           // SceneManager.LoadScene(GameEndedName);
        }
    }
}
