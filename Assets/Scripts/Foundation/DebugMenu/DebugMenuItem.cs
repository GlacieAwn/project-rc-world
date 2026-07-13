using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RCWorld.DebugMenu
{
    // Base type for a selectable debug menu item. Add a new item type by deriving from this class.
    public abstract class DebugMenuItem
    {
        protected DebugMenuItem(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A debug menu item needs a name.", nameof(name));

            Name = name;
        }

        public string Name { get; }
        public virtual string ValueText => string.Empty;
        public virtual bool CanAdjust => false;
        public virtual void Activate() { }
        public virtual void Adjust(int direction) { }
    }

    public sealed class DebugItemAction : DebugMenuItem
    {
        private readonly Action action;
        public DebugItemAction(string name, Action action) : base(name) { this.action = action ?? throw new ArgumentNullException(nameof(action)); }
        public override void Activate() { action.Invoke(); }
    }

    public sealed class DebugItemToggle : DebugMenuItem
    {
        private readonly Func<bool> getValue;
        private readonly Action<bool> setValue;
        public DebugItemToggle(string name, Func<bool> getValue, Action<bool> setValue) : base(name) { this.getValue = getValue ?? throw new ArgumentNullException(nameof(getValue)); this.setValue = setValue ?? throw new ArgumentNullException(nameof(setValue)); }
        public override string ValueText
        {
            get
            {
                if (getValue.Invoke()) return "On";
                return "Off";
            }
        }
        public override void Activate() { setValue.Invoke(!getValue.Invoke()); }
        public override void Adjust(int direction) { if (direction != 0) Activate(); }
    }

    public sealed class DebugItemFloatSlider : DebugMenuItem
    {
        private readonly Func<float> getValue;
        private readonly Action<float> setValue;
        private readonly float minimum;
        private readonly float maximum;
        private readonly float step;
        public DebugItemFloatSlider(string name, Func<float> getValue, Action<float> setValue, float minimum, float maximum, float step) : base(name)
        {
            if (maximum < minimum)
                throw new ArgumentException("Maximum must be greater than or equal to minimum.");
            if (step <= 0f)
                throw new ArgumentOutOfRangeException(nameof(step));
            this.getValue = getValue ?? throw new ArgumentNullException(nameof(getValue)); this.setValue = setValue ?? throw new ArgumentNullException(nameof(setValue)); this.minimum = minimum; this.maximum = maximum; this.step = step;
        }
        public override bool CanAdjust => true;
        public override string ValueText => getValue.Invoke().ToString("0.##");
        public override void Adjust(int direction) { setValue.Invoke(Mathf.Clamp(getValue.Invoke() + (step * direction), minimum, maximum)); }
    }

    public sealed class DebugItemInteger : DebugMenuItem
    {
        private readonly Func<int> getValue;
        private readonly Action<int> setValue;
        private readonly int minimum;
        private readonly int maximum;
        private readonly int step;
        public DebugItemInteger(string name, Func<int> getValue, Action<int> setValue, int minimum, int maximum, int step) : base(name)
        {
            if (maximum < minimum)
                throw new ArgumentException("Maximum must be greater than or equal to minimum.");
            if (step <= 0)
                throw new ArgumentOutOfRangeException(nameof(step));
            this.getValue = getValue ?? throw new ArgumentNullException(nameof(getValue)); this.setValue = setValue ?? throw new ArgumentNullException(nameof(setValue)); this.minimum = minimum; this.maximum = maximum; this.step = step;
        }
        public override bool CanAdjust => true;
        public override string ValueText => getValue.Invoke().ToString();
        public override void Adjust(int direction) { setValue.Invoke(Mathf.Clamp(getValue.Invoke() + (step * direction), minimum, maximum)); }
    }

    public sealed class DebugItemEnum<TEnum> : DebugMenuItem where TEnum : struct, Enum
    {
        private readonly Func<TEnum> getValue;
        private readonly Action<TEnum> setValue;
        private readonly TEnum[] values = (TEnum[])Enum.GetValues(typeof(TEnum));
        public DebugItemEnum(string name, Func<TEnum> getValue, Action<TEnum> setValue) : base(name) { this.getValue = getValue ?? throw new ArgumentNullException(nameof(getValue)); this.setValue = setValue ?? throw new ArgumentNullException(nameof(setValue)); }
        public override bool CanAdjust => true;
        public override string ValueText => getValue.Invoke().ToString();
        public override void Adjust(int direction)
        {
            int currentIndex = Array.IndexOf(values, getValue.Invoke());
            int nextIndex = (currentIndex + direction) % values.Length;
            if (nextIndex < 0) nextIndex += values.Length;
            setValue.Invoke(values[nextIndex]);
        }
    }

    public sealed class DebugItemScene : DebugMenuItem
    {
        private readonly string sceneName;
        public DebugItemScene(string name, string sceneName) : base(name) { this.sceneName = string.IsNullOrWhiteSpace(sceneName) ? throw new ArgumentException("A scene name is required.", nameof(sceneName)) : sceneName; }
        public override string ValueText => sceneName;
        public override void Activate() { SceneManager.LoadScene(sceneName); }
    }
}
