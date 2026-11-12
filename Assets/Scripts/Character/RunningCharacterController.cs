using UnityEngine;
using System;
using System.Collections;

public class RunningCharacterController : MonoBehaviour
{
    [Header("Balance vars")]
    [SerializeField] private float accelerationForce = 20f;
    [SerializeField] private float groundingForce = 0.5f;
    [SerializeField] private float descendingForce = 8f;
    [SerializeField] private float jumpForce = 2f;
    [SerializeField] private float velStartDecel = 25.0f;
    [SerializeField] private float boostForce = 8f;
    [Range(0f, 40f)] 
    [SerializeField] private float minimumSlope = 5f;
    [SerializeField] private float landComboWindow = 3f;
    [SerializeField] private float comboMultiplier = 1.5f;
    [Header("Control vars")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float camHSensitivity = 0.5f;
    [Range(0.1f, 1.0f)]
    [SerializeField] private float camVSensitivity = 0.5f;
    [Range(0.1f, 10f)]
    [SerializeField] private float runThreshold = 5f;
    [Range(0f, 0.5f)]
    [SerializeField] private float coyoteTime = 0.1f;        // For coyote time
    [Range(0f, 1f)]
    [SerializeField] private float airborneResistanceScale = 0.4f;

    protected Rigidbody rb;
    protected Collider col;
    protected Camera camera;
    protected Transform model;
    protected Animator animator;

    public event Action<int, float> goodLanding;                    // Combo Count, landComboWindow
    public event Action comboTimerPause;                            // Call on end of coyote time (we are airborne)
    public event Action comboInterrupt;                             // Fail combo prematurely for some reason
    public event Action jumpEvent;

    private Vector2 movementInput = new Vector2(0, 0);              // Stores horizontal and fwd/back inputs.
    private Vector3 groundNormal = Vector3.up;
    private Vector3 intendedDirection = new Vector3(0, 0, 0);
    private bool isGrounded = false;
    private bool isDescending = false;
    private float currentVelocity = 0f;
    private float timeLastGrounded = 0f;
    private float timeLastGoodLanded = 0f;
    private int consectiveGoodLandings = 0;
    private bool comboOngoing = false;
    private bool jumped = false;
    private bool doubleJumped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        col = this.GetComponent<CapsuleCollider>();
        camera = this.GetComponentInChildren<Camera>();
        model = this.transform.Find("RunnerCharacter");
        animator = model.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AlignVisuals();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        UpdateLinearVelocity();
        UpdateCoyoteTime();
        Move();
        // Apply scaling damping force to reach max velocity.
        ApplySpeedDamping();
        ApplyGroundingForce();
        ApplyDescendingForce();
    }

    public void SubscribeInputs()
    {
        InputManager.instance.HandleRun += UpdateMoveDir;
        InputManager.instance.HandleLook += UpdateLookRotation;
        InputManager.instance.HandleDescend += UpdateDescend;
        InputManager.instance.HandleJump += Jump;
        //Debug.Log("Sub");
    }

    public void OnDisable()
    {
        InputManager.instance.HandleRun -= UpdateMoveDir;
        InputManager.instance.HandleLook -= UpdateLookRotation;
        InputManager.instance.HandleDescend -= UpdateDescend;
        InputManager.instance.HandleJump -= Jump; 
    }

    private void UpdateMoveDir(Vector2 newDir)
    {
        float x = newDir.x;
        float y = newDir.y;

        movementInput = new Vector2(x, y);
    }

    private void UpdateLookRotation(Vector2 dragDir)
    {
        float horizAngleDegree = dragDir.x * camHSensitivity / 10;
        float vertAngleDegree = dragDir.y * camVSensitivity / 50;

        camera.transform.RotateAround(this.transform.position, Vector3.up, horizAngleDegree);
        camera.transform.Rotate(-vertAngleDegree, 0, 0);
    }

    private void UpdateDescend(bool descend)
    {
        isDescending = descend;
    }

    private void Jump()
    {
        if (isGrounded && !jumped)
        {                       // Normal jump check
            jumped = true;
            animator.SetTrigger("JumpTrigger");
            rb.AddForce(groundNormal * jumpForce, ForceMode.Impulse);
            jumpEvent?.Invoke();
        }
        else if(!doubleJumped)
        {                       // Double jump check
            jumped = true;
            doubleJumped = true;
            animator.SetBool("DJFall", true);
            animator.SetTrigger("DoubleJumpTrigger");
            rb.AddForce(groundNormal * jumpForce/2, ForceMode.Impulse);
            rb.AddForce(intendedDirection * jumpForce/2, ForceMode.Impulse);
            jumpEvent?.Invoke();
        }
    }

    private void AlignVisuals()
    {
        // Face direction of User Input
        Quaternion targetLookDir = Quaternion.FromToRotation(Vector3.forward, intendedDirection);
        // Lean to ground
        Quaternion targetLean = Quaternion.FromToRotation(Vector3.up, groundNormal);
        float str = 2f * Time.deltaTime;

        model.transform.rotation = Quaternion.Lerp (model.transform.rotation, targetLean * targetLookDir, str);
        // Multiplying 2 Quaternions is equivalent to addition of both rotations.
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Velocity", currentVelocity);

        if(!isGrounded)
        {
            animator.SetBool("Grounded", false);
        }
        else
        {
            animator.SetBool("Grounded", true);
            animator.SetBool("DJFall", false);
        }

        if (currentVelocity <= runThreshold)
        {
            animator.SetBool("ShouldRun", false);
            animator.SetFloat("AnimationPlaySpeed", 0.5f + (currentVelocity / (2*runThreshold)));
        }
        else
        {
            animator.SetBool("ShouldRun", true);
            animator.SetFloat("AnimationPlaySpeed", 1f + (currentVelocity - runThreshold) / velStartDecel);
        }
    }

    private void UpdateLinearVelocity()
    {
        currentVelocity = rb.linearVelocity.magnitude;
        //Debug.Log(currentVelocity);
    }

    private void UpdateCoyoteTime()
    {
        if(Time.time > timeLastGrounded + coyoteTime && isGrounded)
        {                                   // Only call once
            isGrounded = false;
            if(comboOngoing)                // if we have a combo when we are considered airborne, pause it
            {
                comboTimerPause?.Invoke();
            }
        }
    }

    private void Move()
    {
        Vector3 forceFWD = camera.transform.forward;
        Vector3 forceRGHT = camera.transform.right;
        // Project y such that vectors are parallel to ground contact
        forceFWD.y = 0f;
        forceRGHT.y = 0f;
        forceFWD.Normalize();
        forceRGHT.Normalize();
        Vector3 forceDirFinal = forceFWD * movementInput.y + forceRGHT * movementInput.x;       // Project movement inputs relative to camera rotation
        intendedDirection = forceDirFinal;

        forceDirFinal = AdjustForceToSlope(forceDirFinal);
        rb.AddForce(forceDirFinal * accelerationForce, ForceMode.Acceleration);

        // Debug.Log(forceDirFinal);
    }

    private void ApplySpeedDamping()
    {
        float damping = Mathf.Clamp(2/(1 + Mathf.Exp((velStartDecel - currentVelocity)/5)) -1, 0, 1);
        if(isGrounded)
        {
            rb.linearDamping = damping;
            // Debug.Log(damping);
        }
        else
        {
            rb.linearDamping = damping * airborneResistanceScale;
        }
    }

    private void ApplyGroundingForce()
    {
        if (isGrounded)
        {
            rb.AddForce(-groundNormal * groundingForce, ForceMode.Force);
        }
    }

    private void ApplyDescendingForce()
    {
        if (isDescending && !isGrounded)
        {
            rb.AddForce(Vector3.down * descendingForce, ForceMode.Acceleration);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            groundNormal = collision.GetContact(0).normal;
            timeLastGrounded = Time.time;
            if(isGrounded) return;                      // If we are still considered grounded, we were still in coyote time, or we havent landed
            isGrounded = true;                          // Otherwise, we were airborne and are now grounded
            ResetJumps();
            // Apply speed boost if sufficiently downhill
            CheckGoodLanding();
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            groundNormal = collision.GetContact(0).normal;
            timeLastGrounded = Time.time;
            if(!isGrounded)
            {
                isGrounded = true;
            }
            if(comboOngoing && Time.time > timeLastGoodLanded + landComboWindow)
            {               // if we are still grounded after the window elapses, interrupt combo
                EndCombo();
            }
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            UnGround();
            groundNormal = Vector3.up;
        }
    }

    private void UnGround()
    {
        timeLastGrounded = Time.time;
    }

    private void ResetJumps()
    {
        jumped = false;
        doubleJumped = false;
    }

    private float IsDownhill(Vector3 groundNormal)
    {
        Vector3 downhillComponent = Vector3.ProjectOnPlane(Vector3.down, groundNormal);
        Vector3 horizVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up).normalized;
        return Vector3.Dot(horizVelocity, downhillComponent);
    }

    private void CheckGoodLanding()
    {
        if(Vector3.Angle(Vector3.up, groundNormal) >= minimumSlope)
        {                               // If the slope is steeper than the minimum
            float downhillness = IsDownhill(groundNormal);
            if(downhillness > 0)
            {                                       // Good Landing achieved
                ApplyDownhillBoost(downhillness);
                consectiveGoodLandings++;
                timeLastGoodLanded = Time.time;
                comboOngoing = true;
                goodLanding?.Invoke(consectiveGoodLandings, landComboWindow);
                return;
            }
        }

        // If we reach here, we have not accomplished a good landing.
        if(comboOngoing)
        {
            EndCombo();
        }
    }

    private void EndCombo()
    {
        comboOngoing = false;
        consectiveGoodLandings = 0;
        comboInterrupt?.Invoke();
    }

    private void ApplyDownhillBoost(float downhillness)
    {
        Vector3 boost = intendedDirection * boostForce * Mathf.Pow(comboMultiplier, consectiveGoodLandings) * Mathf.Clamp(downhillness * 2, 0, 1);
        boost = AdjustForceToSlope(boost);
        rb.AddForce(boost, ForceMode.Impulse);
        // Debug.Log(consectiveGoodLandings);
    }

    private Vector3 AdjustForceToSlope(Vector3 force)
    {
        if(groundNormal == Vector3.up)
        {
            return force;
        }

        Quaternion slopeRot = Quaternion.FromToRotation(Vector3.up, groundNormal);
        Vector3 adjustedForce = slopeRot * force;

        // Debug.Log(adjustedForce);
        return adjustedForce;
    }

    public Rigidbody GetRigidbody()
    {
        return rb;
    }
}
