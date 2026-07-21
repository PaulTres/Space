using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Utilities;
namespace Player
{
    [Serializable]
    public class GameSave
    {
        public static readonly string Name = "GameSave";
        public readonly long dateTimeBinary;
        public int CurrentMissionIndex { get; set; } = 0;

        [JsonConstructor]
        public GameSave([JsonProperty("CurrentMissionIndex")] int index)
        {
            dateTimeBinary = DateTime.Now.ToBinary();
            CurrentMissionIndex = index;
        }

        public override string ToString()
        {
            return Name + ": " + DateTime.FromBinary(dateTimeBinary).ToString();
        }

        #region Static helpers
        public static string ConvertToFileName(string fancyString)
        {
            var dateTime = DateTime.Parse(fancyString.Replace("GameSave: ", ""));
            return Name + "_" + dateTime.ToBinary();
        }

        public static string ConvertFileName(string fileName)
        {
            var binary = long.Parse(fileName.Replace("GameSave_", ""));
            return "GameSave: " + DateTime.FromBinary(binary).ToString();
        }

        public static List<string> GetSaveFileNames()
        {
            List<string> fileNames = new();
            var dir = new DirectoryInfo(GlobalConfig.SaveFileLocation);
            foreach (var file in dir.GetFiles("GameSave_*"))
            {
                fileNames.Add(ConvertFileName(file.Name));
            }
            return fileNames;
        }

        public static void DeleteSave(string fileName)
        {
            JSONService.DeleteData(fileName);
        }

        public static GameSave Load(string saveFilePath)
        {
            if (JSONService.LoadData(saveFilePath, out GameSave result))
            {
                return result;
            }
            return null;
        }

        public void Save()
        {
            JSONService.SaveData(this, Name + "_" + dateTimeBinary);
        }
        #endregion
    }
}