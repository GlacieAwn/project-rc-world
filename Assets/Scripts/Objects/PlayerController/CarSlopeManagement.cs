using UnityEngine;

public class CarSlopeManagement : MonoBehaviour
{
    private Transform carTransform;

    public Vector3 GroundNormal { get; private set; } = Vector3.up;
    public float GroundDistance { get; private set; } = Mathf.Infinity;
    public Quaternion GroundOrientation { get; private set; } = Quaternion.identity;
    public bool HasGroundBelow { get; private set; }

    public void Initialize(Transform transformReference)
    {
        carTransform = transformReference;
    }

    public void UpdateSlope()
    {
        if (Physics.Raycast(carTransform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            HasGroundBelow = true;
            GroundNormal = hit.normal;
            GroundDistance = hit.distance;
            Vector3 slopeForward = Vector3.ProjectOnPlane(carTransform.forward, GroundNormal);
            GroundOrientation = slopeForward.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(slopeForward, GroundNormal)
                : Quaternion.FromToRotation(Vector3.up, GroundNormal);
            return;
        }

        HasGroundBelow = false;
        GroundNormal = Vector3.up;
        GroundDistance = Mathf.Infinity;
        GroundOrientation = Quaternion.identity;
    }
}
