using UnityEngine;

public class BusController : MonoBehaviour
{
    [Header("Wheel Colliders")]

    public WheelCollider FrontLeft;
    public WheelCollider FrontRight;
    public WheelCollider RearLeft;
    public WheelCollider RearRight;

    [Header("Visual Wheels")]
    
    public Transform FrontLeftWheel;
    public Transform FrontRightWheel;
    public Transform RearLeftWheel;
    public Transform RearRightWheel;

    [Header("Bus Settings")]

    public float motorTorque = 5000f;
    public float maxSteerAngle = 30f;
    public float brakeTorque = 5000f;
    public float maxSpeed = 25f;

    private float steerInput = 0f;
    private float motorInput = 0f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
    }
    
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        HandleInput();
        ApplyMotor();
        ApplySteering();
        ApplyBrakes();
        UpdateWheelVisuals();
        LimitSpeed();
    }
    
    void Update()
    {
        
    }

    private void HandleInput()
    {
        steerInput = Input.GetAxis("Horizontal");
        motorInput = Input.GetAxis("Vertical");
    }

    private void ApplyMotor()
    {
        // Added a minus (-) sign here to invert the direction
        RearLeft.motorTorque = -motorInput * motorTorque;
        RearRight.motorTorque = -motorInput * motorTorque;
    }

    private void ApplySteering()
    {
        float steerAngle = steerInput * maxSteerAngle;
        FrontLeft.steerAngle = steerAngle;
        FrontRight.steerAngle = steerAngle;
    }
    
    private void ApplyBrakes()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            RearLeft.brakeTorque = brakeTorque;
            RearRight.brakeTorque = brakeTorque;
        }
        else
        {
            RearLeft.brakeTorque = 0f;
            RearRight.brakeTorque = 0f;
        }
    }
    
    private void LimitSpeed()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
    
    private void UpdateWheelVisuals()
    {
        UpdateWheelVisual(FrontLeft, FrontLeftWheel);
        UpdateWheelVisual(FrontRight, FrontRightWheel);
        UpdateWheelVisual(RearLeft, RearLeftWheel);
        UpdateWheelVisual(RearRight, RearRightWheel);
    }

    private void UpdateWheelVisual(WheelCollider collider, Transform wheelTransform)
    {
        collider.GetWorldPose(out Vector3 pos, out Quaternion rot);

        wheelTransform.position = pos;
        wheelTransform.rotation = rot * Quaternion.Euler(0f, 0f, 90f);
    }
}