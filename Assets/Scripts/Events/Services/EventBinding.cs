using System;
using System.Collections.Generic;
using System.Linq;

public delegate void EventCallback<T>(T raisedEvent) where T : struct, IEvent;

public class EventBinding<T> where T : struct, IEvent
{
    private List<EventHandler> Handlers { get; set; } = new List<EventHandler>();

    public void Add(EventCallback<T> callback, int priority)
    {
        EventHandler handler = new EventHandler(callback, null, priority);
        Handlers.Add(handler);
        Handlers = Handlers.OrderByDescending(h => h.Priority).ToList();
    }
    
    public void Add(Action callback, int priority)
    {
        EventHandler handler = new EventHandler(null, callback, priority);
        Handlers.Add(handler);
        Handlers = Handlers.OrderByDescending(h => h.Priority).ToList();
    }

    public void Remove(EventCallback<T> callback)
    {
        Handlers.RemoveAll(h => h.CallbackWithArg != null && h.CallbackWithArg.Equals(callback));
    }
    
    public void Remove(Action callback)
    {
        Handlers.RemoveAll(h => h.CallbackNoArgs != null && h.CallbackNoArgs.Equals(callback));
    }

    public void Raise(T ev)
    {
        foreach (EventHandler handler in Handlers)
        {
            if (handler.CallbackWithArg != null)
            {
                EventCallback<T> callback = handler.CallbackWithArg as EventCallback<T>;
                callback?.Invoke(ev);
            }
            if (handler.CallbackNoArgs != null)
            {
                Action callback = handler.CallbackNoArgs as Action;
                callback?.Invoke();
            }
        }
    }
}
