using UnityEngine;
using TMPro;

public class Speedometer : MonoBehaviour
{
    public Rigidbody busRigidbody;
    public TMP_Text speedText;

    // 1 m/s = 2.23694 mph
    private const float MPS_TO_MPH = 2.23694f;

    void Update()
    {
        if (busRigidbody == null || speedText == null) return;

        float mph = busRigidbody.linearVelocity.magnitude * MPS_TO_MPH;
        speedText.text = Mathf.FloorToInt(mph).ToString();
    }
}