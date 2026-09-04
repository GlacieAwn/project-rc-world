using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private Transform carTransform;
    private BoxCollider carCollider;
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
    private LayerMask groundLayer;
    private float groundRayLength;
    private float groundRayStartHeight;
    private float slopeRotationSpeed;
    private float slopePositionSpeed;
    private float groundClearance;
    private float maxSlopeAngle;
    private Vector3 velocityDirection;
    private Vector3 groundNormal = Vector3.up;
    private Vector3 groundPoint;
    private bool isGrounded;
    private float slopeAngle;

    public float CurrentSpeed { get; private set; }
    public float CurrentMaxSpeed { get; private set; }
    public float CurrentFrontAxelSteerAngle { get; private set; }
    public bool HasMovement { get; private set; } = true;
    public bool IsGrounded => isGrounded;
    public float SlopeAngle => slopeAngle;
    public Vector3 GroundNormal => groundNormal;

    public void Initialize(Transform transformReference, Rigidbody rigidbody, float accelerationValue,
        float decelerationValue, float normalSpeed, float turnSpeed, float turnAngle,
        float smoothing, float curveStart, float curveEnd, float gripValue, float maxGripValue,
        LayerMask groundLayerValue, float groundRayLengthValue, float groundRayStartHeightValue,
        float slopeRotationSpeedValue, float slopePositionSpeedValue, float groundClearanceValue,
        float maxSlopeAngleValue)
    {
        carTransform = transformReference;
        carCollider = carTransform.GetComponent<BoxCollider>();
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
        groundLayer = groundLayerValue;
        groundRayLength = groundRayLengthValue;
        groundRayStartHeight = groundRayStartHeightValue;
        slopeRotationSpeed = slopeRotationSpeedValue;
        slopePositionSpeed = slopePositionSpeedValue;
        groundClearance = groundClearanceValue;
        maxSlopeAngle = maxSlopeAngleValue;
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
        UpdateGroundInfo();

        if (velocityDirection == Vector3.zero)
        {
            velocityDirection = carTransform.forward;
        }

        float gripRate = grip * maxGrip;
        velocityDirection = Vector3.Slerp(velocityDirection, carTransform.forward, gripRate * Time.deltaTime);

        if (isGrounded)
        {
            velocityDirection = ProjectDirectionOntoGround(velocityDirection);
        }

        float speedMagnitude = Mathf.Abs(CurrentSpeed);
        float targetSteeringAngle = Mathf.Clamp(steering, -1f, 1f) * maxTurnAngle;
        CurrentFrontAxelSteerAngle = Mathf.Lerp(CurrentFrontAxelSteerAngle, targetSteeringAngle, steeringSmoothing * Time.deltaTime);
        float steeringPercent = maxTurnAngle > 0f ? CurrentFrontAxelSteerAngle / maxTurnAngle : 0f;
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
            if (speedMagnitude > 0.0001f)
            {
                ApplySteering(steeringTurn);
            }

            carTransform.position += velocityDirection * CurrentSpeed * Time.deltaTime;
            UpdateGroundInfo();
            ApplySlopePosition();
            ApplySlopeRotation();
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

    private void UpdateGroundInfo()
    {
        // The car root is not at the bottom of its collider.  Begin the ray at the
        // collider's world-space bottom center instead, so a short ray samples the
        // surface the car is actually resting on rather than geometry below it.
        Bounds colliderBounds = carCollider.bounds;
        Vector3 rayOrigin = new Vector3(
            colliderBounds.center.x,
            colliderBounds.min.y + groundRayStartHeight,
            colliderBounds.center.z);
        Vector3 rayDirection = Vector3.down;
        Debug.DrawRay(rayOrigin, rayDirection * groundRayLength, Color.yellow);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, groundRayLength, groundLayer,
                QueryTriggerInteraction.Ignore))
        {
            groundNormal = hit.normal;
            groundPoint = hit.point;
            slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            isGrounded = slopeAngle <= maxSlopeAngle;
            // Debug.Log(
            //     $"Ground Ray Hit | Origin: {rayOrigin} | Direction: {rayDirection} | Length: {groundRayLength:F2} | " +
            //     $"Collider: {hit.collider.name} | Point: {hit.point} | Normal: {hit.normal} | " +
            //     $"Slope Angle: {slopeAngle:F2}");
            return;
        }

        isGrounded = false;
        groundNormal = Vector3.up;
        slopeAngle = 0f;
        // Debug.Log(
        //     $"Ground Ray Miss | Origin: {rayOrigin} | Direction: {rayDirection} | Length: {groundRayLength:F2} | " +
        //     "Collider: None | Point: N/A | Normal: N/A | Slope Angle: N/A");
    }

    private Vector3 ProjectDirectionOntoGround(Vector3 direction)
    {
        Vector3 projectedDirection = Vector3.ProjectOnPlane(direction, groundNormal);

        if (projectedDirection.sqrMagnitude < 0.0001f)
        {
            projectedDirection = Vector3.ProjectOnPlane(carTransform.forward, groundNormal);
        }

        return projectedDirection.sqrMagnitude > 0.0001f ? projectedDirection.normalized : carTransform.forward;
    }

    private void ApplySteering(float steeringTurn)
    {
        Vector3 steeringAxis = isGrounded ? groundNormal : Vector3.up;
        carTransform.Rotate(steeringAxis, steeringTurn * maxTurnSpeed * Time.deltaTime, Space.World);
    }

    private void ApplySlopeRotation()
    {
        if (!isGrounded)
        {
            return;
        }

        Vector3 forward = Vector3.ProjectOnPlane(carTransform.forward, groundNormal);

        if (forward.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(forward.normalized, groundNormal);
        carTransform.rotation = Quaternion.Slerp(carTransform.rotation, targetRotation, slopeRotationSpeed * Time.deltaTime);
    }

    private void ApplySlopePosition()
    {
        if (!isGrounded)
        {
            return;
        }

        float targetHeight = groundPoint.y + groundClearance;
        Vector3 targetPosition = carTransform.position;
        targetPosition.y = targetHeight;
        carTransform.position = Vector3.Lerp(carTransform.position, targetPosition, slopePositionSpeed * Time.deltaTime);
    }

    private float CalculateSteeringAuthority(float speedRatio)
    {
        if (steeringCurveStart > 0f && speedRatio < steeringCurveStart)
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
