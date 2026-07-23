using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BusController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float motorForce = 3000f;
    public float brakeForce = 5000f;
    public float maxSpeed = 40f;
    public float reverseSpeed = 10f;

    [Header("Steering Settings")]
    public float maxSteerAngle = 35f;
    public float steerSpeed = 10f;

    [Header("Boost Settings")]
    public float boostMultiplier = 1.5f;
    public float boostDuration = 3f;
    public float boostCooldown = 5f;

    [Header("Drift Settings")]
    public float driftSteerMultiplier = 1.5f;
    public float normalForwardTraction = 1f;
    public float normalSidewaysTraction = 1f;
    public float driftForwardTraction = 0.8f;
    public float driftSidewaysTraction = 0.4f;

    [Header("Effects References")]
    public ParticleSystem boostTrail;
    public AudioSource boostAudio;
    public AudioSource engineAudio;
    public ParticleSystem driftSmokeL, driftSmokeR;
    public AudioSource windAudio, turboAudio;

    [Header("Wheel Colliders")]
    public WheelCollider wheelColliderFL;
    public WheelCollider wheelColliderFR;
    public WheelCollider wheelColliderRL;
    public WheelCollider wheelColliderRR;

    [Header("Wheel Visual Transforms")]
    public Transform wheelVisualFL;
    public Transform wheelVisualFR;
    public Transform wheelVisualRL;
    public Transform wheelVisualRR;

    [Header("Camera Reference Settings")]
    public Transform cameraTarget;

    [Header("Debug Settings")]
    public bool debugMode = true;

    private Rigidbody rb;
    private float currentSteerAngle;
    private float currentBoostTime;
    private float currentBoostCooldown;
    private bool isBoosting;
    private bool isDrifting;
    private float speed;

    // Store original friction values
    private WheelFrictionCurve originalForwardFrictionRL;
    private WheelFrictionCurve originalSidewaysFrictionRL;
    private WheelFrictionCurve originalForwardFrictionRR;
    private WheelFrictionCurve originalSidewaysFrictionRR;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -2f, 0f);
        
        // Validate wheel colliders
        ValidateWheelColliders();
        
        // Store original friction curves
        StoreOriginalFriction();
    }

    void ValidateWheelColliders()
    {
        if (wheelColliderFL == null || wheelColliderFR == null || 
            wheelColliderRL == null || wheelColliderRR == null)
        {
            Debug.LogError("BusController: All WheelColliders must be assigned!", this);
            enabled = false;
        }
    }

    void StoreOriginalFriction()
    {
        if (wheelColliderRL != null)
        {
            originalForwardFrictionRL = wheelColliderRL.forwardFriction;
            originalSidewaysFrictionRL = wheelColliderRL.sidewaysFriction;
        }
        if (wheelColliderRR != null)
        {
            originalForwardFrictionRR = wheelColliderRR.forwardFriction;
            originalSidewaysFrictionRR = wheelColliderRR.sidewaysFriction;
        }
    }

    void Update()
    {
        speed = rb.linearVelocity.magnitude;
        HandleBoostInput();
        HandleDriftInput();
        UpdateWheelVisuals();
        UpdateAudio();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        LimitSpeed();
        UpdateCameraTarget();
    }

    void OnGUI()
    {
        if (!debugMode) return;

        GUILayout.BeginArea(new Rect(10, 10, 350, 320), GUI.skin.box);

        GUILayout.Label("=== BUS DEBUG ===");
        GUILayout.Label("Speed: " + speed.ToString("F2") + " m/s");
        GUILayout.Label("Speed: " + (speed * 3.6f).ToString("F1") + " km/h");
        GUILayout.Label("Velocity: " + rb.linearVelocity.ToString("F2"));
        GUILayout.Label("Steer Angle: " + currentSteerAngle.ToString("F1") + "°");
        GUILayout.Label("Boosting: " + isBoosting);
        GUILayout.Label("Drifting: " + isDrifting);
        GUILayout.Label("Vertical Input: " + Input.GetAxis("Vertical").ToString("F2"));
        GUILayout.Label("Horizontal Input: " + Input.GetAxis("Horizontal").ToString("F2"));
        GUILayout.Label("Mass: " + rb.mass);
        GUILayout.Label("RPM FL: " + (wheelColliderFL != null ? wheelColliderFL.rpm.ToString("F0") : "N/A"));
        GUILayout.Label("RPM RR: " + (wheelColliderRR != null ? wheelColliderRR.rpm.ToString("F0") : "N/A"));
        GUILayout.Label("Is Grounded: " + IsGrounded());

        GUILayout.EndArea();
    }

    bool IsGrounded()
    {
        bool grounded = false;
        if (wheelColliderFL != null && wheelColliderFL.isGrounded) grounded = true;
        if (wheelColliderFR != null && wheelColliderFR.isGrounded) grounded = true;
        if (wheelColliderRL != null && wheelColliderRL.isGrounded) grounded = true;
        if (wheelColliderRR != null && wheelColliderRR.isGrounded) grounded = true;
        return grounded;
    }

    void HandleMotor()
    {
        if (!IsGrounded()) return;

        float motorInput = Input.GetAxis("Vertical");
        float currentMotorForce = motorInput * motorForce;

        // Apply boost multiplier
        if (isBoosting)
        {
            currentMotorForce *= boostMultiplier;
        }

        // Check if braking (reverse input while moving forward)
        bool isBraking = motorInput < 0f && speed > reverseSpeed;

        // Apply to rear wheels (RWD)
        ApplyMotorToWheel(wheelColliderRL, currentMotorForce, isBraking);
        ApplyMotorToWheel(wheelColliderRR, currentMotorForce, isBraking);
    }

    void ApplyMotorToWheel(WheelCollider wheel, float motorTorque, bool isBraking)
    {
        if (wheel == null) return;

        if (isBraking)
        {
            wheel.motorTorque = 0f;
            wheel.brakeTorque = brakeForce;
        }
        else
        {
            wheel.brakeTorque = 0f;
            wheel.motorTorque = motorTorque;
        }
    }

    void HandleSteering()
    {
        float steerInput = Input.GetAxis("Horizontal");
        float targetSteer = steerInput * maxSteerAngle;

        // Reduce steering sensitivity at high speeds
        float speedFactor = Mathf.Clamp01(1f - (speed / maxSpeed) * 0.6f);
        targetSteer *= speedFactor;

        // Increase steering angle during drift
        if (isDrifting)
        {
            targetSteer *= driftSteerMultiplier;
        }

        // Smooth steering
        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteer, steerSpeed * Time.fixedDeltaTime);

        // Apply to front wheels
        if (wheelColliderFL != null)
        {
            wheelColliderFL.steerAngle = currentSteerAngle;
        }
        if (wheelColliderFR != null)
        {
            wheelColliderFR.steerAngle = currentSteerAngle;
        }
    }

    void LimitSpeed()
    {
        float currentMax = isBoosting ? maxSpeed * boostMultiplier : maxSpeed;
        
        if (rb.linearVelocity.magnitude > currentMax)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMax;
        }
    }

    void UpdateWheelVisuals()
    {
        UpdateSingleWheelVisual(wheelColliderFL, wheelVisualFL);
        UpdateSingleWheelVisual(wheelColliderFR, wheelVisualFR);
        UpdateSingleWheelVisual(wheelColliderRL, wheelVisualRL);
        UpdateSingleWheelVisual(wheelColliderRR, wheelVisualRR);
    }

    void UpdateSingleWheelVisual(WheelCollider collider, Transform visual)
    {
        if (collider == null || visual == null) return;

        Vector3 position;
        Quaternion rotation;
        
        // GetWorldPose returns the wheel's world position and rotation
        collider.GetWorldPose(out position, out rotation);
        
        visual.position = position;
        visual.rotation = rotation;
    }

    void UpdateCameraTarget()
    {
        if (cameraTarget == null) return;
        
        Vector3 lookAhead = transform.forward * speed * 0.3f;
        Vector3 targetPosition = transform.position + lookAhead + new Vector3(0f, 2f, -5f);
        cameraTarget.position = Vector3.Lerp(cameraTarget.position, targetPosition, Time.deltaTime * 5f);
    }

    void HandleBoostInput()
    {
        // Check for boost activation
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftCommand)) 
            && currentBoostCooldown <= 0f && !isBoosting)
        {
            ActivateBoost();
        }

        // Handle boost duration
        if (isBoosting)
        {
            currentBoostTime -= Time.deltaTime;
            if (currentBoostTime <= 0f)
            {
                DeactivateBoost();
            }
        }
        else
        {
            // Handle cooldown
            if (currentBoostCooldown > 0f)
            {
                currentBoostCooldown -= Time.deltaTime;
            }
        }
    }

    void ActivateBoost()
    {
        isBoosting = true;
        currentBoostTime = boostDuration;
        currentBoostCooldown = boostCooldown;

        if (boostTrail != null)
        {
            boostTrail.Play();
        }
        if (boostAudio != null)
        {
            boostAudio.Play();
        }
    }

    void DeactivateBoost()
    {
        isBoosting = false;

        if (boostTrail != null)
        {
            boostTrail.Stop();
        }
        if (boostAudio != null)
        {
            boostAudio.Stop();
        }
    }

    void HandleDriftInput()
    {
        isDrifting = Input.GetKey(KeyCode.Space);

        // Apply different traction based on drift state
        SetDriftTraction(wheelColliderRL, isDrifting);
        SetDriftTraction(wheelColliderRR, isDrifting);

        // Handle drift smoke effects
        HandleDriftSmoke();
    }

    void SetDriftTraction(WheelCollider wheel, bool drifting)
    {
        if (wheel == null) return;

        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

        if (drifting)
        {
            forwardFriction.stiffness = driftForwardTraction;
            sidewaysFriction.stiffness = driftSidewaysTraction;
        }
        else
        {
            forwardFriction.stiffness = normalForwardTraction;
            sidewaysFriction.stiffness = normalSidewaysTraction;
        }

        wheel.forwardFriction = forwardFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    void HandleDriftSmoke()
    {
        // Only emit smoke when actually sliding (speed > threshold and drifting)
        bool shouldEmit = isDrifting && speed > 10f;

        if (driftSmokeL != null)
        {
            if (shouldEmit && !driftSmokeL.isPlaying)
            {
                driftSmokeL.Play();
            }
            else if (!shouldEmit && driftSmokeL.isPlaying)
            {
                driftSmokeL.Stop();
            }
        }

        if (driftSmokeR != null)
        {
            if (shouldEmit && !driftSmokeR.isPlaying)
            {
                driftSmokeR.Play();
            }
            else if (!shouldEmit && driftSmokeR.isPlaying)
            {
                driftSmokeR.Stop();
            }
        }
    }

    void UpdateAudio()
    {
        if (engineAudio == null) return;

        float speedRatio = Mathf.Clamp01(speed / maxSpeed);
        
        // Calculate target pitch based on speed
        float targetPitch = 0.5f + speedRatio * 1.2f;
        
        // Increase pitch during boost
        if (isBoosting)
        {
            targetPitch += 0.3f;
        }

        // Apply pitch and volume
        engineAudio.pitch = Mathf.Clamp(targetPitch, 0.5f, 2f);
        engineAudio.volume = Mathf.Lerp(0.4f, 1f, speedRatio);

        // Wind audio - only audible at higher speeds
        if (windAudio != null)
        {
            float windVolume = Mathf.Clamp01((speed - 40f) / 40f);
            windAudio.volume = windVolume;
            windAudio.pitch = 0.8f + speedRatio * 0.4f;
        }

        // Turbo audio - only during boost at high speed
        if (turboAudio != null)
        {
            bool shouldPlayTurbo = speed > 60f && isBoosting;
            
            if (shouldPlayTurbo && !turboAudio.isPlaying)
            {
                turboAudio.Play();
            }
            else if (!shouldPlayTurbo && turboAudio.isPlaying)
            {
                turboAudio.Stop();
            }
        }
    }

    // Reset friction values when disabled
    void OnDisable()
    {
        if (wheelColliderRL != null)
        {
            wheelColliderRL.forwardFriction = originalForwardFrictionRL;
            wheelColliderRL.sidewaysFriction = originalSidewaysFrictionRL;
        }
        if (wheelColliderRR != null)
        {
            wheelColliderRR.forwardFriction = originalForwardFrictionRR;
            wheelColliderRR.sidewaysFriction = originalSidewaysFrictionRR;
        }
    }
}