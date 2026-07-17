using UnityEngine;

public class BusCamera : MonoBehaviour
{
    public Transform bus;
    public Vector3 offset = new Vector3(0f, 6f, -14f);
    public float followSpeed = 5f;
    public float lookSpeed = 8f;

    private void LateUpdate()
 {
    // Smoothly move to the target position
    Vector3 desiredPos =
    bus.position + bus.rotation * offset;
    transform.position = Vector3.Lerp(transform.position, desiredPos,followSpeed * Time.deltaTime);
    // Look at the bus
    Vector3 lookTarget = bus.position + bus.rotation * new Vector3(0f, 2f, 5f);
    transform.rotation = Quaternion.Slerp(transform.rotation,
    Quaternion.LookRotation(lookTarget - transform.position),lookSpeed * Time.deltaTime);
 }


}
