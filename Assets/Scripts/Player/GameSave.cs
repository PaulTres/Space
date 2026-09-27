using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utilities;
namespace Player
{
    [Serializable]
    public class GameSave
    {
        public static readonly string Name = "GameSave";
        public static readonly List<string> Saves = new List<string>();
        public readonly long dateTimeBinary;
        public int CurrentMissionIndex { get; set; } = 0;
        public readonly int Difficulty;

        public GameSave(int difficulty)
        {
            Difficulty = difficulty;
            CurrentMissionIndex = 0;
            dateTimeBinary = DateTime.Now.ToBinary();
        }

        [JsonConstructor]
        public GameSave([JsonProperty("LatestMissionIndex")] int index, [JsonProperty("Difficulty")] int difficulty, [JsonProperty("dateTimeBinary")] long dateTimeBinary)
        {
            this.dateTimeBinary = dateTimeBinary;
            CurrentMissionIndex = index;
            Difficulty = difficulty;
        }

        public void Save()
        {
            JSONService.SaveData(this, Name + "_" + dateTimeBinary + ".json");
        }

        public override string ToString()
        {
            return Name + ": " + DateTime.FromBinary(dateTimeBinary).ToString();
        }

        #region Static helpers
        public static string ConvertToFileName(string fancyString)
        {
            var dateTime = DateTime.Parse(fancyString.Replace("GameSave: ", ""));
            return Name + "_" + dateTime.ToBinary() + ".json";
        }

        public static string ConvertFileName(string fileName)
        {
            var binary = long.Parse(fileName.Replace("GameSave_", "").Replace(".json", ""));
            return "GameSave: " + DateTime.FromBinary(binary).ToString();
        }

        public static List<string> GetSaveFileNames()
        {
            Saves.Clear();
            List<string> fileNames = new();
            var dir = new DirectoryInfo(GlobalConfig.SaveFileLocation);
            foreach (var file in dir.GetFiles("GameSave_*"))
            {
                try
                {
                    var _ = GameSave.Load(file.Name);
                    Saves.Add(file.Name);
                    fileNames.Add(ConvertFileName(file.Name));
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
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
        #endregion
    }
}