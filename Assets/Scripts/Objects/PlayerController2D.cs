using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using Unity.Mathematics;

public class PlayerController2D : MonoBehaviour
{
    [Header("Spline")]
    [SerializeField] private SplineContainer spline;

    [Header("Movement")]
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f;
    [SerializeField] private float maxSpeed = 20f;

    [Header("Misc")]
    [SerializeField] private float bottomOffset;

    private float currentSpeed;
    private float distance;
    private float splineLength;
    private bool isAccelerating;

    private InputSystem_Actions input;

    private void Awake()
    {
        input = new InputSystem_Actions();

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

    private void Start()
    {
        splineLength = spline.CalculateLength();
        bottomOffset = GetMeshBottomOffset();
    }


	private void Update()
	{
		if (input == null)
        {
            return;
        }

        // float accelerationValue = input.Player.Accelerate.ReadValue<float>();
        isAccelerating = input.Player.Accelerate.IsPressed();

        UpdateSpeed(isAccelerating);

        // Move along the spline after speed has been updated
        UpdateDistance();

        

	}
    private float GetMeshBottomOffset()
    {
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer == null)
            return 0f;

        return renderer.bounds.size.y * 0.5f;
    }

    private void UpdateSpeed(bool accelerating)
    {
        if (accelerating)
            currentSpeed += acceleration * Time.deltaTime;
        else
            currentSpeed -= deceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
    }

    private void UpdateDistance()
    {
        distance += currentSpeed * Time.deltaTime;

        if (distance >= splineLength)
        {
            distance -= splineLength;
        }

        // Convert the linear distance along the spline into a normalized 0..1 parameter
        FollowSpline(distance);
    }

    private void FollowSpline(float distanceAlongSpline)
    {

        float t = distanceAlongSpline / splineLength;

        if (spline.Spline.Closed)
            t = Mathf.Repeat(t, 1f);
        else
            t = Mathf.Clamp01(t);

        float3 posF3;
        float3 tanF3;
        float3 upF3;

        spline.Evaluate(t, out posF3, out tanF3, out upF3);

        Vector3 position = new Vector3(posF3.x, posF3.y + bottomOffset, posF3.z);
        Vector3 tangent = new Vector3(tanF3.x, tanF3.y, tanF3.z).normalized;
        Vector3 up = new Vector3(upF3.x, upF3.y, upF3.z).normalized;

        transform.position = position;

        if (tangent.sqrMagnitude > 0f)
            transform.rotation = Quaternion.LookRotation(tangent, up) * Quaternion.Euler(0f, -90f, 0f);
    }

    
}
