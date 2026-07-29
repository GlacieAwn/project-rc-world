using UnityEngine;

/// <summary>
/// Owns the vertical part of an arcade vehicle's movement: ground detection,
/// terrain alignment, ride height, and gravity. It deliberately does not know
/// anything about steering, acceleration, drifting, or collision response.
/// </summary>
public class CarGrounding : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField, Tooltip("Downward acceleration applied while the vehicle is not near the ground.")]
    private float gravity = -30f;

    [Header("Ground Probe")]
    [SerializeField, Min(0f), Tooltip("Distance from the transform used as the desired clearance above the surface.")]
    private float rideHeight = 0.6f;
    [SerializeField, Min(0.01f), Tooltip("Radius of the downward sphere used to find terrain. Larger values are more forgiving on uneven tracks.")]
    private float sphereCastRadius = 0.35f;
    [SerializeField, Min(0f), Tooltip("Moves the cast origin upward so the probe can still see ground when the vehicle is close to it.")]
    private float castStartOffset = 1f;
    [SerializeField, Min(0.01f), Tooltip("Maximum distance below the cast origin at which terrain counts as grounded.")]
    private float groundCheckDistance = 3f;
    [SerializeField] private LayerMask groundLayers = ~0;

    [Header("Terrain Alignment")]
    [SerializeField, Min(0f), Tooltip("How quickly the vehicle rotates its up direction toward the terrain normal.")]
    private float alignmentSpeed = 12f;

    public bool IsGrounded { get; private set; }
    public Vector3 GroundNormal { get; private set; } = Vector3.up;
    public float VerticalVelocity { get; private set; }

    /// <summary>
    /// Call once per frame after horizontal movement has been applied.
    /// This lets the probe evaluate the vehicle's final horizontal position.
    /// </summary>
    public void UpdateGrounding(float deltaTime)
    {
        Debug.Log(transform.position.y);
        Debug.Log(IsGrounded);
        
        if (TryGetGround(out RaycastHit groundHit))
        {
            IsGrounded = true;
            GroundNormal = groundHit.normal;

            // Place the vehicle at a fixed clearance along the surface normal.
            // This is intentionally a snap: arcade cars feel planted and do not
            // need suspension oscillation to be convincing.
            transform.position = groundHit.point + GroundNormal * rideHeight;

            // Rotate only enough each frame to keep slopes and crests smooth.
            // FromToRotation preserves the current heading as much as possible
            // while changing the vehicle's up vector to match the terrain.
            Quaternion terrainRotation = Quaternion.FromToRotation(transform.up, GroundNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, terrainRotation,
                1f - Mathf.Exp(-alignmentSpeed * deltaTime));

            // A ground hit cancels accumulated falling speed, preventing bounce
            // or a continued downward push after landing.
            VerticalVelocity = 0f;
            Debug.Log(groundHit.collider.name);
            return;
        }

        IsGrounded = false;
        GroundNormal = Vector3.up;

        // Integrating velocity gives predictable, Rigidbody-free falling.
        VerticalVelocity += gravity * deltaTime;
        transform.position += Vector3.up * VerticalVelocity * deltaTime;

    }

    private bool TryGetGround(out RaycastHit groundHit)
    {
        Vector3 castOrigin = transform.position + Vector3.up * castStartOffset;

        return Physics.SphereCast(castOrigin, sphereCastRadius, Vector3.down, out groundHit,
            groundCheckDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 castOrigin = transform.position + Vector3.up * castStartOffset;
        Gizmos.color = IsGrounded ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(castOrigin, sphereCastRadius);
        Gizmos.DrawLine(castOrigin, castOrigin + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(castOrigin + Vector3.down * groundCheckDistance, sphereCastRadius);
    }

}
