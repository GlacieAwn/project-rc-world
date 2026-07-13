using System;

namespace RCWorld.DebugMenu
{
    // Public entry point for registering runtime debug menu content.
    public static class DebugMenu
    {
        // Registers a command that is invoked when selected.
        public static DebugItemAction RegisterAction(string categoryPath, string name, Action action)
        {
            return DebugMenuRegistry.Register(new DebugItemAction(name, action), categoryPath);
        }

        // Registers a Boolean value that can be switched on and off.
        public static DebugItemToggle RegisterToggle(string categoryPath, string name, Func<bool> getValue, Action<bool> setValue)
        {
            return DebugMenuRegistry.Register(new DebugItemToggle(name, getValue, setValue), categoryPath);
        }

        // Registers a floating point slider.
        public static DebugItemFloatSlider RegisterSlider(string categoryPath, string name, Func<float> getValue, Action<float> setValue, float minimum, float maximum, float step = 0.1f)
        {
            return DebugMenuRegistry.Register(new DebugItemFloatSlider(name, getValue, setValue, minimum, maximum, step), categoryPath);
        }

        // Registers an integer value that is adjusted in fixed increments.
        public static DebugItemInteger RegisterInteger(string categoryPath, string name, Func<int> getValue, Action<int> setValue, int minimum, int maximum, int step = 1)
        {
            return DebugMenuRegistry.Register(new DebugItemInteger(name, getValue, setValue, minimum, maximum, step), categoryPath);
        }

        // Registers an enum selector.
        public static DebugItemEnum<TEnum> RegisterEnum<TEnum>(string categoryPath, string name, Func<TEnum> getValue, Action<TEnum> setValue) where TEnum : struct, Enum
        {
            return DebugMenuRegistry.Register(new DebugItemEnum<TEnum>(name, getValue, setValue), categoryPath);
        }

        // Registers a scene load command using a scene name included in the build settings.
        public static DebugItemScene RegisterScene(string categoryPath, string name, string sceneName)
        {
            return DebugMenuRegistry.Register(new DebugItemScene(name, sceneName), categoryPath);
        }

        // Replaces the source used to open and navigate the menu. The menu takes ownership of the input instance.
        public static void SetInput(IDebugMenuInput input)
        {
            DebugMenuManager.Instance.SetInput(input);
        }

        // Configures menu behavior without coupling the framework to a particular game pause or cursor system.
        public static void ConfigureModalBehavior(bool pauseGameplayWhenOpen, bool unlockCursorWhenOpen)
        {
            DebugMenuManager.Instance.ConfigureModalBehavior(pauseGameplayWhenOpen, unlockCursorWhenOpen);
        }
    }
}
