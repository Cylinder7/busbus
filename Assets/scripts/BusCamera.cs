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
    public float positionSmoothing = 5f;

    private Transform CameraTransform;
    private BusController busController;
    private FieldInfo boostingField;

    [Header("Default Follow")]
    public float defaultDistance = 12f;
    public float defaultHeight = 4f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 3f;
    public float minVerticalAngle = -40f;
    public float maxVerticalAngle = 90f;
    private float yaw;
    private float pitch = 25f;
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
    if (BusRoot == null || CameraTarget == null || busController == null) return;

    // Mouse look orbit
    if (Input.GetMouseButton(1))
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
    }
    else
    {
        // Snap back behind bus
        yaw = Mathf.LerpAngle(yaw, BusRoot.eulerAngles.y, positionSmoothing * Time.deltaTime);
        pitch = Mathf.Lerp(pitch, 25f, positionSmoothing * Time.deltaTime);
    }

    // Calculate orbit position
    Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
    Vector3 desiredPosition = BusRoot.position + rotation * new Vector3(0, 0, -defaultDistance);
    desiredPosition.y = Mathf.Max(desiredPosition.y, BusRoot.position.y + 0.5f);

    // Boost shake
    bool isBoosting = false;
    if (boostingField != null)
    {
        object value = boostingField.GetValue(busController);
        if (value is bool flag) isBoosting = flag;
    }
    if (isBoosting)
        desiredPosition += Random.insideUnitSphere * BoostCameraShakeIntensity;

    // Apply position and rotation
    CameraTransform.position = Vector3.Lerp(CameraTransform.position, desiredPosition, FollowSpeed * Time.deltaTime);
    CameraTransform.LookAt(BusRoot.position + Vector3.up * defaultHeight * 0.5f);
    }
}
