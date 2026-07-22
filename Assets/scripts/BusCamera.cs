using System.Reflection;
using UnityEngine;

public class BusCamera : MonoBehaviour
{
    public Transform BusRoot;
    public Transform CameraTarget;
    public Vector3 CameraOffset;
    public float FollowSpeed = 5f;
    public float LookSpeed = 5f;
    public float BoostCameraShakeIntensity = 0.5f;

    private Transform CameraTransform;
    private BusController busController;
    private FieldInfo boostingField;

    void Awake()
    {
        CameraTransform = GetComponent<Transform>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (BusRoot != null)
        {
            busController = BusRoot.GetComponent<BusController>();
            boostingField = typeof(BusController).GetField("isBoosting", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(BusRoot == null || CameraTarget == null || busController == null) { return; }
        Vector3 desiredPosition = BusRoot.TransformPoint(CameraOffset);
        CameraTransform.position = Vector3.Lerp(CameraTransform.position, desiredPosition, FollowSpeed * Time.deltaTime);
        Vector3 LookTarget = CameraTarget != null ? CameraTarget.position : BusRoot.position + BusRoot.forward * 10f;
        Vector3 LookDirection = (LookTarget - CameraTransform.position).normalized;
        Quaternion desiredRotation = Quaternion.LookRotation(LookDirection);
        CameraTransform.rotation = Quaternion.Slerp(CameraTransform.rotation, desiredRotation, LookSpeed * Time.deltaTime);
        bool isBoosting = false;
        if (boostingField != null)
        {
            object value = boostingField.GetValue(busController);
            if (value is bool flag)
            {
                isBoosting = flag;
            }
        }

        if (isBoosting)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * BoostCameraShakeIntensity;
            CameraTransform.position += shakeOffset;
        }
    }
}
