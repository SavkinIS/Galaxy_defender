using System;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Разработческие читы: правят SoftValue прямо в файле сохранения.
/// Работают только вне Play — в Play файл перезаписывается из памяти при выходе.
/// </summary>
public static class SaveSoftCurrencyCheats
{
    [MenuItem("Tools/Save/Soft Currency/+1000", false, 200)]
    private static void Add1000() => Add(1000);

    [MenuItem("Tools/Save/Soft Currency/+10000", false, 201)]
    private static void Add10000() => Add(10000);

    [MenuItem("Tools/Save/Soft Currency/Reset to 0", false, 202)]
    private static void ResetBalance() => Apply(0, "reset");

    private static void Add(int amount)
    {
        if (!TryLoad(out SaveData data, out ISaveStorage storage))
            return;

        Apply(data, storage, data.SoftValue + amount, $"+{amount}");
    }

    private static void Apply(int value, string action)
    {
        if (!TryLoad(out SaveData data, out ISaveStorage storage))
            return;

        Apply(data, storage, value, action);
    }

    private static void Apply(SaveData data, ISaveStorage storage, int value, string action)
    {
        int before = data.SoftValue;
        int clamped = Mathf.Max(0, value);
        data.SoftValue = clamped;

        if (!storage.TryWrite(JsonConvert.SerializeObject(data)))
        {
            Debug.LogError("[SaveCheat] Failed to write save file");
            return;
        }

        string note = EditorApplication.isPlaying
            ? " — Play уже запущен, при выходе файл будет перезаписан из памяти"
            : string.Empty;

        Debug.Log($"[SaveCheat] Soft {action}: {before} -> {clamped}{note}");
    }

    private static bool TryLoad(out SaveData data, out ISaveStorage storage)
    {
        storage = new FileSaveStorage();
        data = null;

        if (!storage.TryRead(out string content) || string.IsNullOrEmpty(content))
        {
            Debug.LogWarning("[SaveCheat] Save file not found. Запусти игру один раз, чтобы он создался.");
            return false;
        }

        try
        {
            data = JsonConvert.DeserializeObject<SaveData>(content);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveCheat] Save file is broken: {e}");
            return false;
        }

        if (data == null)
        {
            Debug.LogWarning("[SaveCheat] Save file is empty");
            return false;
        }

        return true;
    }
}
