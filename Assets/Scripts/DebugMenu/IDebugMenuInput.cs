using System;

namespace RCWorld.DebugMenu
{
    // Input contract consumed by the debug menu. Implement this interface to use a different activation method.
    public interface IDebugMenuInput : IDisposable
    {
        bool OpenClosePressedThisFrame { get; }
        bool NavigateUpPressedThisFrame { get; }
        bool NavigateDownPressedThisFrame { get; }
        bool NavigateLeftPressedThisFrame { get; }
        bool NavigateRightPressedThisFrame { get; }
        bool SubmitPressedThisFrame { get; }
        bool BackPressedThisFrame { get; }
    }
}
