using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus
{
    private static EventBus instance;
    public static EventBus Instance
    {
        get
        {
            if (instance == null)
                instance = new EventBus();
            return instance;
        }
    }

    private Dictionary<Type, List<Delegate>> eventHandlers = new Dictionary<Type, List<Delegate>>();

    public void Subscribe<T>(Action<T> handler) where T : class
    {
        Type eventType = typeof(T);

        if (!eventHandlers.ContainsKey(eventType))
            eventHandlers[eventType] = new List<Delegate>();

        if (!eventHandlers[eventType].Contains(handler))
            eventHandlers[eventType].Add(handler);
    }

    public void Unsubscribe<T>(Action<T> handler) where T : class
    {
        Type eventType = typeof(T);

        if (eventHandlers.ContainsKey(eventType))
        {
            eventHandlers[eventType].Remove(handler);

            if (eventHandlers[eventType].Count == 0)
                eventHandlers.Remove(eventType);
        }
    }

    public void Publish<T>(T eventData) where T : class
    {
        Type eventType = typeof(T);

        if (eventHandlers.ContainsKey(eventType))
        {

            List<Delegate> handlers = new List<Delegate>(eventHandlers[eventType]);

            foreach (Delegate handler in handlers)
            {
                try
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[EventBus] Erreur lors de l'exécution d'un handler pour {eventType.Name}: {ex.Message}");
                }
            }
        }
    }

    public void Clear()
    {
        eventHandlers.Clear();
    }

    public int GetSubscriberCount<T>() where T : class
    {
        Type eventType = typeof(T);
        return eventHandlers.ContainsKey(eventType) ? eventHandlers[eventType].Count : 0;
    }
}
