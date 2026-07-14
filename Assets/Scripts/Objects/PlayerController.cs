using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private TMP_Text debugText;

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
        Vector3 movementDirection = GetMovementDirection();
        float signedSpeed = Vector3.Dot(rb.linearVelocity, movementDirection);

        if (input == null)
        {
            return;
        }

        isAccelerating = input.Player.Accelerate.IsPressed();
        isReversing = input.Player.Reverse.IsPressed();

        UpdateSpeed(isAccelerating, isReversing);

        debugText.text = 
            "RC Car Debug Values:\n" +
            $"\nRotation: {transform.eulerAngles}" +
            $"\nPosition: {transform.position}\n" +
            $"Velocity: {rb.linearVelocity}\n" +
            $"Speed: {rb.linearVelocity.magnitude:F4}\n" +
            $"Angular: {rb.angularVelocity}\n" +
            $"Sleeping: {rb.IsSleeping()}" + 
            $"SignedSpeed: {signedSpeed}\n" +
            "Terrain: N/A";
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        Vector3 movementDirection = GetMovementDirection();

        if (isAccelerating && !isReversing)
        {
            rb.AddForce(movementDirection * acceleration, ForceMode.Force);
        }
        else if (isReversing && !isAccelerating)
        {
            rb.AddForce(-movementDirection * acceleration, ForceMode.Force);
        }
        else
        {
            Vector3 velocity = rb.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (horizontalVelocity.sqrMagnitude > 0.0001f)
            {
                rb.AddForce(-horizontalVelocity.normalized * deceleration, ForceMode.Force);
            }
        }

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalCurrentVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        // clamp magnitude if it's less than a visible value so that the rigidbody doesn't wobble
        if (horizontalCurrentVelocity.magnitude < 0.04f)
        {
            currentVelocity.x = 0f;
            currentVelocity.z = 0f;
            rb.linearVelocity = currentVelocity;
            Debug.Log(rb.linearVelocity);
        }

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);

    }

    private Vector3 GetMovementDirection()
    {
        Vector3 direction = transform.forward;
        direction.y = 0f;

        return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
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
