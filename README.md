# 📣 Event Weaver

## 📝 Summary

Event Weaver is a Unity event bus system that simplifies event-driven architecture by automatically injecting listener registration and unregistration at build time. Navigate the sections below to learn more:

- 🔹 [Overview](#-overview)
- 🔹 [Weaver Integration](#-weaver-integration)
- 🔹 [Screenshots](#-screenshots)
- 🔹 [Installation](#-installation)
- 🔹 [Example Usage](#-example-usage)

---

## 🚀 Overview

- **EventRegistry**: Central registry tracking active event listeners.
- **IEventListener<T>**: Implement this interface in your types to handle events of type `T`.
- **IEvent**: Marker interface for custom event payloads, defined as `record` types.

---

## ⚙️ Weaver Integration

The build-time Weaver (via Mono.Cecil) handles all listener wiring:

1. **Detection**  
   Scans compiled assemblies for types implementing `IEventListener<T>`.
2. **Injection for MonoBehaviours**  
   - 🔹 Inserts `EventRegistry.Register<T>(this)` in `OnEnable`.  
   - 🔹 Inserts `EventRegistry.Unregister<T>(this)` in `OnDisable`.
3. **Injection for Plain Types**  
   - 🔹 Adds registration call in the constructor.  
   - 🔹 Adds unregistration call in the finalizer.

---

## 🖼️ Screenshots

> **Event History**  
> _Placeholder for Event History window screenshot_

> **Event Viewer**  
> _Placeholder for Event Viewer window screenshot_

---

## 📦 Installation

Install via Unity Package Manager using Git URL:

1. Open **Window > Package Manager**.  
2. Click **+** and select **Add package from Git URL...**  
3. Paste:  
   ```
   https://github.com/landosilva/event-weaver.git?path=/Assets/Root
   ```  
4. Click **Add**.

---

## 🛠️ Example Usage

```csharp
public record PlayerScored(int Score) : IEvent;

public class ScoreDisplay : MonoBehaviour, IEventListener<PlayerScored>
{
    public void OnListenedTo(PlayerScored e)
    {
        Debug.Log($"Player scored {e.Score} points!");
    }
}

// Raising events:
new PlayerScored(10).Raise();
or
PlayerScored playerScored = new (10);
EventRegistry.Raise(playerScored);
```

Listeners are wired automatically—no manual registration calls needed.

---

*❤️ Thank you for using Event Weaver!*
