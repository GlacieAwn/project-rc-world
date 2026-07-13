using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f;
    [SerializeField] private float maxSpeed = 20f;

    private float currentSpeed;
    private bool isAccelerating;
    private bool isReversing;

    private InputSystem_Actions input;
    private Rigidbody rb;

    private void Awake()
    {
        input = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (input != null)
        {
            input.Enable();
        }
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.Disable();
        }
    }

    private void Update()
    {
        if (input == null)
        {
            return;
        }

        isAccelerating = input.Player.Accelerate.IsPressed();
        isReversing = input.Player.Reverse.IsPressed();

        UpdateSpeed(isAccelerating, isReversing);
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (isAccelerating && !isReversing)
        {
            rb.AddForce(transform.forward * acceleration, ForceMode.Force);
        }
        else if (isReversing && !isAccelerating)
        {
            rb.AddForce(-transform.forward * acceleration, ForceMode.Force);
        }
        else
        {
            Vector3 velocity = rb.linearVelocity;
            if (velocity.sqrMagnitude > 0.0001f)
            {
                rb.AddForce(-velocity.normalized * deceleration, ForceMode.Force);
            }
        }

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }

    private void UpdateSpeed(bool accelerating, bool reversing)
    {
        if (accelerating && !reversing)
            currentSpeed += acceleration * Time.deltaTime;
        else if (reversing && !accelerating)
            currentSpeed -= acceleration * Time.deltaTime;
        else
            currentSpeed -= deceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
    }
}
