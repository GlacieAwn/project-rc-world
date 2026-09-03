using System;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// Coordinates overall player movement.
    /// Handles initialization and updates of secondary components.
    /// </summary>
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

    [Header("Car Components")]
    [SerializeField] private CarInput carInput;
    [SerializeField] private CarMovement carMovement;
    [SerializeField] private CarDrift carDrift;
    [SerializeField] private CarBoost carBoost;
    [SerializeField] private CarHeat carHeat;
    [SerializeField] private CarEffects carEffects;
    [SerializeField] private CarSlopeManagement carSlopeManagement;
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private LapManager lapManager;

    private Rigidbody rb;

    public event Action OnOverheated;
    public event Action OnRecoveredFromOverheat;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        carInput = GetOrAddComponent(carInput);
        carMovement = GetOrAddComponent(carMovement);
        carDrift = GetOrAddComponent(carDrift);
        carBoost = GetOrAddComponent(carBoost);
        carHeat = GetOrAddComponent(carHeat);
        carEffects = GetOrAddComponent(carEffects);
        carSlopeManagement = GetOrAddComponent(carSlopeManagement);
        checkpointManager = GetOrAddComponent(checkpointManager);
        lapManager = GetOrAddComponent(lapManager);

        carInput.Initialize();
        carMovement.Initialize(transform, rb, acceleration, deceleration, normalSpeed, maxTurnSpeed,maxTurnAngle, steeringSmoothing, steeringCurveStart, steeringCurveEnd, grip, maxGrip);
        carDrift.Initialize(driftStartSpeed, driftSteeringStrength, driftAngleMultiplier,driftCounterSteerReduction, driftGripReduction, carRotationLerpSpeed, carRotationMaxAngle);
        carBoost.Initialize(normalSpeed, boostSpeed, rampUpTime, boostHoldTime, rampDownTime,cornerTriggerTag, carMovement, carDrift);
        carHeat.Initialize(maxHeat, currentHeat, heatGeneration, passiveCooling,accelerationHeatModifier, driftHeatModifier, boostHeatModifier,decelerationCoolingModifier, overheated);
        carEffects.Initialize(frontAxel, rearAxel, car, axleSpinMultiplier, carRotationLerpSpeed);
        carSlopeManagement.Initialize(transform);

        carHeat.OnOverheated += HandleOverheated;
        carHeat.OnRecoveredFromOverheat += HandleRecoveredFromOverheat;
    }

    private T GetOrAddComponent<T>(T component) where T : Component
    {
        if (component != null)
        {
            return component;
        }

        component = GetComponent<T>();

        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    private void OnEnable()
    {
        if (carInput != null)
        {
            carInput.EnableInput();
        }
    }

    private void OnDisable()
    {
        if (carInput != null)
        {
            carInput.DisableInput();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        carBoost.OnTriggerEnter(other);
        checkpointManager.OnTriggerEnter(other);
        lapManager.OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        carBoost.OnTriggerExit(other);
    }

    public void TriggerBoost()
    {
        carBoost.TriggerBoost();
    }

    public void AddHeat(float amount = 0f)
    {
        carHeat.AddHeat(amount);
    }

    public void CoolHeat(float amount = 0f)
    {
        carHeat.CoolHeat(amount);
    }

    public bool IsOverheated()
    {
        return carHeat.IsOverheated;
    }

    private void Update()
    {
        carSlopeManagement.UpdateSlope();
        UpdateDebugText();

        CarInput.Frame inputFrame = carInput.ReadInput();
        carBoost.UpdateDriftRelease(inputFrame.DriftHeld);
        carMovement.UpdateSpeed(inputFrame.Accelerating, inputFrame.Reversing);
        carHeat.UpdateHeat(inputFrame.Accelerating, inputFrame.Reversing, carMovement.CurrentSpeed, carDrift.IsHolding, carBoost.IsActive);
        carMovement.UpdateMovement(inputFrame.Steering, carDrift.IsHolding, carDrift.Direction, carDrift.CounterSteerReduction, carDrift.SteeringStrength);
        carEffects.UpdateAxles(carMovement.CurrentSpeed, carMovement.CurrentFrontAxelSteerAngle);
        carDrift.UpdateDrift(inputFrame.DriftHeld, inputFrame.Steering, carMovement.CurrentSpeed, carEffects.CurrentCarRotationAngle);
        carEffects.ApplyCarRotation(carDrift.TargetCarRotationAngle);
    }

    private void FixedUpdate()
    {
        carMovement.ClampRigidbodyVelocity();
    }

    private void UpdateDebugText()
    {
        Vector3 movementDirection = transform.forward;
        movementDirection.y = 0f;

        if (movementDirection.sqrMagnitude > 0.0001f)
        {
            movementDirection.Normalize();
        }
        else
        {
            movementDirection = Vector3.forward;
        }

        float signedSpeed = Vector3.Dot(rb.linearVelocity, movementDirection);

        debugText.text =
            "RC Car Debug Values:\n" +
            $"\nRotation: {transform.eulerAngles}" +
            $"\nPosition: {transform.position}\n" +
            $"Velocity: {rb.linearVelocity}\n" +
            $"Speed: {carMovement.CurrentSpeed:F4}\n" +
            $"Angular: {rb.angularVelocity}\n" +
            $"Sleeping: {rb.IsSleeping()}" +
            $"SignedSpeed: {signedSpeed}\n" +
            $"Ground Normal: {carSlopeManagement.GroundNormal}\n" +
            $"Ground Distance: {carSlopeManagement.GroundDistance:F4}\n" +
            $"Steering: {carInput.Steering}\n" +
            $"Current Drift State: {carDrift.State}\n" +
            $"Heat: {carHeat.CurrentHeat}\n" +
            $"Overheated: {carHeat.IsOverheated}\n";
    }

    private void HandleOverheated()
    {
        OnOverheated?.Invoke();
    }

    private void HandleRecoveredFromOverheat()
    {
        OnRecoveredFromOverheat?.Invoke();
    }

	
}
