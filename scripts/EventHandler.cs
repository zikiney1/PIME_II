using System;
using System.Collections.Generic;
using System.Linq;

public static class EventHandler
{
    static Dictionary<string, Event> events = new();
    static bool isSetup = false;
    public static void Setup()
    {
        if (isSetup) return;
        isSetup = true;
        Events.Setup();
    }
    public static Event Get(string name)
    {
        if (events.ContainsKey(name))
        {
            return events[name];
        }
        return null;
    }
    public static void AddEvent(string name,bool isReward, Action action)
    {
        if (events.ContainsKey(name))
        {
            events[name].action += action;
        }
        else
        {
            Event ev = new();
            ev.name = name;
            ev.action = action;
            ev.isReward = isReward;
            events.Add(name, ev);
        }
    }
    public static void EmitEvent(string name)
    {
        if (events.ContainsKey(name))
        {
            if (events[name] == null) return;
            if (events[name].wasActivated) return;
            events[name].action.Invoke();
        }
    }

    public static void TurnOffEvents(string[] eventsToTurnOff)
    {
        foreach (string e in eventsToTurnOff) {
            if (events.ContainsKey(e))
            {
                events[e].wasActivated = true;
            }
            
        }
    }
}

public class Event
{
    public string name;
    public Action action;
    public bool wasActivated = false;
    public bool isReward = false;
}