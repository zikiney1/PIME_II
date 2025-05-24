using Godot;
using System;
using System.Collections.Generic;

public static class QuestHandler
{
    public static Dictionary<string, Quest> quests = new();
    public static List<Quest> ActiveQuests = new();
    static bool isSetup = false;
    public static void Setup()
    {
        if (isSetup) return;
        
        isSetup = true;
    }

    public static void AddQuest(string name) => ActiveQuests.Add(quests[name]);
    public static void SetActiveQuests(string[] questsRaw)
    {
        foreach (string q in questsRaw)
        {
            if (quests.ContainsKey(q))
            {
                ActiveQuests.Add(quests[q]);
            }
        }
    }
}


public class Quest
{
    public string name;
    public string description;
    public bool completed;
    public bool active;
    public Action onCompleted;
    public Quest(string name, string description, bool completed, bool active)
    {
        this.name = name;
        this.description = description;
        this.completed = completed;
        this.active = active;
    }
    public bool isCompleted() => completed && !active;

    public void Complete()
    {
        active = false;
        completed = true;
        onCompleted?.Invoke();
    }
}