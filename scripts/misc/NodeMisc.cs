using Godot;
using System;
public static class NodeMisc
{

    /// <summary>
    /// Removes all children from the specified node and queues them for deletion.
    /// </summary>
    /// <param name="node">The node from which all children will be removed.</param>
    public static void RemoveAllChildren(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
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
    public static Timer GenTimer(Node father, float time = 0.5f, Action timeout = null)
    {
        Timer timer = new Timer();
        timer.WaitTime = time;
        timer.OneShot = true;
        if (timeout != null) timer.Timeout += timeout;
        father.AddChild(timer);
        return timer;
    }

    /// <summary>
    /// Generates a <see cref="VisibleOnScreenNotifier2D"/> as a child of the given <see cref="Node2D"/>.
    /// When the node is visible on screen, the <see cref="Node2D"/> will be visible and processing will be enabled.
    /// When the node is not visible on screen, the <see cref="Node2D"/> will be invisible and processing will be disabled.
    /// </summary>
    /// <param name="node">The <see cref="Node2D"/> to which the <see cref="VisibleOnScreenNotifier2D"/> will be added.</param>
    /// <returns>The generated <see cref="VisibleOnScreenNotifier2D"/>.</returns>
    public static VisibleOnScreenNotifier2D GenVisibleNotifier(Node2D node)
    {
        VisibleOnScreenNotifier2D notifier = new();
        notifier.ScreenExited += () =>
        {
            node.SetProcess(false);
            node.SetPhysicsProcess(false);
        };
        notifier.ScreenEntered += () =>
        {
            node.SetProcess(true);
            node.SetPhysicsProcess(true);
        };
        
        node.AddChild(notifier);
        return notifier;
    }
}