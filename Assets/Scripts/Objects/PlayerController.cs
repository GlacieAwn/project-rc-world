using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float maxTurnSpeed = 80f;
    [SerializeField] private float maxTurnAngle = 30f;
    [SerializeField] private float axleSpinMultiplier = 24f;
    [SerializeField] private float steeringSmoothing = 8f;
    [SerializeField] [Range(0f, 1f)] private float steeringCurveStart = 0.25f;
    [SerializeField] [Range(0f, 1f)] private float steeringCurveEnd = 0.8f;
    [SerializeField] [Range(0f, 1f)] private float grip = 0.8f;
    [SerializeField] private float maxGrip = 8f;
    [SerializeField] private TMP_Text debugText;

    [Header("Object Assignment")]
    [SerializeField] private Transform frontAxel;
    [SerializeField] private Transform rearAxel;


    private float currentSpeed;
    private bool isAccelerating;
    private bool isReversing;

    private InputSystem_Actions input;
    private Rigidbody rb;
    private float steering;
    private float frontAxelSpinAngle;
    private float rearAxelSpinAngle;
    private float currentFrontAxelSteerAngle;
    private Vector3 velocityDirection;

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
        steering = input.Player.Steer.ReadValue<float>();

        UpdateSpeed(isAccelerating, isReversing);

        if (velocityDirection == Vector3.zero)
            velocityDirection = transform.forward;

        float gripRate = grip * maxGrip;
        velocityDirection = Vector3.Slerp(velocityDirection, transform.forward, gripRate * Time.deltaTime);

        transform.position += velocityDirection * currentSpeed * Time.deltaTime;


        float speedMagnitude = Mathf.Abs(currentSpeed);
        float targetSteeringAngle = Mathf.Clamp(steering, -1f, 1f) * maxTurnAngle;
        currentFrontAxelSteerAngle = Mathf.Lerp(currentFrontAxelSteerAngle, targetSteeringAngle, steeringSmoothing * Time.deltaTime);

        float steeringPercent = currentFrontAxelSteerAngle / maxTurnAngle;
        float speedRatio = maxSpeed > 0f ? Mathf.Clamp01(speedMagnitude / maxSpeed) : 0f;

        float steeringAuthority = 1f;
        if (speedRatio < steeringCurveStart)
        {
            steeringAuthority = Mathf.Lerp(0f, 1f, speedRatio / steeringCurveStart);
        }
        else if (speedRatio > steeringCurveEnd)
        {
            steeringAuthority = Mathf.Lerp(1f, 0f, (speedRatio - steeringCurveEnd) / (3f - steeringCurveEnd));
        }

        float steeringDirection = currentSpeed >= 0f ? 1f : -1f;
        float steeringTurn = steeringPercent * steeringAuthority * steeringDirection;

        if (speedMagnitude > 0.0001f)
        {
            transform.Rotate(0f, steeringTurn * maxTurnSpeed * Time.deltaTime, 0f);
        }

        float spinDirection = currentSpeed >= 0f ? 1f : -1f;

        if (frontAxel != null)
        {
            frontAxelSpinAngle = Mathf.Repeat(frontAxelSpinAngle + spinDirection * speedMagnitude * axleSpinMultiplier * Time.deltaTime, 360f);
            frontAxel.localRotation = Quaternion.Euler(frontAxelSpinAngle, currentFrontAxelSteerAngle, 0f);
        }

        if (rearAxel != null)
        {
            rearAxelSpinAngle = Mathf.Repeat(rearAxelSpinAngle + spinDirection * speedMagnitude * axleSpinMultiplier * Time.deltaTime, 360f);
            rearAxel.localRotation = Quaternion.Euler(rearAxelSpinAngle, 0f, 0f);
        }

        debugText.text = 
            "RC Car Debug Values:\n" +
            $"\nRotation: {transform.eulerAngles}" +
            $"\nPosition: {transform.position}\n" +
            $"Velocity: {rb.linearVelocity}\n" +
            $"Speed: {currentSpeed:F4}\n" +
            $"Angular: {rb.angularVelocity}\n" +
            $"Sleeping: {rb.IsSleeping()}" + 
            $"SignedSpeed: {signedSpeed}\n" +
            "Terrain: N/A\n" +
            $"Steering: {steering}\n";

    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        // Vector3 movementDirection = GetMovementDirection();

        // if (isAccelerating && !isReversing)
        // {
        //     rb.AddForce(movementDirection * acceleration, ForceMode.Force);
        // }
        // else if (isReversing && !isAccelerating)
        // {
        //     rb.AddForce(-movementDirection * acceleration, ForceMode.Force);
        // }
        // else
        // {
        //     Vector3 velocity = rb.linearVelocity;
        //     Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        //     if (horizontalVelocity.sqrMagnitude > 0.0001f)
        //     {
        //         rb.AddForce(-horizontalVelocity.normalized * deceleration, ForceMode.Force);
        //     }

        //     else 
        //     {
        //         rb.linearVelocity = Vector3.zero;
        //     }
        // }
        

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
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed, maxSpeed);
    }
}
