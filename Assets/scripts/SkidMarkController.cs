using UnityEngine;

public class SkidMarkController : MonoBehaviour
{
    public WheelCollider wheelCollider; // Reference to the WheelCollider component
    private TrailRenderer skidTrail; // Reference to the TrailRenderer component
    private float skidThreshold = 0.5f; // Threshold for detecting skidding
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        skidTrail = GetComponent<TrailRenderer>();
        skidTrail.emitting = false; // Disable the skid trail initially
    }

    // Update is called once per frame
    void Update()
    {
        WheelHit wheelHit;
        wheelCollider.GetGroundHit(out wheelHit); // Get the ground hit information from the WheelCollider
        bool isSkidding = Mathf.Abs(wheelHit.sidewaysSlip) > skidThreshold || Mathf.Abs(wheelHit.forwardSlip) > skidThreshold; // Check if the wheel is skidding based on slip values
        skidTrail.emitting = isSkidding; // Emit the skid trail if the wheel is skidding
    }
}
