using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{

    Rigidbody rigidBody;

    //Assingables
    [Header("Assingables")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform head;
    [SerializeField] private Collider playerCollider;
    [SerializeField] private GameObject playerHands;
    [SerializeField] private GameObject gunSocket;

    //Rotation and look
    float xRotation;
    const float sensitivity = 50f;

    //Movement
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4500;
    [SerializeField] private float maxSpeed = 20;
    [SerializeField] private bool enableSprint = false;
    [SerializeField] private float sprintMultiplicker = 1.2f;

    //[SerializeField] private float extraGravity;

    [SerializeField] private float friction = 0.175f;
    [SerializeField] [Range(0,180)] private float maxSlopeAngle = 35f;

    [SerializeField] private LayerMask whatIsGround;

    private bool grounded;
    private float distToGround;

    //Crouch & Slide
    [Header("Crouch and Slide")]
    [SerializeField] private float slideForce = 400;
    [SerializeField] private float slideFriction = 0.2f;
    [SerializeField] private float startVelSlide = 3f;
    [SerializeField] private float crouchSpeed;

    Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    Vector3 playerScale;


    //Jumping
    [Header("Jumping")]
    [SerializeField] private bool enableAutoJump = true;
    [SerializeField] private float jumpForce = 550f;

    [Header("Head")]
    [SerializeField] private float yVelToEffect=10f;
    [SerializeField] private float downTime=0.2f;
    [SerializeField] private float upTime=10f;
    [SerializeField] private float handsMultiplicker = 0.1f;
    [SerializeField] private float gunSocketMultiplicker = 0.1f;
    private float yRbVel;
    Vector3 headPosition;
    Vector3 handsPosition;
    Vector3 gunSocketPosition;

    //Input
    Vector2 inputDirection = new Vector2();
    bool crouching;

    //Sliding
    Vector3 normalVector = Vector3.up;

    //Particles
    [Header("Particle")]
    [SerializeField] private GameObject landParticleObject;
    [SerializeField] private float startVelocity = 3f;
    ParticleSystem landParticle;


    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        landParticle = landParticleObject.GetComponent<ParticleSystem>();
    }

    void Start()
    {
        headPosition = head.localPosition;
        handsPosition = playerHands.transform.localPosition;
        gunSocketPosition = gunSocket.transform.localPosition;
        playerScale = playerCollider.gameObject.transform.localScale;
        LockCursor();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        Movement();
    }

    void Update()
    {
        MyInput();
        Look();
        GunSocketTransform();
        yRbVel = rigidBody.velocity.y;
    }

    /// <summary>
    /// Find user input. Should put this in its own class but im lazy
    /// </summary>
    void MyInput()
    {
        inputDirection.x = Input.GetAxisRaw("Horizontal");
        inputDirection.y = Input.GetAxisRaw("Vertical");
        inputDirection.Normalize();

        if ((Input.GetButtonDown("Jump")&&!enableAutoJump)||(Input.GetButton("Jump") && enableAutoJump))
        {
            Jump();
        }

        //Crouching
        if (Input.GetButtonDown("Crouch"))
            StartCrouch();
        if (Input.GetButtonUp("Crouch"))
            StopCrouch();
    }

    void StartCrouch()
    {
        crouching = true;
        // squash the player
        //playerCollider.gameObject.transform.localScale = crouchScale;
        StartCoroutine("PlayerChangeScale", crouchScale);
        // move them up
        //transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        // if player is moving at a fast enough speed
        if (rigidBody.velocity.magnitude > startVelSlide)
        {
            // and on the ground
            if (grounded)
            {
                // slide boost forward
                rigidBody.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    void StopCrouch()
    {
        crouching = false;
        // reset scale
        //playerCollider.gameObject.transform.localScale = playerScale;
        StartCoroutine("PlayerChangeScale", playerScale);
        // move down
        //transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    IEnumerator PlayerChangeScale(Vector3 targetScale)
    {
        for (float t = 0.01f; t < crouchSpeed; t += 2 * Time.deltaTime)
        {
            playerCollider.gameObject.transform.localScale = Vector3.Lerp(playerCollider.gameObject.transform.localScale, targetScale, (t / crouchSpeed));
            yield return null;
        }
    }

    void Movement()
    {
        #region UselessComment
        //Debug.Log(rigidBody.velocity.magnitude);

        // Extra gravity
        //rigidBody.AddForce(Vector3.down * Time.deltaTime * extraGravity);
        #endregion

        ApplyFriction();

        //Set max speed
        float maxSpeed = this.maxSpeed - 5f;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded)
        {
            rigidBody.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;
        #region UselessComment
        //float multiplier = 1f - (Mathf.Abs(rigidBody.velocity.x) + Mathf.Abs(rigidBody.velocity.z)) / maxSpeed, multiplierV = 1f - (Mathf.Abs(rigidBody.velocity.x) + Mathf.Abs(rigidBody.velocity.z)) / maxSpeed;
        //// Movement in air
        //if (!grounded)
        //{
        //    multiplier = 0.5f;
        //    multiplierV = 0.5f;
        //}
        #endregion

        if (Input.GetButton("Sprint") && enableSprint)
        {
            multiplier = sprintMultiplicker; multiplierV = sprintMultiplicker;
        }

        // Movement while sliding
        if (grounded && crouching) multiplierV = 0f;


        //Apply forces to move player
        // more easily adjust left/right while in the air than forward/back
        rigidBody.AddForce(orientation.transform.right * inputDirection.x * moveSpeed * Time.deltaTime * multiplier);
        rigidBody.AddForce(orientation.transform.forward * inputDirection.y * moveSpeed * Time.deltaTime * multiplier * multiplierV);

        if (new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).magnitude > maxSpeed)
        {
            #region UselessComment
            //rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
            //Debug.Log(new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z) * ((Mathf.Abs(rigidBody.velocity.x) + Mathf.Abs(rigidBody.velocity.z)) - maxSpeed));
            //Debug.Log(new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).normalized);
            //Debug.Log(new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).normalized *
            //    Mathf.Min((Mathf.Abs(rigidBody.velocity.x) + Mathf.Abs(rigidBody.velocity.z)) - maxSpeed, inputDirection.magnitude * moveSpeed * Time.deltaTime * multiplier * multiplierV) *
            //    new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).magnitude);
            //Mathf.Min(new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).magnitude - maxSpeed, moveSpeed * Time.deltaTime * multiplier * multiplierV)
            #endregion
            rigidBody.AddForce(new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).normalized *
                Mathf.Min(new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).magnitude - maxSpeed, moveSpeed * Time.deltaTime * multiplier * multiplierV) * maxSpeed);
        }
    }

    void Jump()
    {
        if (grounded)
        {
            //Add jump forces
            grounded = false;
            rigidBody.AddForce(normalVector * jumpForce);
        }
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime;

        //Find current look rotation
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        float desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Perform the rotations
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    void ApplyFriction()
    {
        if (!grounded || Input.GetButton("Jump")) return;

        //Slow down sliding
        if (crouching)
        {
            rigidBody.AddForce(moveSpeed * Time.deltaTime * -rigidBody.velocity.normalized * slideFriction);
            return;
        }

        Vector3 inverseVelocity = -orientation.InverseTransformDirection(rigidBody.velocity);

        if (inputDirection.x == 0)
        {
            rigidBody.AddForce(inverseVelocity.x * orientation.transform.right * moveSpeed * friction * Time.deltaTime);
        }
        if (inputDirection.y == 0)
        {
            rigidBody.AddForce(inverseVelocity.z * orientation.transform.forward * moveSpeed * friction * Time.deltaTime);
        }
    }

    IEnumerator LandShake(float velocity)
    {
        //Debug.Log(Mathf.Min(Mathf.Abs(velocity + yVelToEffect)/20, 0.7f));
        float yPos = Mathf.Min(Mathf.Abs(velocity + yVelToEffect)/20, 0.7f);

        if (yPos * 10 > startVelocity)
        {
            landParticle.Play();
        }
        for(float t = 0.01f; t<downTime; t += 1 * Time.deltaTime)
        {
            if(playerHands != null) playerHands.transform.localPosition = Vector3.Lerp(playerHands.transform.localPosition, new Vector3(handsPosition.x, handsPosition.y - yPos * handsMultiplicker, handsPosition.z), (t / downTime));
            if(gunSocket != null) gunSocket.transform.localPosition = Vector3.Lerp(gunSocket.transform.localPosition, new Vector3(gunSocket.transform.localPosition.x, gunSocketPosition.y - yPos * gunSocketMultiplicker, gunSocket.transform.localPosition.z), (t / downTime));
            head.localPosition = Vector3.Lerp(head.localPosition, new Vector3(0,headPosition.y - yPos, 0), (t/downTime));
            yield return null;
        }
        for (float t = 0.01f; t < upTime; t += 1 * Time.deltaTime)
        {
            if (playerHands != null) playerHands.transform.localPosition = Vector3.Lerp(playerHands.transform.localPosition, handsPosition, (t / upTime));
            if (gunSocket != null) gunSocket.transform.localPosition = Vector3.Lerp(gunSocket.transform.localPosition, new Vector3(gunSocket.transform.localPosition.x, gunSocketPosition.y, gunSocket.transform.localPosition.z), (t / upTime));
            head.localPosition = Vector3.Lerp(head.localPosition, headPosition, (t/upTime));
            if (Vector3.Distance(headPosition, head.localPosition) < 0.0001f) { head.localPosition = headPosition;  break; }
            yield return null;
        }
    }

    private void GunSocketTransform()
    {
        Vector3 targetPos = new Vector3(Mathf.Max(inputDirection.x,0.1f) * rigidBody.velocity.magnitude / 10f, rigidBody.velocity.y/20f , Mathf.Max(inputDirection.y, 0.1f) * rigidBody.velocity.magnitude / 10f);
        gunSocket.transform.localPosition = Vector3.Lerp(gunSocket.transform.localPosition, gunSocketPosition - targetPos / 30f, 2f * Time.deltaTime);
    }

    bool IsFloorAngle(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position -Vector3.up * (distToGround+0.01f));
    }

    /// <summary>
    /// Handle ground detection
    /// </summary>
    void OnCollisionEnter(Collision other)
    {
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloorAngle(normal))
            {
                bool prevGrounded = grounded;
                grounded = true;
                if (!prevGrounded && grounded && yRbVel < -yVelToEffect) { StopCoroutine("LandShake"); StartCoroutine(LandShake(yRbVel));}
                normalVector = normal;
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        distToGround = playerCollider.bounds.extents.y;
        grounded = Physics.Raycast(transform.position, -Vector3.up, distToGround+0.01f,whatIsGround);
    }
}