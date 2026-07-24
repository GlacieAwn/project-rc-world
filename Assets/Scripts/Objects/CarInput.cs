using UnityEngine;

public class CarInput : MonoBehaviour
{
    public struct Frame
    {
        public bool Accelerating;
        public bool Reversing;
        public float Steering;
        public bool DriftHeld;
    }

    private InputSystem_Actions input;

    public float Steering { get; private set; }

    public void Initialize()
    {
        input = new InputSystem_Actions();
    }

    public void EnableInput()
    {
        input.Enable();
    }

    public void DisableInput()
    {
        input.Disable();
    }

    public Frame ReadInput()
    {
        Frame frame = new Frame();
        frame.Accelerating = input.Player.Accelerate.IsPressed();
        frame.Reversing = input.Player.Reverse.IsPressed();
        frame.Steering = input.Player.Steer.ReadValue<float>();
        frame.DriftHeld = input.Player.Drift.IsPressed();
        Steering = frame.Steering;
        return frame;
    }
}
