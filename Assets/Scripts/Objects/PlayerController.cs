using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f;
    [SerializeField] private float normalSpeed = 20f;
    [SerializeField] private float boostSpeed = 30f;
    [SerializeField] private float rampUpTime = 0.3f;
    [SerializeField] private float boostHoldTime = 0.6f;
    [SerializeField] private float rampDownTime = 0.3f;
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

    [Header("Boost Trigger")]
    [SerializeField] private string cornerTriggerTag = "Corner";

    [Header("Vehicle Heat")]
    [SerializeField] private float maxHeat = 100f;
    [SerializeField] private float currentHeat;
    [SerializeField] private float heatGeneration = 5f;
    [SerializeField] private float passiveCooling = 2f;
    [SerializeField] private float accelerationHeatModifier = 1f;
    [SerializeField] private float driftHeatModifier = 1f;
    [SerializeField] private float boostHeatModifier = 1f;
    [SerializeField] private float decelerationCoolingModifier = 1f;
    [SerializeField] private bool overheated;

    [Header("Object Assignment")]
    [SerializeField] private Transform frontAxel;
    [SerializeField] private Transform rearAxel;
    [SerializeField] private Transform car;
    [SerializeField] private float carRotationLerpSpeed = 8f;
    [SerializeField] private float carRotationMaxAngle = 30f;

    private float currentSpeed;
    private float currentMaxSpeed;
    private bool isAccelerating;
    private bool isReversing;
    private bool boostActive;
    private Coroutine boostRoutine;
    private bool isInsideCornerTrigger;
    private bool cornerBoostTriggered;
    private bool previousDriftInputHeld;

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
        currentMaxSpeed = normalSpeed;

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

    private void OnTriggerEnter(Collider other)
    {
        // Entering a corner trigger prepares the boost request for the next drift release.
        if (!IsCornerTrigger(other))
        {
            return;
        }

        isInsideCornerTrigger = true;
        cornerBoostTriggered = false;
    }

    private void OnTriggerExit(Collider other)
    {
        // Leaving the trigger cancels the pending corner boost request.
        if (!IsCornerTrigger(other))
        {
            return;
        }

        isInsideCornerTrigger = false;
        cornerBoostTriggered = false;
    }

    public void TriggerBoost()
    {
        // This is the shared entry point for future boost sources such as corner trigger zones.
        if (boostActive || boostRoutine != null)
        {
            return;
        }

        boostActive = true;
        boostRoutine = StartCoroutine(BoostRoutine());
    }

    private IEnumerator BoostRoutine()
    {
        // Ramp up to the boosted speed smoothly.
        float elapsed = 0f;
        while (elapsed < rampUpTime)
        {
            float t = rampUpTime > 0f ? elapsed / rampUpTime : 1f;
            currentMaxSpeed = Mathf.Lerp(normalSpeed, boostSpeed, t);
            currentSpeed = Mathf.Clamp(currentSpeed, -currentMaxSpeed, currentMaxSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentMaxSpeed = boostSpeed;
        currentSpeed = Mathf.Clamp(currentSpeed, -currentMaxSpeed, currentMaxSpeed);

        // Hold the boosted speed briefly.
        yield return new WaitForSeconds(boostHoldTime);

        // Ramp back down to the normal speed smoothly.
        elapsed = 0f;
        while (elapsed < rampDownTime)
        {
            float t = rampDownTime > 0f ? elapsed / rampDownTime : 1f;
            currentMaxSpeed = Mathf.Lerp(boostSpeed, normalSpeed, t);
            currentSpeed = Mathf.Clamp(currentSpeed, -currentMaxSpeed, currentMaxSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentMaxSpeed = normalSpeed;
        currentSpeed = Mathf.Clamp(currentSpeed, -currentMaxSpeed, currentMaxSpeed);
        boostActive = false;
        boostRoutine = null;
    }

    public event System.Action OnOverheated;
    public event System.Action OnRecoveredFromOverheat;

    public void AddHeat(float amount = 0f)
    {
        // Gameplay systems can call this with any tuned value later without changing the heat logic.
        float heatToAdd = amount > 0f ? amount : heatGeneration;
        currentHeat = Mathf.Clamp(currentHeat + heatToAdd, 0f, maxHeat);
        UpdateOverheatedState();
    }

    public void CoolHeat(float amount = 0f)
    {
        // Gameplay systems can call this with any tuned value later without changing the heat logic.
        float heatToRemove = amount > 0f ? amount : passiveCooling;
        currentHeat = Mathf.Clamp(currentHeat - heatToRemove, 0f, maxHeat);
        UpdateOverheatedState();
    }

    public bool IsOverheated()
    {
        return overheated;
    }

    private void UpdateOverheatedState()
    {
        bool nextOverheated = currentHeat >= maxHeat;
        if (overheated != nextOverheated)
        {
            overheated = nextOverheated;

            if (overheated)
            {
                // TODO: apply the overheat gameplay effect here.
                OnOverheated?.Invoke();
            }
            else
            {
                OnRecoveredFromOverheat?.Invoke();
            }
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
            $"Steering: {steering}\n" +
            $"Current Drift State: {currentDriftState}\n" + 
            $"Heat: {currentHeat}\n" +
            $"Overheated: {overheated}\n";


        if (input == null)
        {
            return;
        }

        isAccelerating = input.Player.Accelerate.IsPressed();
        isReversing = input.Player.Reverse.IsPressed();
        steering = input.Player.Steer.ReadValue<float>();
        bool driftInputHeld = input.Player.Drift.IsPressed();

        if (previousDriftInputHeld && !driftInputHeld)
        {
            TryTriggerCornerBoost();
        }

        previousDriftInputHeld = driftInputHeld;

        UpdateSpeed(isAccelerating, isReversing);
        ApplyHeatFromInputs(isAccelerating, isReversing);
        ApplyCoolingFromInputs(isAccelerating, isReversing);

        if (velocityDirection == Vector3.zero)
            velocityDirection = transform.forward;

        float gripRate = grip * maxGrip;
        velocityDirection = Vector3.Slerp(velocityDirection, transform.forward, gripRate * Time.deltaTime);

        transform.position += velocityDirection * currentSpeed * Time.deltaTime;


        float speedMagnitude = Mathf.Abs(currentSpeed);
        float targetSteeringAngle = Mathf.Clamp(steering, -1f, 1f) * maxTurnAngle;
        currentFrontAxelSteerAngle = Mathf.Lerp(currentFrontAxelSteerAngle, targetSteeringAngle, steeringSmoothing * Time.deltaTime);

        float steeringPercent = currentFrontAxelSteerAngle / maxTurnAngle;
        float speedRatio = currentMaxSpeed > 0f ? Mathf.Clamp01(speedMagnitude / currentMaxSpeed) : 0f;

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
            steeringTurn = driftDirection * Mathf.Clamp01(driftInfluence * driftSteeringStrength * steeringAuthority);
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
                Debug.Log("Released!");
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
            // Debug.Log(rb.linearVelocity);
        }

        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, currentMaxSpeed);

    }

    private void TryTriggerCornerBoost()
    {
        // Only trigger a corner boost if the player was drifting and has just released the drift input while inside the zone.
        if (!isInsideCornerTrigger || cornerBoostTriggered || input == null)
        {
            return;
        }

        bool driftInputHeld = input.Player.Drift.IsPressed();
        bool wasDriftingBeforeRelease = currentDriftState == DRIFT_STATE.Holding;

        if (!wasDriftingBeforeRelease || driftInputHeld)
        {
            return;
        }

        cornerBoostTriggered = true;
        TriggerBoost();
    }

    private bool IsCornerTrigger(Collider other)
    {
        return !string.IsNullOrEmpty(cornerTriggerTag) && other.CompareTag(cornerTriggerTag);
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

    private void ApplyHeatFromInputs(bool accelerating, bool reversing)
    {
        if (accelerating && !reversing)
        {
            AddHeat(heatGeneration * accelerationHeatModifier);
        }

        if (currentDriftState == DRIFT_STATE.Holding)
        {
            AddHeat(heatGeneration * driftHeatModifier);
        }

        if (boostActive)
        {
            AddHeat(heatGeneration * boostHeatModifier);
        }
    }

    private void ApplyCoolingFromInputs(bool accelerating, bool reversing)
    {
        bool isDecelerating = (!accelerating && reversing) || (accelerating && reversing) || (!accelerating && !reversing && Mathf.Abs(currentSpeed) > 0.0001f);

        if (isDecelerating)
        {
            CoolHeat(passiveCooling * decelerationCoolingModifier);
        }
    }

    private void UpdateSpeed(bool accelerating, bool reversing)
    {
        if (accelerating && !reversing)
            currentSpeed += acceleration * Time.deltaTime;
        else if (reversing && !accelerating)
            currentSpeed -= acceleration * Time.deltaTime;
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);

        currentSpeed = Mathf.Clamp(currentSpeed, -currentMaxSpeed, currentMaxSpeed);
    }
}
