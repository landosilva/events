using System;
using Lando.Events;
using UnityEngine;

public class AnotherTest : MonoBehaviour,
    IEventListener<AnyKeyPressed>
{
    private void Update()
    {
        if (!Input.anyKeyDown) 
            return;
        
        KeyCode keyCode = Input.inputString.Length > 0 
            ? (KeyCode)Enum.Parse(typeof(KeyCode), Input.inputString) 
            : KeyCode.None;
        new AnyKeyPressed(keyCode).Raise();
    }

    public void OnListenedTo(AnyKeyPressed e)
    {
        Debug.Log($"Key Pressed: {e.KeyCode}");
    }
}

public record AnyKeyPressed(KeyCode KeyCode) : IEvent
{
    public KeyCode KeyCode { get; } = KeyCode;
}