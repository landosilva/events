using Lando.Events;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour,
    IEventListener<OnTestEvent2>
{
    private void Start()
    {
        new OnTestEvent("Hey there! 1").Raise();
        new OnTestEvent2("Hey there! 2").Raise();
        new OnTestEvent3("Hey there! 3").Raise();
        new OnTestEvent4("Hey there! 4").Raise();
        new OnTestEvent5("Hey there! 5").Raise();
        new OnTestEvent6("Hey there! 6").Raise();
    }

    public void OnListenedTo(OnTestEvent e)
    {
        Debug.Log(e.Message);
    }

    public void OnListenedTo(OnTestEvent2 e)
    {
        Debug.Log(e.Message);
    }

    public void OnListenedTo(OnTestEvent3 e)
    {
        Debug.Log(e.Message);
    }
    
    public void OnListenedTo(OnTestEvent4 e)
    {
        Debug.Log(e.Message);
    }
    
    public void OnListenedTo(OnTestEvent5 e)
    {
        Debug.Log(e.Message);
    }
    
    public void OnListenedTo(OnTestEvent6 e)
    {
        Debug.Log(e.Message);
    }
}

public record OnTestEvent(string Message) : IEvent
{
    public string Message { get; } = Message;
}

public record OnTestEvent2(string Message) : IEvent
{
    public string Message { get; } = Message;
}

public record OnTestEvent3(string Message) : IEvent
{
    public string Message { get; } = Message;
}

public record OnTestEvent4(string Message) : IEvent
{
    public string Message { get; } = Message;
}

public record OnTestEvent5(string Message) : IEvent
{
    public string Message { get; } = Message;
}

public record OnTestEvent6(string Message) : IEvent
{
    public string Message { get; } = Message;
}