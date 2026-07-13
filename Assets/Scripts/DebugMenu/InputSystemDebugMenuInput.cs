using UnityEngine.InputSystem;

namespace RCWorld.DebugMenu
{
    // Default Input System implementation. Bindings live here, outside of menu logic, and can be replaced with any IDebugMenuInput implementation.
    public sealed class InputSystemDebugMenuInput : IDebugMenuInput
    {
        private readonly InputAction openCloseAction;
        private readonly InputAction upAction;
        private readonly InputAction downAction;
        private readonly InputAction leftAction;
        private readonly InputAction rightAction;
        private readonly InputAction submitAction;
        private readonly InputAction backAction;

        // Creates the development input profile. F1 opens the menu; arrows/WASD and gamepad controls navigate it.
        public InputSystemDebugMenuInput()
        {
            openCloseAction = CreateButtonAction("Debug Menu Open", "<Keyboard>/f1", "<Gamepad>/start");
            upAction = CreateButtonAction("Debug Menu Up", "<Keyboard>/upArrow", "<Keyboard>/w", "<Gamepad>/dpad/up", "<Gamepad>/leftStick/up");
            downAction = CreateButtonAction("Debug Menu Down", "<Keyboard>/downArrow", "<Keyboard>/s", "<Gamepad>/dpad/down", "<Gamepad>/leftStick/down");
            leftAction = CreateButtonAction("Debug Menu Left", "<Keyboard>/leftArrow", "<Keyboard>/a", "<Gamepad>/dpad/left", "<Gamepad>/leftStick/left");
            rightAction = CreateButtonAction("Debug Menu Right", "<Keyboard>/rightArrow", "<Keyboard>/d", "<Gamepad>/dpad/right", "<Gamepad>/leftStick/right");
            submitAction = CreateButtonAction("Debug Menu Submit", "<Keyboard>/enter", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            backAction = CreateButtonAction("Debug Menu Back", "<Keyboard>/escape", "<Keyboard>/backspace", "<Gamepad>/buttonEast");

            EnableAll();
        }

        public bool OpenClosePressedThisFrame => openCloseAction.WasPressedThisFrame();
        public bool NavigateUpPressedThisFrame => upAction.WasPressedThisFrame();
        public bool NavigateDownPressedThisFrame => downAction.WasPressedThisFrame();
        public bool NavigateLeftPressedThisFrame => leftAction.WasPressedThisFrame();
        public bool NavigateRightPressedThisFrame => rightAction.WasPressedThisFrame();
        public bool SubmitPressedThisFrame => submitAction.WasPressedThisFrame();
        public bool BackPressedThisFrame => backAction.WasPressedThisFrame();

        public void Dispose()
        {
            openCloseAction.Dispose(); upAction.Dispose(); downAction.Dispose(); leftAction.Dispose(); rightAction.Dispose(); submitAction.Dispose(); backAction.Dispose();
        }

        private static InputAction CreateButtonAction(string name, params string[] bindings)
        {
            InputAction action = new InputAction(name, InputActionType.Button);
            foreach (string binding in bindings)
                action.AddBinding(binding);
            return action;
        }

        private void EnableAll()
        {
            openCloseAction.Enable(); upAction.Enable(); downAction.Enable(); leftAction.Enable(); rightAction.Enable(); submitAction.Enable(); backAction.Enable();
        }
    }
}
