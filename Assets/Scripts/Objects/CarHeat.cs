using System;
using UnityEngine;
public class CarHeat : MonoBehaviour
{
    private float maxHeat; private float heatGeneration; private float passiveCooling; private float accelerationModifier; private float driftModifier; private float boostModifier; private float decelerationModifier;
    public float CurrentHeat { get; private set; } public bool IsOverheated { get; private set; } public event Action OnOverheated; public event Action OnRecoveredFromOverheat;
    public void Initialize(float maximum, float current, float generation, float cooling, float acceleration, float drift, float boost, float deceleration, bool overheated) { maxHeat = maximum; CurrentHeat = current; heatGeneration = generation; passiveCooling = cooling; accelerationModifier = acceleration; driftModifier = drift; boostModifier = boost; decelerationModifier = deceleration; IsOverheated = overheated; }
    public void UpdateHeat(bool accelerating, bool reversing, float speed, bool drifting, bool boosting) { if (accelerating && !reversing) AddHeat(heatGeneration * accelerationModifier); if (drifting) AddHeat(heatGeneration * driftModifier); if (boosting) AddHeat(heatGeneration * boostModifier); bool decelerating = (!accelerating && reversing) || (accelerating && reversing) || (!accelerating && !reversing && Mathf.Abs(speed) > 0.0001f); if (decelerating) CoolHeat(passiveCooling * decelerationModifier); }
    public void AddHeat(float amount = 0f) { float value = amount > 0f ? amount : heatGeneration; CurrentHeat = Mathf.Clamp(CurrentHeat + value, 0f, maxHeat); UpdateOverheatedState(); }
    public void CoolHeat(float amount = 0f) { float value = amount > 0f ? amount : passiveCooling; CurrentHeat = Mathf.Clamp(CurrentHeat - value, 0f, maxHeat); UpdateOverheatedState(); }
    private void UpdateOverheatedState() { bool next = CurrentHeat >= maxHeat; if (IsOverheated == next) return; IsOverheated = next; if (IsOverheated) OnOverheated?.Invoke(); else OnRecoveredFromOverheat?.Invoke(); }
}
