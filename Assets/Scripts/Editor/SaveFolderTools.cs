using System.IO;
using UnityEditor;
using UnityEngine;

public static class SaveFolderTools
{
    [MenuItem("Tools/Save/Open Save Folder", false, 100)]
    public static void OpenSaveFolder()
    {
        string path = Application.persistentDataPath;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        Debug.Log($"[Save] persistentDataPath: {path}");
        EditorUtility.RevealInFinder(path);
    }
}
