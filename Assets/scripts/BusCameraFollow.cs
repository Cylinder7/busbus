using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;         // Drag your bus here in the Inspector
    public Vector3 offset = new Vector3(0, 3, -6);
    public float positionSmoothing = 5f;
    public float rotationSmoothing = 3f;

    void LateUpdate()
    {
        // Desired position: behind and above the bus in world space
        Vector3 desiredPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothing * Time.deltaTime);

        // Smoothly look at the bus
        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothing * Time.deltaTime);

        Debug.Log("Target: " + target.name);
    // ... rest of code
    }
}

