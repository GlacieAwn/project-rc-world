using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private Transform carTransform;
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

    public void Initialize(Transform transformReference, float accelerationValue,
        float decelerationValue, float normalSpeed, float turnSpeed, float turnAngle,
        float smoothing, float curveStart, float curveEnd, float gripValue, float maxGripValue)
    {
        carTransform = transformReference;
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
            // Grounding owns vertical movement. Keep drive motion on the world
            // horizontal plane even when the vehicle is visually tilted on a slope.
            velocityDirection = Vector3.ProjectOnPlane(carTransform.forward, Vector3.up).normalized;
        }

        float gripRate = grip * maxGrip;
        Vector3 flatForward = Vector3.ProjectOnPlane(carTransform.forward, Vector3.up).normalized;
        velocityDirection = Vector3.Slerp(velocityDirection, flatForward, gripRate * Time.deltaTime);
        carTransform.position += velocityDirection * CurrentSpeed * Time.deltaTime;

        float speedMagnitude = Mathf.Abs(CurrentSpeed);
        float targetSteeringAngle = Mathf.Clamp(steering, -1f, 1f) * maxTurnAngle;
        CurrentFrontAxelSteerAngle = Mathf.Lerp(CurrentFrontAxelSteerAngle, targetSteeringAngle,
            steeringSmoothing * Time.deltaTime);
        float steeringPercent = CurrentFrontAxelSteerAngle / maxTurnAngle;
        float speedRatio = CurrentMaxSpeed > 0f ? Mathf.Clamp01(speedMagnitude / CurrentMaxSpeed) : 0f;
        float steeringAuthority = CalculateSteeringAuthority(speedRatio);
        float steeringDirection = CurrentSpeed >= 0f ? 1f : -1f;
        float steeringTurn = steeringPercent * steeringAuthority * steeringDirection;

        if (isDrifting)
        {
            float driftSteeringAmount = Mathf.Clamp01(Mathf.Abs(steering));
            float driftSteeringSign = Mathf.Abs(steering) > 0.001f ? Mathf.Sign(steering) : 0f;
            float driftInfluence = driftSteeringSign == driftDirection ? driftSteeringAmount :
                -driftSteeringAmount * counterSteerReduction;
            steeringTurn = driftDirection * Mathf.Clamp01(driftInfluence * driftSteeringStrength * steeringAuthority);
        }

        if (speedMagnitude > 0.0001f)
        {
            carTransform.Rotate(0f, steeringTurn * maxTurnSpeed * Time.deltaTime, 0f);
        }
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
