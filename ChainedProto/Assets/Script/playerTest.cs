using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerTest : MonoBehaviour
{
    private PlayerIn pInput;

    public Rigidbody rb;
    public Transform ball;

    [Header("Chain Settings")]
    public float chainMaxLength = 5f;
    public float pullForce = 40f;
    [Header("player")]
    public float moveSpeed = 8f;
    [Header("Dash")]
    public float dashCooldown = 0.5f;
    private float lastDashTime;
    public float dashDuration = 0.2f;
    public float dashSpeed = 20f;
    private bool isDashing;
    [Header("Impulse")]
    public float impCooldown = 0.3f;
    private float lastImpTime;
    private bool isImp;
    public float impDuration = 0.2f;
    public float swordOffset = 1f;

    public GameObject sword;
    private SwordDirection swordDir;

    private void Awake()
    {
        pInput = new PlayerIn();
        swordDir = sword.GetComponent<SwordDirection>();
        sword.transform.SetParent(null);

        sword.SetActive(false);
    }

    private void OnEnable()
    {
        pInput.Player.Move.Enable();
        pInput.Player.Dash.Enable();
        pInput.Player.Impulse.Enable();
        pInput.Player.Dash.performed += OnDash;
        pInput.Player.Impulse.performed += OnImpulse;
    }

    private void OnDisable()
    {
        pInput.Player.Move.Disable();
        pInput.Player.Dash.Disable();
        pInput.Player.Impulse.Disable();
        pInput.Player.Dash.performed -= OnDash;
        pInput.Player.Impulse.performed -= OnImpulse;
    }

    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;
    bool isGrounded;

    void Update() { }

    void FixedUpdate()
    {
        HandleMovement();
        HandleChainConstraint();
    }

    void OnDash(InputAction.CallbackContext context)
    {
        if (Time.time <= lastDashTime + dashCooldown || isDashing)
            return;

        lastDashTime = Time.time;
        Debug.Log("dash");
        StartCoroutine(DashCoroutine());
    }

    void OnImpulse(InputAction.CallbackContext context)
    {
        if (Time.time <= lastImpTime + impCooldown || isImp)
            return;

        lastImpTime = Time.time;
        Debug.Log("impulse");

        Vector2 moveInput = pInput.Player.Move.ReadValue<Vector2>();

        if (moveInput == Vector2.zero)
            moveInput = Vector2.right;

        Vector2 snapDir = SnapTo8Directions(moveInput);

        StartCoroutine(ImpulseCoroutine(snapDir));
    }

    Vector2 SnapTo8Directions(Vector2 input)
    {
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
    
        float snapped = Mathf.Round(angle / 45f) * 45f;
        float rad = snapped * Mathf.Deg2Rad;
        return new Vector2(Mathf.Round(Mathf.Cos(rad)), Mathf.Round(Mathf.Sin(rad)));
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;

        Vector3 dir = ball.position - rb.position;
        dir.z = 0f;
        dir.Normalize();

        float timer = 0f;

        while (timer < dashDuration)
        {

            Vector3 current = rb.linearVelocity;
            rb.linearVelocity = new Vector3(
                dir.x * dashSpeed,       
                current.y + dir.y * dashSpeed,
                0f
            );
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
    }

    IEnumerator ImpulseCoroutine(Vector2 direction)
    {
        isImp = true;

        Vector3 dir3 = new Vector3(direction.x, direction.y, 0f);

        Vector3 swordPos = rb.position + dir3 * swordOffset;
        sword.transform.position = swordPos;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        sword.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        sword.SetActive(true);

        BoxCollider bc = sword.GetComponent<BoxCollider>();
        Collider[] hits = Physics.OverlapBox(sword.transform.TransformPoint(bc.center),bc.size * 0.5f,sword.transform.rotation);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Ball"))
            {
                Rigidbody ballRb = hit.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    //StartCoroutine(slowmo());
                    ballRb.linearVelocity = Vector3.zero;
                    ballRb.AddForce(dir3 * swordDir.hitForce, ForceMode.Impulse);
                    Debug.Log("Ball hit via OverlapBox");
                }
            }
        }

        yield return new WaitForSeconds(impDuration);

        sword.SetActive(false);
        isImp = false;
    }

    void HandleMovement()
    {
        if (isDashing) return;

        Vector2 move = pInput.Player.Move.ReadValue<Vector2>();
        float moveX = move.x;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveX * moveSpeed;

        rb.linearVelocity = velocity;
    }

    void HandleChainConstraint()
    {
        Vector3 playerPos = rb.position;
        Vector3 ballPos = ball.position;

        Vector3 dir = playerPos - ballPos;
        float dist = dir.magnitude;

        if (dist > chainMaxLength)
        {
            Vector3 normal = dir.normalized;

            Vector3 correctedPos = ballPos + normal * chainMaxLength;
            rb.position = Vector3.Lerp(rb.position, correctedPos, 0.5f);

            Vector3 velocity = rb.linearVelocity;
            float radialSpeed = Vector3.Dot(velocity, normal);

            if (radialSpeed > 0f)
            {
                velocity -= normal * radialSpeed;
                rb.linearVelocity = velocity;
            }
        }
    }

    //IEnumerator slowmo()
    //{
    //    Time.timeScale = (0.1f);
    //    yield return new WaitForSeconds(0.1f);
    //    Time.timeScale = 1f;

    //}
}