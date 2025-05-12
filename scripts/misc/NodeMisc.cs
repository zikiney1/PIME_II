using Godot;
using System;
public static class NodeMisc{
    
    /// <summary>
    /// Removes all children from the specified node and queues them for deletion.
    /// </summary>
    /// <param name="node">The node from which all children will be removed.</param>
    public static void RemoveAllChildren(Node node){
        foreach(Node child in node.GetChildren()){
            node.RemoveChild(child);
            child.QueueFree();
        }
    }
    /// <summary>
    /// Generate a timer as a child of the given node.
    /// If a timeout action is given, it will be added to the timer's Timeout signal.
    /// </summary>
    /// <param name="father">The parent node to which the timer will be added.</param>
    /// <param name="time">The time in seconds to wait for the timer to timeout.</param>
    /// <param name="timeout">The action to call when the timer timeouts.</param>
    public static Timer GenTimer(Node father,float time = 0.5f, Action timeout = null){
        Timer timer = new Timer();
        timer.WaitTime = time;
        timer.OneShot = true;
        if(timeout != null) timer.Timeout += timeout;
        father.AddChild(timer);
        return timer;
    }
}