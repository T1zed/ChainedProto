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

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision avec : " + other.gameObject.name + " | tag : " + other.gameObject.tag);
        if (other.gameObject.CompareTag("Bumper"))
        {
            Bumper bumper = other.gameObject.GetComponent<Bumper>();
            if (bumper != null)
            {
                float rad = bumper.angle * Mathf.Deg2Rad;
                Vector3 bumperDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

                rb.linearVelocity = Vector3.zero;
                rb.AddForce(bumperDir * bumper.force, ForceMode.Impulse);
                Debug.Log("Ball hit Bumper");
            }
        }
    }
}