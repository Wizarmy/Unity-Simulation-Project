using System;
using UnityEngine;

[Serializable]
public class GameMessage
{
    public MessageType Type { get; private set; }
    public string Text { get; private set; }
    public float Timestamp { get; private set; }
    public string Source { get; private set; }     // e.g. "BaseEntity", "Player", etc.

    public GameMessage(MessageType type, string text, string source = "")
    {
        Type = type;
        Text = text;
        Timestamp = Time.time;
        Source = source;
    }

    public override string ToString()
    {
        string prefix = Type switch
        {
            MessageType.Warning => "<color=yellow>[WARNING]</color>",
            MessageType.Error   => "<color=red>[ERROR]</color>",
            MessageType.LevelUp => "<color=cyan>[LEVEL UP]</color>",
            _ => $"[{Type}]"
        };

        return $"{prefix} {Text}";
    }
}