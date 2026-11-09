using System;
using System.IO;
using UnityEngine;

public class FileSaveStorage : ISaveStorage
{
    private const string FileName = "save.json";

    private readonly string _path;
    
    public FileSaveStorage()
    {
        _path = System.IO.Path.Combine(Application.persistentDataPath, FileName);
    }
    
    public bool Exists() =>
        File.Exists(_path);

    public bool TryRead(out string content)
    {
        content = null;
        
        try
        {
            if (Exists() == false)
                return false;
            
            content = File.ReadAllText(_path);
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"[{this.GetType().Name}] Read failed: {_path} \n{e}");
            return false;
        }
    }

    public bool TryWrite(string content)
    {
        content ??= string.Empty;
        
        try
        {
            var directory = System.IO.Path.GetDirectoryName(_path);

            if (string.IsNullOrEmpty(directory) == false &&
                Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(_path,content);
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"[{this.GetType().Name}] Write failed: {_path} \n{e}");
            return false;
        }
    }

    public bool TryDelete()
    {
        try
        {
            if (Exists() == false)
                return false;

            File.Delete(_path);
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"[{this.GetType().Name}] Delete failed: {_path} \n{e}]");
            return false;
        }
    }
}