using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private Transform carTransform;
    private Rigidbody rb;
    private float acceleration;
    private float deceleration;
    private float maxTurnSpeed;
    private float maxTurnAngle;
    private float steeringSmoothing;
    private float steeringCurveStart;
    private float steeringCurveEnd;
    private float grip;
    private float maxGrip;
    private Vector3 velocityDirection;

    public float CurrentSpeed { get; private set; }
    public float CurrentMaxSpeed { get; private set; }
    public float CurrentFrontAxelSteerAngle { get; private set; }
    public bool HasMovement { get; private set; } = true;

    public void Initialize(Transform transformReference, Rigidbody rigidbody, float accelerationValue,
        float decelerationValue, float normalSpeed, float turnSpeed, float turnAngle,
        float smoothing, float curveStart, float curveEnd, float gripValue, float maxGripValue)
    {
        carTransform = transformReference;
        rb = rigidbody;
        acceleration = accelerationValue;
        deceleration = decelerationValue;
        CurrentMaxSpeed = normalSpeed;
        maxTurnSpeed = turnSpeed;
        maxTurnAngle = turnAngle;
        steeringSmoothing = smoothing;
        steeringCurveStart = curveStart;
        steeringCurveEnd = curveEnd;
        grip = gripValue;
        maxGrip = maxGripValue;
    }

    public void SetMaxSpeed(float maxSpeed)
    {
        CurrentMaxSpeed = maxSpeed;
        CurrentSpeed = Mathf.Clamp(CurrentSpeed, -CurrentMaxSpeed, CurrentMaxSpeed);
    }

    public void SetMovementEnabled(bool enabled)
    {
        HasMovement = enabled;
    }

    public void UpdateSpeed(bool accelerating, bool reversing)
    {
        if (accelerating && !reversing)
        {
            CurrentSpeed += acceleration * Time.deltaTime;
        }
        else if (reversing && !accelerating)
        {
            CurrentSpeed -= acceleration * Time.deltaTime;
        }
        else
        {
            CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, 0f, deceleration * Time.deltaTime);
        }

        CurrentSpeed = Mathf.Clamp(CurrentSpeed, -CurrentMaxSpeed, CurrentMaxSpeed);
    }

    public void UpdateMovement(float steering, bool isDrifting, float driftDirection,
        float counterSteerReduction, float driftSteeringStrength)
    {
        if (velocityDirection == Vector3.zero)
        {
            velocityDirection = carTransform.forward;
        }

        float gripRate = grip * maxGrip;
        velocityDirection = Vector3.Slerp(velocityDirection, carTransform.forward, gripRate * Time.deltaTime);

        float speedMagnitude = Mathf.Abs(CurrentSpeed);
        float targetSteeringAngle = Mathf.Clamp(steering, -1f, 1f) * maxTurnAngle;
        CurrentFrontAxelSteerAngle = Mathf.Lerp(CurrentFrontAxelSteerAngle, targetSteeringAngle, steeringSmoothing * Time.deltaTime);
        float steeringPercent = CurrentFrontAxelSteerAngle / maxTurnAngle;
        float speedRatio = CurrentMaxSpeed > 0f ? Mathf.Clamp01(speedMagnitude / CurrentMaxSpeed) : 0f;
        float steeringAuthority = CalculateSteeringAuthority(speedRatio);
        float steeringDirection = CurrentSpeed >= 0f ? 1f : -1f;
        float steeringTurn = steeringPercent * steeringAuthority * steeringDirection;

        if (isDrifting)
        {
            float driftSteeringAmount = Mathf.Clamp01(Mathf.Abs(steering));
            float driftSteeringSign = Mathf.Abs(steering) > 0.001f ? Mathf.Sign(steering) : 0f;
            float driftInfluence = driftSteeringSign == driftDirection ? driftSteeringAmount : -driftSteeringAmount * counterSteerReduction;
            steeringTurn = driftDirection * Mathf.Clamp01(driftInfluence * driftSteeringStrength * steeringAuthority);
        }

        if (HasMovement)
        {
            carTransform.position += velocityDirection * CurrentSpeed * Time.deltaTime;

            if (speedMagnitude > 0.0001f)
            {
                carTransform.Rotate(0f, steeringTurn * maxTurnSpeed * Time.deltaTime, 0f);
            }
        }
        else if (CurrentSpeed < 0f)
        {
            CurrentSpeed = 0f;
        }
    }

    public void ClampRigidbodyVelocity()
    {
        if (rb == null)
        {
            return;
        }

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        if (horizontalVelocity.magnitude < 0.04f)
        {
            currentVelocity.x = 0f;
            currentVelocity.z = 0f;
            rb.linearVelocity = currentVelocity;
        }

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, CurrentMaxSpeed);
    }

    private float CalculateSteeringAuthority(float speedRatio)
    {
        if (speedRatio < steeringCurveStart)
        {
            return Mathf.Lerp(0f, 1f, speedRatio / steeringCurveStart);
        }

        if (speedRatio > steeringCurveEnd)
        {
            return Mathf.Lerp(1f, 0f, (speedRatio - steeringCurveEnd) / (3f - steeringCurveEnd));
        }

        return 1f;
    }
}
