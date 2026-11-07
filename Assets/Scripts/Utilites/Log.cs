using UnityEngine;

public static class Log
{
    public static bool Enabled = true;

    public static void Info(string message)
    {
        if (Enabled)
            Debug.Log(message);
    }

    public static void Warn(string message)
    {
        if (Enabled)
            Debug.LogWarning(message);
    }

    public static void Error(string message)
    {
        Debug.LogError(message);
    }
}
