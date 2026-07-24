using Newtonsoft.Json;
using System.IO;
using UnityEngine;
namespace Utilities
{
    public static class JSONService
    {
        /// <summary>
        /// Delete an object. The file should be located in the persistent data path and have the 
        /// specified file name.
        /// </summary>
        /// <param name="fileName">The name of the JSON file to delete.</param>
        /// <returns>True if the file was successfully deleted; otherwise, false.</returns>
        public static bool DeleteData(string fileName)
        {
            string path = Path.Combine(GlobalConfig.SaveFileLocation, fileName);
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }
                else
                {
                    Debug.LogWarning("File not found: " + path);
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error deleting data: " + e.Message);
                return false;
            }
        }
        /// <summary>
        /// Save an object of type T to a JSON file. The file will be located in the persistent data path
        /// and have the specified file name. If the file is successfully saved, the method will return true.
        /// If there is an error (e.g., invalid data, file write error), the method will return false.
        /// </summary>
        /// <typeparam name="T">The type of the object to save.</typeparam>
        /// <param name="data">The object to save.</param>
        /// <param name="fileName">The name of the JSON file.</param>
        /// <returns>True if the data was successfully saved; otherwise, false.</returns>
        public static bool SaveData<T>(T data, string fileName)
        {
            string path = Path.Combine(GlobalConfig.SaveFileLocation, fileName);
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(data));
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error saving data: " + e.Message);
                return false;
            }
        }
        /// <summary>
        /// LoadSave an object of type T from a JSON file. The file should be located in the persistent 
        /// data path and have the specified file name. If the file is successfully loaded, the 
        /// result parameter will contain the deserialized object and the method will return true. 
        /// If there is an error (e.g., file not found, invalid JSON), the method will return false 
        /// and the result will be set to default(T).
        /// </summary>
        /// <typeparam name="T">The type of the object to load.</typeparam>
        /// <param name="fileName">The name of the JSON file.</param>
        /// <param name="result">The variable to store the loaded object.</param>
        /// <returns>True if the data was successfully loaded; otherwise, false.</returns>
        public static bool LoadData<T>(string fileName, out T result)
        {
            string path = Path.Combine(GlobalConfig.SaveFileLocation, fileName);
            try
            {
                result = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
                return true;
            }
            catch (System.Exception e)
            {
                result = default;
                Debug.LogError("Error loading data: " + e.Message);
                return false;
            }
        }
    }
}