# Runtime Debug Menu

This is a game-agnostic, runtime-safe debug-menu framework. It creates its persistent manager before the first scene loads and has no dependency on project gameplay code.

## Registering content

Register from the system that owns the value or command, typically during its initialization. Categories use slash-separated paths and are created automatically.

```csharp
DebugMenu.RegisterAction("Tools", "Refill Resources", RefillResources);

DebugMenu.RegisterToggle("Tools", "Invulnerable", () => isInvulnerable, value => isInvulnerable = value);

DebugMenu.RegisterSlider("Physics", "Gravity", () => Physics.gravity.y, SetGravity, -30f, 0f, 0.5f);

DebugMenu.RegisterInteger("Tools", "Lap", () => currentLap, SetLap, 1, 99);

DebugMenu.RegisterEnum("Graphics", "Quality", GetQuality, SetQuality);

DebugMenu.RegisterScene("Scenes", "Test Track", "TestTrack");
```

Do not register the same item each time a scene object is enabled. Register once from an appropriate owning service, or keep the returned item and add an unregister feature if a future system needs transient content.

## Input

`IDebugMenuInput` is the only input contract used by the manager. `InputSystemDebugMenuInput` is the development implementation and owns the F1, keyboard, and gamepad bindings. To use a hidden release activation method, implement `IDebugMenuInput` and install it:

```csharp
DebugMenu.SetInput(new SecretDebugMenuInput());
```

The replacement can read Input System actions, a button-combination detector, a cheat-code service, or any other source. It does not require changes to navigation, UI, registration, or item types.

## Extending item types

Derive from `DebugMenuItem`, override `ValueText`, `Activate`, and/or `Adjust`, then register it through `DebugMenuRegistry.Register(item, categoryPath)`. The UI renders every item through the base class, so no UI changes are required for ordinary new item types.

## Current controls

The default development input uses F1 or gamepad Start to open/close; arrows/WASD or D-pad/left stick to navigate; Enter/Space or gamepad South to activate; and Escape/Backspace or gamepad East to go back.
