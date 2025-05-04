# 📣 Events

**Events** is a lightweight event bus system for Unity, designed to facilitate decoupled communication between game components. It allows for clean and maintainable code by enabling components to publish and subscribe to events without direct references.

---

## ✨ Features

- 🧩 Decoupled architecture for better modularity
- 🔄 Supports custom event types
- 🛠️ Easy integration into existing projects
- 📦 Minimalistic and efficient design

---

## 🚀 Example Usage

```csharp
// Define a custom event
public class PlayerScoredEvent
{
    public int Score { get; set; }
}

// Subscribe to the event
EventBus.Subscribe<PlayerScoredEvent>(OnPlayerScored);

// Publish the event
EventBus.Publish(new PlayerScoredEvent { Score = 10 });

// Event handler
void OnPlayerScored(PlayerScoredEvent e)
{
    Debug.Log($"Player scored: {e.Score}");
}
```

---

## 📦 Installation

1. Clone or download the repository.
2. Copy the contents of `Assets/_PackageRoot` into your Unity project's `Assets` folder.

---
