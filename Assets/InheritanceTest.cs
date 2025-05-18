using Lando.Events;
using UnityEngine;

public class InheritanceTest : AnotherTest,
    IEventListener<OnTestEvent>
{
    public void OnListenedTo(OnTestEvent e)
    {
        Debug.Log(e.Message);
    }
}