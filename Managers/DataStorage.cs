using System;
using System.Collections.Generic;
using System.IO;
using FaceIDHRM.Models;
using Newtonsoft.Json;

namespace FaceIDHRM.Managers
{
    public static class DataStorage
    {
        private static readonly string DataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        
        static DataStorage()
        {
            if (!Directory.Exists(DataFolder))
            {
                Directory.CreateDirectory(DataFolder);
            }
        }

        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto, // Cho phép deserialize đa hình (Abstract Class)
                Formatting = Formatting.Indented
            };
        }

        public static void SaveData<T>(List<T> data, string fileName)
        {
            string path = Path.Combine(DataFolder, fileName);
            string json = JsonConvert.SerializeObject(data, GetSettings());
            File.WriteAllText(path, json);
        }

        public static List<T> LoadData<T>(string fileName)
        {
            string path = Path.Combine(DataFolder, fileName);
            if (!File.Exists(path))
            {
                return new List<T>();
            }
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<T>>(json, GetSettings()) ?? new List<T>();
        }
    }
}
