using System;
using System.Collections.Generic;

public class EventHandler{
    Dictionary<string,Action> events = new Dictionary<string,Action>();
    public void AddEvent(string name,Action action){
        if(events.ContainsKey(name)){
            events[name] += action;
        }else{
            events.Add(name,action);
        }
    }
}