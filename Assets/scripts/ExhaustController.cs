using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public BusController bus;
    private ParticleSystem ps; 
    private ParticleSystem.EmissionModule em;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        em = ps.emission;
    }

    // Update is called once per frame
    void Update()
    {
        float throttle = Mathf.Abs(Input.GetAxis("Vertical"));
        em.rateOverTime = Mathf.Lerp(5f, 30f, throttle);
    }
}
