using Godot;
using System;
public static class NodeMisc{
    public static void RemoveAllChildren(Node node){
        foreach(Node child in node.GetChildren()){
            node.RemoveChild(child);
            child.QueueFree();
        }
    }
    public static Timer GenTimer(Node father,float time = 0.5f, Action timeout = null){
        Timer timer = new Timer();
        timer.WaitTime = time;
        timer.OneShot = true;
        if(timeout != null) timer.Timeout += timeout;
        father.AddChild(timer);
        return timer;
    }
}