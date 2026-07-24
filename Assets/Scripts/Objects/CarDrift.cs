using UnityEngine;

public class CarDrift : MonoBehaviour
{
    public enum DriftState { None, Holding, Released }
    private float driftStartSpeed; private float driftAngleMultiplier; private float driftGripReduction; private float rotationLerpSpeed; private float rotationMaxAngle; private float driftChargeTime;
    public float SteeringStrength { get; private set; }
    public float CounterSteerReduction { get; private set; }
    public float Direction { get; private set; }
    public float TargetCarRotationAngle { get; private set; }
    public DriftState State { get; private set; }
    public bool IsHolding { get { return State == DriftState.Holding; } }
    public void Initialize(float startSpeed, float strength, float angleMultiplier, float counterSteerReduction, float gripReduction, float lerpSpeed, float maxAngle) { driftStartSpeed = startSpeed; SteeringStrength = strength; driftAngleMultiplier = angleMultiplier; CounterSteerReduction = counterSteerReduction; driftGripReduction = gripReduction; rotationLerpSpeed = lerpSpeed; rotationMaxAngle = maxAngle; }
    public void UpdateDrift(bool driftHeld, float steering, float speed, float currentAngle)
    {
        switch (State)
        {
            case DriftState.None:
                if (driftHeld && Mathf.Abs(steering) > 0.1f && Mathf.Abs(speed) > driftStartSpeed) { Direction = Mathf.Sign(steering); driftChargeTime = 0f; TargetCarRotationAngle = 0f; State = DriftState.Holding; }
                break;
            case DriftState.Holding:
                driftChargeTime += Time.deltaTime; float target = currentAngle; float gripScale = Mathf.Lerp(1f, 1.35f, Mathf.Clamp01(driftGripReduction));
                if (Mathf.Abs(steering) > 0.001f) { float sign = Mathf.Sign(steering); float magnitude = Mathf.Abs(steering); float maxDriftAngle = rotationMaxAngle * driftAngleMultiplier * gripScale; if (sign == Direction) target = Mathf.Max(Mathf.Abs(currentAngle), Mathf.Clamp(magnitude * maxDriftAngle, 0f, maxDriftAngle)) * Direction; else { float counter = Mathf.Clamp01(magnitude * CounterSteerReduction); float reduced = Mathf.Max(Mathf.Abs(currentAngle) * (1f - counter), 0f); float opposite = Mathf.Max(rotationMaxAngle * 0.15f * counter, 0.25f); target = (reduced > 0.0001f ? Mathf.Sign(currentAngle) : Direction) * Mathf.Max(reduced, opposite); } }
                TargetCarRotationAngle = Mathf.Lerp(TargetCarRotationAngle, target, rotationLerpSpeed * Time.deltaTime); if (!driftHeld) State = DriftState.Released;
                break;
            case DriftState.Released:
                Debug.Log("Released!"); TargetCarRotationAngle = 0f; driftChargeTime = 0f; Direction = 0f; State = DriftState.None;
                break;
        }
    }
}
