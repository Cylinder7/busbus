using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollReset : MonoBehaviour
{
    private struct BoneTransform
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
    }

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    private Rigidbody[] rigidbodies;
    private Dictionary<Transform, BoneTransform> originalBoneTransforms = new Dictionary<Transform, BoneTransform>();

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        rigidbodies = GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody rb in rigidbodies)
        {
            BoneTransform bt;
            bt.localPosition = rb.transform.localPosition;
            bt.localRotation = rb.transform.localRotation;
            
            originalBoneTransforms.Add(rb.transform, bt);
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            ResetToOriginalState();
        }
    }

    public void ResetToOriginalState()
    {

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        foreach (Rigidbody rb in rigidbodies)
        {
            // Reset velocity physics
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (originalBoneTransforms.TryGetValue(rb.transform, out BoneTransform bt))
            {
                rb.transform.localPosition = bt.localPosition;
                rb.transform.localRotation = bt.localRotation;
            }
            rb.Sleep();
        }
    }
}