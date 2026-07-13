using System;

namespace RCWorld.DebugMenu
{
    /// <summary>
    /// Public entry point for registering runtime debug menu content.
    /// </summary>
    public static class DebugMenu
    {
        /// <summary>
        /// Registers a command that is invoked when selected.
        /// </summary>
        public static DebugItemAction RegisterAction(string categoryPath, string name, Action action)
        {
            return DebugMenuRegistry.Register(new DebugItemAction(name, action), categoryPath);
        }

        /// <summary>
        /// Registers a Boolean value that can be switched on and off.
        /// </summary>
        public static DebugItemToggle RegisterToggle(string categoryPath, string name, Func<bool> getValue, Action<bool> setValue)
        {
            return DebugMenuRegistry.Register(new DebugItemToggle(name, getValue, setValue), categoryPath);
        }

        /// <summary>
        /// Registers a floating point slider.
        /// </summary>
        public static DebugItemFloatSlider RegisterSlider(string categoryPath, string name, Func<float> getValue, Action<float> setValue, float minimum, float maximum, float step = 0.1f)
        {
            return DebugMenuRegistry.Register(new DebugItemFloatSlider(name, getValue, setValue, minimum, maximum, step), categoryPath);
        }

        /// <summary>
        /// Registers an integer value that is adjusted in fixed increments.
        /// </summary>
        public static DebugItemInteger RegisterInteger(string categoryPath, string name, Func<int> getValue, Action<int> setValue, int minimum, int maximum, int step = 1)
        {
            return DebugMenuRegistry.Register(new DebugItemInteger(name, getValue, setValue, minimum, maximum, step), categoryPath);
        }

        /// <summary>
        /// Registers an enum selector.
        /// </summary>
        public static DebugItemEnum<TEnum> RegisterEnum<TEnum>(string categoryPath, string name, Func<TEnum> getValue, Action<TEnum> setValue) where TEnum : struct, Enum
        {
            return DebugMenuRegistry.Register(new DebugItemEnum<TEnum>(name, getValue, setValue), categoryPath);
        }

        /// <summary>
        /// Registers a scene load command using a scene name included in the build settings.
        /// </summary>
        public static DebugItemScene RegisterScene(string categoryPath, string name, string sceneName)
        {
            return DebugMenuRegistry.Register(new DebugItemScene(name, sceneName), categoryPath);
        }

        /// <summary>
        /// Replaces the source used to open and navigate the menu. The menu takes ownership of the input instance.
        /// </summary>
        public static void SetInput(IDebugMenuInput input)
        {
            DebugMenuManager.Instance.SetInput(input);
        }

        /// <summary>
        /// Configures menu behavior without coupling the framework to a particular game pause or cursor system.
        /// </summary>
        public static void ConfigureModalBehavior(bool pauseGameplayWhenOpen, bool unlockCursorWhenOpen)
        {
            DebugMenuManager.Instance.ConfigureModalBehavior(pauseGameplayWhenOpen, unlockCursorWhenOpen);
        }
    }
}
