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
    [SerializeField] private float driftStartSpeed = 5f;
    [SerializeField] private float driftSteeringStrength = 0.4f;
    [SerializeField] private float driftAngleMultiplier = 2f;
    [SerializeField] private float driftCounterSteerReduction = 0.5f;
    [SerializeField] private float driftGripReduction = 0.35f;

    [Header("Object Assignment")]
    [SerializeField] private Transform frontAxel;
    [SerializeField] private Transform rearAxel;
    [SerializeField] private Transform car;
    [SerializeField] private float carRotationLerpSpeed = 8f;
    [SerializeField] private float carRotationMaxAngle = 30f;

    private float currentSpeed;
    private bool isAccelerating;
    private bool isReversing;

    private InputSystem_Actions input;
    private Rigidbody rb;
    private float steering;
    private float frontAxelSpinAngle;
    private float rearAxelSpinAngle;
    private float currentFrontAxelSteerAngle;
    private float driftDirection;
    private float driftChargeTime;
    private float currentCarRotationAngle;
    private float targetCarRotationAngle;
    private Quaternion carBaseLocalRotation = Quaternion.identity;
    private Vector3 velocityDirection;
    private DRIFT_STATE currentDriftState;

    enum DRIFT_STATE {
        None,
        Holding,
        Released
    };

    private void Awake()
    {
        input = new InputSystem_Actions();
        rb = GetComponent<Rigidbody>();

        if (car != null)
        {
            carBaseLocalRotation = car.localRotation;
        }
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


        if (input == null)
        {
            return;
        }

        isAccelerating = input.Player.Accelerate.IsPressed();
        isReversing = input.Player.Reverse.IsPressed();
        steering = input.Player.Steer.ReadValue<float>();
        bool driftInputHeld = input.Player.Drift.IsPressed();

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

        if (currentDriftState == DRIFT_STATE.Holding)
        {
            float driftSteeringAmount = Mathf.Clamp01(Mathf.Abs(steering));
            float driftSteeringSign = Mathf.Abs(steering) > 0.001f ? Mathf.Sign(steering) : 0f;
            float driftInfluence = driftSteeringSign == driftDirection ? driftSteeringAmount : -driftSteeringAmount * driftCounterSteerReduction;
            steeringTurn = steeringDirection * Mathf.Clamp01(driftInfluence * driftSteeringStrength * steeringAuthority);
        }

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

        switch (currentDriftState)
        {
            case DRIFT_STATE.None:
                if (driftInputHeld && Mathf.Abs(steering) > 0.1f && Mathf.Abs(currentSpeed) > driftStartSpeed)
                {
                    driftDirection = Mathf.Sign(steering);
                    driftChargeTime = 0f;
                    targetCarRotationAngle = 0f;
                    currentDriftState = DRIFT_STATE.Holding;
                }
                break;
            case DRIFT_STATE.Holding:
                driftChargeTime += Time.deltaTime;

                float gripScale = Mathf.Lerp(1f, 1.35f, Mathf.Clamp01(driftGripReduction));
                float driftAngleTarget = currentCarRotationAngle;

                if (Mathf.Abs(steering) > 0.001f)
                {
                    float steeringSign = Mathf.Sign(steering);
                    float steeringMagnitude = Mathf.Abs(steering);
                    float maxDriftAngle = carRotationMaxAngle * driftAngleMultiplier * gripScale; 

                    if (steeringSign == driftDirection)
                    {
                        float sameDirectionAngle = Mathf.Clamp(steeringMagnitude * maxDriftAngle, 0f, maxDriftAngle);
                        driftAngleTarget = Mathf.Max(Mathf.Abs(currentCarRotationAngle), sameDirectionAngle) * driftDirection;
                    }
                    else
                    {
                        float counterSteerAmount = Mathf.Clamp01(steeringMagnitude * driftCounterSteerReduction);
                        float reducedMagnitude = Mathf.Max(Mathf.Abs(currentCarRotationAngle) * (1f - counterSteerAmount), 0f);
                        float oppositeClamp = Mathf.Max(carRotationMaxAngle * 0.15f * counterSteerAmount, 0.25f);
                        driftAngleTarget = (reducedMagnitude > 0.0001f ? Mathf.Sign(currentCarRotationAngle) : driftDirection) * Mathf.Max(reducedMagnitude, oppositeClamp);
                    }
                }

                targetCarRotationAngle = Mathf.Lerp(targetCarRotationAngle, driftAngleTarget, carRotationLerpSpeed * Time.deltaTime);

                if (!driftInputHeld)
                {
                    currentDriftState = DRIFT_STATE.Released;
                }
                break;
            case DRIFT_STATE.Released:
                targetCarRotationAngle = 0f;
                // TODO: boost hook - use driftChargeTime to determine boost tier once the boost system exists.
                driftChargeTime = 0f;
                driftDirection = 0f;
                currentDriftState = DRIFT_STATE.None;
                break;
            default:
                break;
        }

        ApplyCarRotation();

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

    private void ApplyCarRotation()
    {
        if (car == null)
        {
            return;
        }

        currentCarRotationAngle = Mathf.Lerp(currentCarRotationAngle, targetCarRotationAngle, carRotationLerpSpeed * Time.deltaTime);
        car.localRotation = carBaseLocalRotation * Quaternion.Euler(0f, currentCarRotationAngle, 0f);
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
