using System;
using System.Collections.Generic;

public static class EventHandler{
    static Dictionary<string,Action> events = new Dictionary<string,Action>();
    public static void AddEvent(string name,Action action){
        if(events.ContainsKey(name)){
            events[name] += action;
        }else{
            events.Add(name,action);
        }
    }
    public static void EmitEvent(string name){
        if(events.ContainsKey(name)){
            if(events[name] == null) return;
            events[name].Invoke();
        }
    }
}