using UnityEngine;

namespace RCWorld.DebugMenu
{
    // Persistent runtime coordinator for debug-menu input, navigation, UI, pause state, and cursor state.
    public sealed class DebugMenuManager : MonoBehaviour
    {
        private static DebugMenuManager instance;

        [SerializeField] private bool pauseGameplayWhenOpen = true;
        [SerializeField] private bool unlockCursorWhenOpen = true;

        private readonly DebugMenuNavigation navigation = new DebugMenuNavigation();
        private IDebugMenuInput input;
        private DebugMenuUI menuUI;
        private float timeScaleBeforeOpening;
        private CursorLockMode cursorLockModeBeforeOpening;
        private bool cursorWasVisibleBeforeOpening;

        // Gets the persistent manager, creating it when it is first needed.
        public static DebugMenuManager Instance
        {
            get
            {
                if (instance == null) CreateInstance();
                return instance;
            }
        }

        public bool IsOpen { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreatePersistentInstance()
        {
            if (instance == null) CreateInstance();
        }

        // Sets whether the manager pauses gameplay and unlocks the cursor while open.
        public void ConfigureModalBehavior(bool pauseGameplay, bool unlockCursor)
        {
            pauseGameplayWhenOpen = pauseGameplay;
            unlockCursorWhenOpen = unlockCursor;
        }

        // Replaces and disposes the active input source.
        public void SetInput(IDebugMenuInput newInput)
        {
            if (newInput == null) throw new System.ArgumentNullException(nameof(newInput));
            input?.Dispose();
            input = newInput;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            menuUI = new DebugMenuUI(transform);
            input = new InputSystemDebugMenuInput();
            DebugMenuRegistry.ContentChanged += RefreshUI;
        }

        private void Update()
        {
            if (input.OpenClosePressedThisFrame)
            {
                Toggle();
                return;
            }

            if (!IsOpen) return;

            if (input.NavigateUpPressedThisFrame) navigation.Move(-1);
            else if (input.NavigateDownPressedThisFrame) navigation.Move(1);
            else if (input.NavigateLeftPressedThisFrame) AdjustSelectedItem(-1);
            else if (input.NavigateRightPressedThisFrame) AdjustSelectedItem(1);
            else if (input.SubmitPressedThisFrame) ActivateSelection();
            else if (input.BackPressedThisFrame) GoBackOrClose();

            RefreshUI();
        }

        private void OnDestroy()
        {
            DebugMenuRegistry.ContentChanged -= RefreshUI;
            input?.Dispose();
            if (instance == this) instance = null;
        }

        private static void CreateInstance()
        {
            GameObject gameObject = new GameObject("Debug Menu Manager");
            instance = gameObject.AddComponent<DebugMenuManager>();
        }

        private void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        private void Open()
        {
            IsOpen = true;
            navigation.Reset(DebugMenuRegistry.RootCategory);
            if (pauseGameplayWhenOpen)
            {
                timeScaleBeforeOpening = Time.timeScale;
                Time.timeScale = 0f;
            }
            if (unlockCursorWhenOpen)
            {
                cursorLockModeBeforeOpening = Cursor.lockState;
                cursorWasVisibleBeforeOpening = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            menuUI.SetVisible(true);
            RefreshUI();
        }

        private void Close()
        {
            IsOpen = false;
            if (pauseGameplayWhenOpen) Time.timeScale = timeScaleBeforeOpening;
            if (unlockCursorWhenOpen)
            {
                Cursor.lockState = cursorLockModeBeforeOpening;
                Cursor.visible = cursorWasVisibleBeforeOpening;
            }
            menuUI.SetVisible(false);
        }

        private void ActivateSelection()
        {
            if (navigation.TryGetSelectedCategory(out DebugMenuCategory category))
            {
                navigation.OpenCategory(category);
                return;
            }
            if (navigation.TryGetSelectedItem(out DebugMenuItem item)) item.Activate();
        }

        private void AdjustSelectedItem(int direction)
        {
            if (navigation.TryGetSelectedItem(out DebugMenuItem item) && item.CanAdjust) item.Adjust(direction);
        }

        private void GoBackOrClose()
        {
            if (navigation.CurrentCategory.Parent == null) Close();
            else navigation.GoBack();
        }

        private void RefreshUI()
        {
            if (IsOpen) menuUI.Render(navigation.CurrentCategory, navigation.SelectedIndex);
        }
    }
}
