using System;

public class SaveService  : ISaveService
{
   private SaveData _saveData;
   private readonly ISaveStorage _fileSaveStorage;

   public SaveService(ISaveStorage  fileSaveStorage)
   {
      _fileSaveStorage = fileSaveStorage;
   }
   
   public SaveData GetSaveData()
   {
      return _saveData;
   }

   public void Load()
   {
      if (!_fileSaveStorage.TryRead(out var content) || string.IsNullOrEmpty(content))
      {
         CreateNew();
         return;
      }
      try
      {
         SaveData data = Newtonsoft.Json.JsonConvert.DeserializeObject<SaveData>(content);
         
         if (data == null)
         {
            CreateNew();
            return;
         }
         
         Migrate(data);
         _saveData = data;
      }
      catch (Exception e)
      {
         Log.Error($"[SaveService] Save file is broken, creating new: {e}");
         CreateNew();
      }
   }
   private void Migrate(SaveData data)
   {
      if (data.Version < SaveData.CurrentVersion)
         data.Version = SaveData.CurrentVersion;
   }
   
   public void Save()
   {
      if (_saveData == null)
      {
         Log.Error("[SaveService] Save skipped: save data is not loaded");
         return;
      }

      string json = Newtonsoft.Json.JsonConvert.SerializeObject(_saveData);
      if (!_fileSaveStorage.TryWrite(json))
         Log.Error("[SaveService] Failed to write save file");
   }

   private void CreateNew()
   {
      _saveData = new SaveData();
      _saveData.FirstLaunchUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
      Save();
   }
}