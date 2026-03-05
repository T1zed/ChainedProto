using UnityEngine;
using UnityEngine.UIElements;

public class BallBehavior : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Impulse")]
    public float hitForce = 20f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update() { }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("sword"))
        {
            SwordDirection sd = other.GetComponent<SwordDirection>();

            if (sd != null)
            {
                Debug.Log("test");
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(sd.hitDirection * hitForce, ForceMode.Impulse);
            }
        }
    }
}