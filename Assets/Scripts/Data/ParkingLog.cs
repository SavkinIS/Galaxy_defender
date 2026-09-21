using UnityEngine;

public static class ParkingLog
{
    public static bool Enabled = true;

    public static void Info(string message)
    {
        if (Enabled)
            Debug.Log("[Parking] " + message);
    }

    public static void Warn(string message)
    {
        if (Enabled)
            Debug.LogWarning("[Parking] " + message);
    }

    public static void Error(string message)
    {
        Debug.LogError("[Parking] " + message);
    }
}
