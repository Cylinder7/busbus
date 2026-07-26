using UnityEngine;

public class ResetOnR : MonoBehaviour
{
    public Transform obj;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialVelocity;
    private Vector3 initialAngularVelocity;

    private Rigidbody[] rigidbodies;
    private Vector3[] initialBodyPositions;
    private Quaternion[] initialBodyRotations;
    private Vector3[] initialBodyVelocities;
    private Vector3[] initialBodyAngularVelocities;

    void Start()
    {
        if (obj == null)
        {
            obj = transform;
        }

        initialPosition = obj.position;
        initialRotation = obj.rotation;

        rigidbodies = obj.GetComponentsInChildren<Rigidbody>(true);
        initialBodyPositions = new Vector3[rigidbodies.Length];
        initialBodyRotations = new Quaternion[rigidbodies.Length];
        initialBodyVelocities = new Vector3[rigidbodies.Length];
        initialBodyAngularVelocities = new Vector3[rigidbodies.Length];

        Rigidbody rootRigidbody = obj.GetComponent<Rigidbody>();
        if (rootRigidbody != null)
        {
            initialVelocity = rootRigidbody.linearVelocity;
            initialAngularVelocity = rootRigidbody.angularVelocity;
        }
        else
        {
            initialVelocity = Vector3.zero;
            initialAngularVelocity = Vector3.zero;
        }

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody rb = rigidbodies[i];
            initialBodyPositions[i] = rb.position;
            initialBodyRotations[i] = rb.rotation;
            initialBodyVelocities[i] = rb.linearVelocity;
            initialBodyAngularVelocities[i] = rb.angularVelocity;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            obj.position = initialPosition;
            obj.rotation = initialRotation;

            Rigidbody rootRigidbody = obj.GetComponent<Rigidbody>();
            if (rootRigidbody != null)
            {
                rootRigidbody.linearVelocity = initialVelocity;
                rootRigidbody.angularVelocity = initialAngularVelocity;
            }

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                Rigidbody rb = rigidbodies[i];
                rb.transform.position = initialBodyPositions[i];
                rb.transform.rotation = initialBodyRotations[i];
                rb.linearVelocity = initialBodyVelocities[i];
                rb.angularVelocity = initialBodyAngularVelocities[i];
                rb.Sleep();
            }
        }
    }
}
