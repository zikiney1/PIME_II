using System;
using Godot;
public static class NodeFac{
    public static Timer GenTimer(Node father,float time = 0.5f, Action timeout = null){
        Timer timer = new Timer();
        timer.WaitTime = time;
        timer.OneShot = true;
        if(timeout != null) timer.Timeout += timeout;
        father.AddChild(timer);
        return timer;
    }
}