using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BusController : MonoBehaviour
{

    [Header("Movement Settings")]
    public float motorForce;
    public float brakeForce;
    public float maxSpeed;
    public float reverseSpeed;

    [Header("Steering Settings")]
    public float steerAngle;
    public float steerSpeed;

    [Header("Boost Settings")]
    public float boostMultiplier;
    public float boostDuration;
    public float boostCooldown;

    [Header("Drift Settings")]
    public float driftSteerMultiplier;
    public float tractionMultiplier;

    [Header("Effects References")]
    public ParticleSystem boostTrail;
    public AudioSource boostAudio;
    public AudioSource engineAudio;
    public ParticleSystem driftSmokeL, driftSmokeR;
    public AudioSource windAudio,turboAudio;

    [Header("Wheel References")]
    public Transform wheelFL;
    public Transform wheelFR;
    public Transform wheelRL;
    public Transform wheelRR;

    [Header("Camera Reference Settings")]
    public Transform cameraTarget;

    private Rigidbody rb;
    private float currentSteerAngle;
    private float currentBoostTime;
    private float currentBoostCooldown;
    private bool isBoosting;
    private bool isDrifting;
    private float speed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    // Update is called once per frame
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
    
    void HandleMotor()
    {
        float motorInput = Input.GetAxis("Vertical");
        float motorTorque = motorInput * motorForce;

        if (isBoosting)
        {
            motorTorque *= boostMultiplier;
        }

        if (motorInput < 0 && speed > reverseSpeed)
        {
            motorTorque = Mathf.Max(motorTorque, -brakeForce);
        }
        rb.AddForce(transform.forward * motorTorque, ForceMode.Force);
    }

    void HandleSteering()
    {
        float steerInput = Input.GetAxis("Horizontal");
        float targetSteer = steerInput * steerAngle;

        float speedFactor = Mathf.Clamp01(1f - (speed / maxSpeed)*0.6f);
        targetSteer *= speedFactor;

        if (isDrifting)
        {
            targetSteer *= driftSteerMultiplier;
        }

        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteer, steerSpeed * Time.fixedDeltaTime);
        rb.AddTorque(Vector3.up * currentSteerAngle, ForceMode.Force); 
    }    
    void LimitSpeed()
    {
        float currentMax =  isBoosting ? maxSpeed * boostMultiplier : maxSpeed;
        if (rb.linearVelocity.magnitude > currentMax)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMax;
        }

    }

    void UpdateWheelVisuals()
    {
        if (wheelFL == null){return;}
        if (wheelFR == null){return;}
        if (wheelRL == null){return;}
        if (wheelRR == null){return;}

        float distencePerFrame = speed * Time.deltaTime;
        float wheelRadius = wheelFL.localScale.y / 2f;
        float rotationDegrees = (distencePerFrame / (2 * Mathf.PI * wheelRadius)) * 360f;
        Quaternion rollRotation = Quaternion.Euler(rotationDegrees, 0f, 0f);

        wheelFL.localRotation *= rollRotation;
        wheelFR.localRotation *= rollRotation;
        wheelRL.localRotation *= rollRotation;
        wheelRR.localRotation *= rollRotation;

        Quaternion steerRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
        wheelFL.localRotation = steerRotation * wheelFL.localRotation;
        wheelFR.localRotation = steerRotation * wheelFR.localRotation;
    }

    void UpdateCameraTarget()
    {
        if (cameraTarget == null) { return; }
        Vector3 lookAhead = transform.forward * speed *0.3f;
        Vector3 targetPosition = transform.position + lookAhead + new Vector3(0f, 2f, -5f);
        cameraTarget.position = Vector3.Lerp(cameraTarget.position, targetPosition, Time.deltaTime * 5f);
    }

    void HandleBoostInput()
    {
        if ((Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftCommand)) && currentBoostCooldown <= 0f)
        {
            if (!isBoosting)
            {
                ActivateBoost();
            }
        }
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

        if (isDrifting)
        {
            Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
            Vector3 rightVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
            rb.linearVelocity = forwardVelocity + rightVelocity * tractionMultiplier;
            if (driftSmokeL != null && driftSmokeR != null && !driftSmokeL.isEmitting && !driftSmokeR.isEmitting)
            {
                if (!driftSmokeL.isPlaying) driftSmokeL.Play();
                if (!driftSmokeR.isPlaying) driftSmokeR.Play();
            }
        }
        else
        {
            if (driftSmokeL != null && driftSmokeR != null && (driftSmokeL.isEmitting || driftSmokeR.isEmitting))
            {
                if (driftSmokeL.isPlaying) driftSmokeL.Stop();
                if (driftSmokeR.isPlaying) driftSmokeR.Stop();
            }
        }
    }

    void UpdateAudio()
    {
        if (engineAudio == null) { return; }
        float basePitch = 0.5f;
        if (isBoosting)
        {
            engineAudio.pitch*=1.5f;
        }

        engineAudio.pitch = Mathf.Clamp(basePitch + (speed / maxSpeed), 0.5f, 2f);
        engineAudio.volume = Mathf.Lerp(0.5f, 1f, speed / maxSpeed);

        if(windAudio != null)
        {
            windAudio.volume = Mathf.Clamp01((speed-40f)/40f);
            windAudio.pitch = 0.8f + (speed / maxSpeed) * 0.4f;
        }
        if(turboAudio != null)
        {
            bool turboRange = speed > 60f && isBoosting;
            if(turboRange && !turboAudio.isPlaying)
            {
                turboAudio.Play();
            }
            else if(!turboRange && turboAudio.isPlaying)
            {
                turboAudio.Stop();
            }
        }
    }
}
