# 📣 Event Weaver

**Event Weaver** is a lightweight event bus system for Unity, designed to facilitate decoupled communication between game components. By simply inheriting an interface, you can automatically listen to events without manual registration—since it’s injected at build time by the Weaver.

---

## 🚀 Overview

- The core **EventRegistry** maintains a registry of listeners.
- Components implement `IEventListener<T>` to handle events of type `T`.
- Custom events are defined as `record` types implementing `IEvent`.

---

## ⚙️ Weaver Integration

A build-time **Weaver** automatically injects registration and unregistration calls:

1. **Detection**: After Unity compiles assemblies, the Weaver (using Mono.Cecil) scans for types implementing `IEventListener<T>`.
2. **Injection**:
   - For **MonoBehaviour** types, it inserts `EventRegistry.Register<T>(this)` in `OnEnable` and `EventRegistry.Unregister<T>(this)` in `OnDisable`.
   - For **plain types**, it adds registration in the constructor and unregistration in a finalizer.
3. **Automation**: No manual action required—listeners are wired into the EventRegistry at runtime automatically.

---

## 🖼️ Screenshots

> **Event History**  
> *Placeholder for Event History window screenshot*

> **Event Viewer**  
> *Placeholder for Event Viewer window screenshot*

---

## 📦 Installation

Install via Unity’s Package Manager using the Git URL:

1. Open **Window > Package Manager**.
2. Click the **+** button and choose **Add package from Git URL...**
3. Enter the Git URL:
   ```
   https://github.com/landosilva/event-weaver.git?path=/Assets/Root
   ```
4. Click **Add**.

---

## 🛠️ Example Usage

```csharp
public record PlayerScored(int Score) : IEvent
{
    public int Score { get; } = Score;
}

public record EnemyDefeated(string EnemyName) : IEvent
{
    public string EnemyName { get; } = EnemyName;
}

public class ScoreDisplay : MonoBehaviour, IEventListener<PlayerScored>
{
    public void OnListenedTo(PlayerScored e)
    {
        Debug.Log($"Player scored {e.Score} points!");
    }
}

public class EnemyTracker : MonoBehaviour, IEventListener<EnemyDefeated>
{
    public void OnListenedTo(EnemyDefeated e)
    {
        Debug.Log($"Enemy defeated: {e.EnemyName}");
    }
}
```

Now, anywhere in your code you can publish events without worrying about wiring:

```csharp
EventRegistry.Publish(new PlayerScored(10));
EventRegistry.Publish(new EnemyDefeated("Goblin"));
```

Listeners will automatically register and unregister at runtime, making event-driven communication effortless.

---

*Thank you very much—enjoy using Event Weaver!*
