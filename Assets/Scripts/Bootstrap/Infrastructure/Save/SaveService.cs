public class SaveService  : ISaveService
{
   private SaveData _saveData;

   public void Save()
   {
      string json = Newtonsoft.Json.JsonConvert.SerializeObject(_saveData);
   }

   public SaveData GetSaveData()
   {
      return _saveData;
   }

   public void Load()
   {
      string json = Newtonsoft.Json.JsonConvert.SerializeObject(_saveData);
      
      if (string.IsNullOrEmpty(json))
         _saveData = new SaveData();
   }
}