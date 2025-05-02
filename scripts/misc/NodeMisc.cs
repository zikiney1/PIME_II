using Godot;
public static class NodeMisc{
    public static void RemoveAllChildren(Node node){
        foreach(Node child in node.GetChildren()){
            node.RemoveChild(child);
            child.QueueFree();
        }
    }
}