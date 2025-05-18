# 📣 Events

**Events** is a lightweight event bus system for Unity, designed to facilitate decoupled communication between game components. It allows game objects and systems to publish and subscribe to events without direct references.

---

## 🚀 Overview

- The core **EventBus** maintains a registry of listeners.
- Components implement `IEventListener<T>` to handle events of type `T`.
- Custom events are defined as types implementing `IEvent`.

## ⚙️ Weaver Integration

A build-time **Weaver** automatically injects registration and unregistration calls:

1. **Detection**: After Unity compiles assemblies, the Weaver (using Mono.Cecil) scans for classes implementing `IEventListener<T>`.
2. **Injection**:
   - For **MonoBehaviour** classes, it inserts `EventBus.Register<T>(this)` in `OnEnable` and `EventBus.Unregister<T>(this)` in `OnDisable`.
   - For **plain classes**, it adds registration in the constructor and unregistration in a finalizer.
3. **Automation**: No manual action required. Listeners are wired into the EventBus at runtime automatically.

## 📦 Installation

1. WIP

---

## 🛠️ Example Usage

```csharp
public record PlayerScored(int Score) : IEvent
{
    public int Score { get; } = Score;
}

public class ScoreDisplay : MonoBehaviour, IEventListener<PlayerScored>
{
    public void OnListenedTo(PlayerScored e)
    {
        Debug.Log($"Player scored {e.Score} points!");
    }
}
```

Listeners will be wired automatically when the scene runs.

---
