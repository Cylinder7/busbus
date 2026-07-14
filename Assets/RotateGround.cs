using UnityEngine;
// 1. Add this namespace at the top
using UnityEngine.InputSystem; 

public class RotateGround : MonoBehaviour
{
    private Vector3 initLocation;
    private Vector3 initVelocity;
    private Vector3 angVelocity;
    private Rigidbody rigidBody;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        initVelocity = rigidBody.linearVelocity;
        angVelocity = rigidBody.angularVelocity;
        initLocation = rigidBody.transform.position;
    }

    void Update()
    {
        // 2. Use the New Input System syntax
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rigidBody.linearVelocity = new Vector3(rigidBody.linearVelocity.x, 10f, rigidBody.linearVelocity.z);
        }
    }
}