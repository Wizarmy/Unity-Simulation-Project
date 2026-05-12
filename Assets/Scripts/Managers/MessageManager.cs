using System;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    public static MessageManager Instance { get; private set; }

    /// <summary>
    /// Subscribe to this to receive all messages (great for UI, console, etc.)
    /// </summary>
    public event Action<GameMessage> OnMessageReceived;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Capture ALL Debug.Log / LogWarning / LogError
        Application.logMessageReceived += HandleUnityLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        MessageType msgType = type switch
        {
            LogType.Error or LogType.Exception => MessageType.Error,
            LogType.Warning => MessageType.Warning,
            _ => MessageType.Debug
        };

        var message = new GameMessage(msgType, logString, "Unity");
        OnMessageReceived?.Invoke(message);
    }

    #region Public Logging Methods

    public void Log(string text, string source = "")
    {
        var msg = new GameMessage(MessageType.Debug, text, source);
        OnMessageReceived?.Invoke(msg);
        Debug.Log(text);           // Still go to Unity console
    }

    public void LogWarning(string text, string source = "")
    {
        var msg = new GameMessage(MessageType.Warning, text, source);
        OnMessageReceived?.Invoke(msg);
        Debug.LogWarning(text);
    }

    public void LogError(string text, string source = "")
    {
        var msg = new GameMessage(MessageType.Error, text, source);
        OnMessageReceived?.Invoke(msg);
        Debug.LogError(text);
    }

    public void LogSystem(string text, string source = "")
    {
        var msg = new GameMessage(MessageType.System, text, source);
        OnMessageReceived?.Invoke(msg);
        Debug.Log($"<color=lightblue>[SYSTEM]</color> {text}");
    }

    // Example future method
    public void LogCombat(string text, string source = "")
    {
        var msg = new GameMessage(MessageType.Combat, text, source);
        OnMessageReceived?.Invoke(msg);
    }

    #endregion
}