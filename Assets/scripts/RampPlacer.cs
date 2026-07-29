using UnityEngine;

public class RampPlacer : MonoBehaviour
{
    // where you can place the ramps between
    public int startX = 0;
    public int endX = 20;

    public int startZ = -173;

    public float zDistance = 50f;
    public float yLevel = 0.4f;

    public Quaternion rotation = Quaternion.Euler(-50f, 0f, 0f);

    public int rampsToPlace = 3;
    public Transform ramp;

    private void Start()
    {
        if (!Application.isPlaying)
            return;

        PlaceRamps();
    }

    [ContextMenu("Place Ramps")]
    public void PlaceRamps()
    {
        if (ramp == null || rampsToPlace <= 0)
            return;

        for (var i = 0; i < rampsToPlace; i++)
        {
            int x = UnityEngine.Random.Range(startX, endX);
            float z = (i * -zDistance)+startZ;
            Vector3 positionToPlace = new Vector3(x, yLevel, z);

            Transform clonedRamp = Instantiate(ramp, positionToPlace, rotation, transform);
            clonedRamp.name = "ramp_" + i;
        }
    }
}