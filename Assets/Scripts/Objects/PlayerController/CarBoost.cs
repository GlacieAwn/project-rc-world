using System.Collections;
using UnityEngine;

public class CarBoost : MonoBehaviour
{
    private float normalSpeed;
    private float boostSpeed;
    private float rampUpTime;
    private float holdTime;
    private float rampDownTime;
    private string cornerTriggerTag;
    private CarMovement movement;
    private CarDrift drift;
    private Coroutine boostRoutine;
    private bool isInsideCorner;
    private bool cornerBoostTriggered;
    private bool previousDriftInputHeld;

    public bool IsActive { get; private set; }

    public void Initialize(float normal, float boosted, float rampUp, float hold, float rampDown, string tag, CarMovement movementReference, CarDrift driftReference)
    {
        normalSpeed = normal;
        boostSpeed = boosted;
        rampUpTime = rampUp;
        holdTime = hold;
        rampDownTime = rampDown;
        cornerTriggerTag = tag;
        movement = movementReference;
        drift = driftReference;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!IsCornerTrigger(other)) return;
        isInsideCorner = true;
        cornerBoostTriggered = false;
    }

    public void OnTriggerExit(Collider other)
    {
        if (!IsCornerTrigger(other)) return;
        isInsideCorner = false;
        cornerBoostTriggered = false;
    }

    public void UpdateDriftRelease(bool driftHeld)
    {
        if (previousDriftInputHeld && !driftHeld && isInsideCorner && !cornerBoostTriggered && drift.IsHolding)
        {
            cornerBoostTriggered = true;
            TriggerBoost();
        }
        previousDriftInputHeld = driftHeld;
    }

    public void TriggerBoost()
    {
        if (IsActive || boostRoutine != null) return;
        IsActive = true;
        boostRoutine = StartCoroutine(BoostRoutine());
    }

    private IEnumerator BoostRoutine()
    {
        float elapsed = 0f;
        while (elapsed < rampUpTime)
        {
            float t = rampUpTime > 0f ? elapsed / rampUpTime : 1f;
            movement.SetMaxSpeed(Mathf.Lerp(normalSpeed, boostSpeed, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        movement.SetMaxSpeed(boostSpeed);
        yield return new WaitForSeconds(holdTime);

        elapsed = 0f;
        while (elapsed < rampDownTime)
        {
            float t = rampDownTime > 0f ? elapsed / rampDownTime : 1f;
            movement.SetMaxSpeed(Mathf.Lerp(boostSpeed, normalSpeed, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        movement.SetMaxSpeed(normalSpeed);
        IsActive = false;
        boostRoutine = null;
    }

    private bool IsCornerTrigger(Collider other)
    {
        return !string.IsNullOrEmpty(cornerTriggerTag) && other.CompareTag(cornerTriggerTag);
    }
}
