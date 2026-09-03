using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 rotationOffset = new Vector3(0f, 0f, 0f);

    [SerializeField] private float smoothTime = 0.02f;

    private Vector3 cameraVelocity;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position + target.rotation * positionOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref cameraVelocity, smoothTime);
        transform.rotation = target.rotation * Quaternion.Euler(rotationOffset);
    }
}
