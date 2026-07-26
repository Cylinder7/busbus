using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BusController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float motorForce = 2800f;
    public float brakeForce = 4500f;
    public float maxSpeed = 22f;
    public float reverseSpeed = 8f;
    public float extraDriveForce = 4000f;
    public float boostDriveForceMultiplier = 1.1f;

    [Header("Damage Settings")]
    public float damageCapacity = 100f;
    public float durability = 10f;

    [Header("Steering Settings")]
    public float maxSteerAngle = 35f;
    public float steerSpeed = 10f;

    [Header("Boost Settings")]
    public float boostMultiplier = 1.25f;
    public float boostDuration = 3f;
    public float boostCooldown = 5f;

    [Header("Trick Settings")]
    public float trickBoostMultiplier = 1.5f;
    public float trickingSpeed = 1f;
    public float trickDuration = 1f;
    public float trickRotationSpeed = 360f;

    [Header("Drift Settings")]
    public float driftSteerMultiplier = 1.5f;
    public float normalForwardTraction = 1f;
    public float normalSidewaysTraction = 1f;
    public float driftForwardTraction = 0.8f;
    public float driftSidewaysTraction = 0.4f;

    [Header("Effects References")]
    public ParticleSystem boostTrail;
    
    public ParticleSystem driftSmokeL, driftSmokeR;

    [Header("Sounds Refrences")]
    public AudioSource boostAudio;
    public AudioSource engineAudio;
    public AudioSource turboAudio;
    public AudioSource brakingAudio;
    public AudioSource driftAudio;
    public AudioSource deathAudio;
    public AudioSource lightDamageAudio;
    public AudioSource mediumDamageAudio;
    public AudioSource heavyDamageAudio;
    public AudioSource landSuccessAudio;
    public AudioSource trick1Audio;
    public AudioSource trick2Audio;
    public AudioSource trick3Audio;
    public AudioSource trick4Audio;
    public AudioSource trick5Audio;
    public AudioSource trick6Audio;
    public AudioSource fullTrickAudio;

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
    private float damage;
    private readonly List<float> speedHistory = new List<float>();
    private const int maxSpeedHistory = 20;
    private int trickCombo = 0;
    private bool isTricking;
    private bool isTrickBoost;
    private Coroutine trickCoroutine;

    private Vector3 OriginalPosition;
    private Quaternion OriginalRotation;

    // Store original friction values
    private WheelFrictionCurve originalForwardFrictionRL;
    private WheelFrictionCurve originalSidewaysFrictionRL;
    private WheelFrictionCurve originalForwardFrictionRR;
    private WheelFrictionCurve originalSidewaysFrictionRR;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -2f, 0f);
        ValidateWheelColliders();
        StoreOriginalFriction();
        OriginalPosition = rb.transform.position;
        OriginalRotation = rb.transform.rotation;
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
        HandleReset();
        CheckForCrash();
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        LimitSpeed();
        UpdateCameraTarget();
        HandleTricking();
    }

    void OnGUI()
    {
        if (!debugMode) return;

        GUILayout.BeginArea(new Rect(10, 12, 350, 420), GUI.skin.box);

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
        GUILayout.Label("Speed History Samples: " + speedHistory.Count);

        int startIndex = Mathf.Max(0, speedHistory.Count - 8);
        for (int i = startIndex; i < speedHistory.Count; i++)
        {
            GUILayout.Label("[" + i + "] " + speedHistory[i].ToString("F2") + " m/s");
        }

        if (speedHistory.Count == 0)
        {
            GUILayout.Label("(no samples yet)");
        }

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

    if (isBoosting)
        currentMotorForce *= boostMultiplier + (trickCombo/10);

    // Braking: reverse input while moving forward at any speed
    bool isMovingForward = Vector3.Dot(rb.linearVelocity, transform.forward) > 0.5f;
    bool isBraking = motorInput < 0f && isMovingForward;

    ApplyMotorToWheel(wheelColliderRL, currentMotorForce, isBraking);
    ApplyMotorToWheel(wheelColliderRR, currentMotorForce, isBraking);

    // Also brake front wheels for stronger deceleration
    if (wheelColliderFL != null)
        wheelColliderFL.brakeTorque = isBraking ? brakeForce : 0f;
    if (wheelColliderFR != null)
        wheelColliderFR.brakeTorque = isBraking ? brakeForce : 0f;

    if (motorInput > 0f && !isBraking)
    {
        float currentMaxSpeed = maxSpeed * (isBoosting ? boostMultiplier : 1f);
        float speedFactor = Mathf.Clamp01(1f - (speed / (currentMaxSpeed * 1.05f)));
        float forwardForce = extraDriveForce * speedFactor * (isBoosting ? boostDriveForceMultiplier : 0.7f);

        rb.AddForce(transform.forward * forwardForce, ForceMode.Acceleration);
    }
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
    void HandleReset()
    {
        if(Input.GetKey(KeyCode.R))
        {
            rb.transform.SetPositionAndRotation(OriginalPosition, OriginalRotation);
            rb.linearVelocity = Vector3.zero;
        }
    }

    void CheckForCrash()
    {
        if (speedHistory.Count >= maxSpeedHistory)
        {
            speedHistory.RemoveAt(0);
        }

        speedHistory.Add(speed);
    }

    IEnumerator PerformTrick(Vector3 axis)
    {
        isTricking = true;
        bool startedBoost = false;

        if (!isBoosting)
        {
            ActivateBoost();
            isTrickBoost = true;
            startedBoost = true;
        }
        else
        {
            isTrickBoost = false;
        }

        float totalRotation = 0f;
        while (totalRotation < 450f)
        {
            float deltaRotation = trickRotationSpeed * Time.deltaTime;
            if (totalRotation + deltaRotation > 450f)
            {
                deltaRotation = 450f - totalRotation;
            }

            transform.Rotate(axis * deltaRotation, Space.Self);
            totalRotation += deltaRotation;
            yield return null;
        }

        isTricking = false;

        if (isTrickBoost && startedBoost)
        {
            isTrickBoost = false;
            DeactivateBoost();
        }
    }

    void StartTrick(Vector3 axis)
    {
        if (isTricking) return;

        if (trickCoroutine != null)
        {
            StopCoroutine(trickCoroutine);
            trickCoroutine = null;
        }

        trickCoroutine = StartCoroutine(PerformTrick(axis));
    }

    void HandleTricking()
    {
        if(IsGrounded())
        {
            return;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                // rotate bus 360 on its local z axis
                StartTrick(Vector3.forward);
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                // rotate bus 360 on its local y axis
                StartTrick(Vector3.up);
            }
        }        
    }
}