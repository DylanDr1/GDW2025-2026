using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement2 : MonoBehaviour
{
    [Header("Controller Additions")]
    public CharacterController characterController;
    public Transform cam;
    private MovingPlat currentPlatform;

    public Animator animator;

    public string state = "idle";

    //[HideInInspector]
    public bool canMove = true;
    public bool canAttack = true;
    [HideInInspector]
    public bool isAttacking;
    [HideInInspector]
    public bool hanging = false;

    [SerializeField] bool canDash = true;

    [SerializeField] bool canDoubleJump = false;

    [SerializeField]
    private float hangOffset = 2.1f;

    public float speed = 6f;
    public float jump = 12f;
    public float rollSpeed = 8f;
    private bool canDodgeRoll = true;
    public float dashTime;

    [Header("Ground Settings")]
    public Transform groundCheck;
    public float groundDistance = 0.4f; //checks if there is a ground
    public LayerMask groundMask;
    //[HideInInspector]
    public bool isGrounded;

    [Header("Simulated Gravity Settings")]
    public Vector3 transformVelocity; //this applies the gravity and causes the player to fall
    public float gravity = -15.81f; //how heavy gravity gets
    [HideInInspector]
    public float turnSmoothTime = 0.1f; //allows for smoother turning of the model
    float turnSmoothVelocity;

    [Header("Animations")]
    private string currentState;
    public const string PLAYER_IDLE = "CappiStance";
    const string PLAYER_BATTLE_IDLE = "CappiBMStance";
    const string PLAYER_RUN = "CappiWalk";
    const string PLAYER_JUMP = "CappiJumpStart";
    const string PLAYER_FALLING = "CappiJump";
    const string PLAYER_ROLL = "CappiRoll";
    const string PLAYER_AIR_ATTACK = "Player_air_attack";

    [SerializeField] ParticleSystem cloudVFX;

    void Start()
    {
        //animator = GetComponent<Animator>();
    }



    void Update()
    {


        States();
        //Animations();
        //LedgeGrab();



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
    private void States()
    {


        if (state == "idle")
        {
            //ground check
            if (transformVelocity.y < 0)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
                if (isGrounded)
                {
                    canDoubleJump = true;
                }
            }

            if (isGrounded && transformVelocity.y < 0)
            {
                transformVelocity.y = -10f;
            }

            //movement (hanging prevents movement)
            if (!hanging && canDodgeRoll)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");
                ApplyPlatformMovement();
                //handles turning
                Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
                if (canMove)
                {
                    if (horizontal != 0f || vertical != 0f)
                    {
                        state = "walk";
                    }


                }

                if (!hanging)
                {
                    transformVelocity.y += gravity * Time.deltaTime;
                }

                if (canDodgeRoll && canMove)
                {
                    characterController.Move(transformVelocity * Time.deltaTime);
                }


                if (Input.GetButtonDown("Jump") && isGrounded)
                {
                    transformVelocity.y = Mathf.Sqrt(jump * -2f * gravity);
                    isGrounded = false;
                    cloudVFX.Play();
                    state = "jump";
                }
                else if (Input.GetButtonDown("Jump") && hanging && canDodgeRoll)
                {
                    hanging = false;
                    cloudVFX.Play();
                    transformVelocity.y = Mathf.Sqrt(jump * -2f * gravity);
                    state = "jump";
                }
            }
        }
        else if (state == "jump")
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            if (direction.magnitude >= 0.1f)
            {

                float targetAngle = Mathf.Atan2(direction.x, vertical) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                characterController.Move(moveDir.normalized * Time.deltaTime * speed);
            }

            characterController.Move(transformVelocity * Time.deltaTime);
            transformVelocity.y += gravity * Time.deltaTime;

            if (transformVelocity.y < 0)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            }

            if (Input.GetButtonDown("Jump") && !isGrounded && canDoubleJump)
            {
                Debug.Log("DJ");
                transformVelocity.y = Mathf.Sqrt(jump * -2f * gravity);
                canDoubleJump = false;
                cloudVFX.Play();
                state = "jump";
            }

            if (isGrounded && transformVelocity.y < 0)
            {
                transformVelocity.y = -10f;
            }

            if (horizontal == 0 && vertical == 0 && isGrounded && !isAttacking && canAttack && transformVelocity.y < 0)
            {
                cloudVFX.Play();
                state = "idle";

            }
            if (horizontal != 0 && isGrounded || vertical != 0 && isGrounded && transformVelocity.y < 0)
            {
                cloudVFX.Play();
                state = "walk";

            }

            if (!isGrounded && !isAttacking && transformVelocity.y < 0)
            {
                state = "fall";
            }
        }
        else if (state == "fall")
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            if (direction.magnitude >= 0.1f)
            {

                float targetAngle = Mathf.Atan2(direction.x, vertical) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                characterController.Move(moveDir.normalized * Time.deltaTime * speed);
            }

            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            characterController.Move(transformVelocity * Time.deltaTime);
            transformVelocity.y += gravity * Time.deltaTime;

            if (Input.GetButtonDown("Jump") && !isGrounded && canDoubleJump)
            {
                Debug.Log("DJ");
                transformVelocity.y = Mathf.Sqrt(jump * -2f * gravity);
                canDoubleJump = false;
                cloudVFX.Play();
                state = "jump";
            }

            if (horizontal == 0 && vertical == 0 && isGrounded && !isAttacking && canAttack)
            {
                cloudVFX.Play();
                state = "idle";

            }
            if (horizontal != 0 && isGrounded || vertical != 0 && isGrounded)
            {
                cloudVFX.Play();
                state = "walk";

            }


        }
        else if (state == "walk")
        {
            //ground check
            if (transformVelocity.y < 0)
            {
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            }
            if (isGrounded && transformVelocity.y < 0)
            {
                transformVelocity.y = -10f;
            }

            //movement (hanging prevents movement)
            if (!hanging && canDodgeRoll)
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");
                
                //handles turning
                Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
                if (canMove)
                {
                    if (direction.magnitude >= 0.1f)
                    {
                        float targetAngle = Mathf.Atan2(direction.x, vertical) * Mathf.Rad2Deg + cam.eulerAngles.y;
                        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, angle, 0f);

                        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                        characterController.Move(moveDir.normalized * Time.deltaTime * speed);
                    }

                }

                if (horizontal == 0f && vertical == 0f)
                {
                    state = "idle";
                }

                if (!hanging)
                {
                    transformVelocity.y += gravity * Time.deltaTime;
                }

                if (canDodgeRoll && canMove)
                {
                    characterController.Move(transformVelocity * Time.deltaTime);
                }


                if (Input.GetButtonDown("Jump") && isGrounded && canDodgeRoll)
                {
                    transformVelocity.y = Mathf.Sqrt(jump * -2f * gravity);
                    isGrounded = false;
                    cloudVFX.Play();
                    state = "jump";
                }
                else if (Input.GetButtonDown("Jump") && hanging && canDodgeRoll)
                {
                    hanging = false;
                    cloudVFX.Play();
                    transformVelocity.y = Mathf.Sqrt(jump * -2f * gravity);
                    state = "jump";
                }
            }
        }
        else if (state == "hanging")
        {
            if (Input.GetButtonDown("Jump"))
            {
                hanging = false;
                transformVelocity.y = Mathf.Sqrt(jump * -2f * gravity);
                isGrounded = false;
                state = "jump";
            }
        }
        if (!hanging && canDodgeRoll)
        {




            if (state == "walk" && Input.GetMouseButtonDown(1) && isGrounded && canDash)
            {
                canDash = false;
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                characterController.SimpleMove(forward * rollSpeed);
                StartCoroutine(Rolling());
            }
        }




        if (!isGrounded && !isAttacking && transformVelocity.y > 0)
        {
            state = "jump";
        }

        if (!isGrounded && !isAttacking && transformVelocity.y < 0)
        {
            state = "fall";
        }

        if (isGrounded && isAttacking && canDodgeRoll)
        {
            state = "attack ground";
        }
    }

    private IEnumerator Rolling()
    {
        canDash = false;
        canAttack = false;
        canMove = false;
        // animator.SetBool("Rolling", true);
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        float startTime = Time.time;
        while (Time.time < startTime + dashTime)
        {
            characterController.SimpleMove(forward * rollSpeed);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        // animator.SetBool("Rolling", false);
        canMove = true;
        canDash = true;
        canAttack = true;

    }
}