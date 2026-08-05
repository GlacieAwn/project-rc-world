using UnityEngine;

public class CarEffects : MonoBehaviour
{
    private Transform frontAxel;
    private Transform rearAxel;
    private Transform car;
    private float axleSpinMultiplier;
    private float rotationLerpSpeed;
    private float frontSpinAngle;
    private float rearSpinAngle;
    private Quaternion carBaseLocalRotation = Quaternion.identity;

    public float CurrentCarRotationAngle { get; private set; }

    public void Initialize(Transform front, Transform rear, Transform carTransform, float spinMultiplier, float lerpSpeed)
    {
        frontAxel = front;
        rearAxel = rear;
        car = carTransform;
        axleSpinMultiplier = spinMultiplier;
        rotationLerpSpeed = lerpSpeed;
        if (car != null)
        {
            carBaseLocalRotation = car.localRotation;
        }
    }

    public void UpdateAxles(float speed, float steeringAngle)
    {
        float magnitude = Mathf.Abs(speed);
        float direction = speed >= 0f ? 1f : -1f;
        
        if (frontAxel != null)
        {
            frontSpinAngle = Mathf.Repeat(frontSpinAngle + direction * magnitude * axleSpinMultiplier * Time.deltaTime, 360f);
            frontAxel.localRotation = Quaternion.Euler(frontSpinAngle, steeringAngle, 0f);
        }
        
        if (rearAxel != null)
        {
            rearSpinAngle = Mathf.Repeat(rearSpinAngle + direction * magnitude * axleSpinMultiplier * Time.deltaTime, 360f);
            rearAxel.localRotation = Quaternion.Euler(rearSpinAngle, 0f, 0f);
        }
    }

    public void ApplyCarRotation(float targetAngle)
    {
        if (car == null)
        {
            return;
        }
        
        CurrentCarRotationAngle = Mathf.Lerp(CurrentCarRotationAngle, targetAngle, rotationLerpSpeed * Time.deltaTime);
        car.localRotation = carBaseLocalRotation * Quaternion.Euler(0f, CurrentCarRotationAngle, 0f);
    }
}
