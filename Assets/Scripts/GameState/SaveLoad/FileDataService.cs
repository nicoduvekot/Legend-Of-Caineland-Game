using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GameState.SaveLoad
{
    public class FileDataService : IDataService
    {
        private readonly ISerializer _serializer;
        private readonly string _dataPath;
        private readonly string _savesPath;
        private readonly string _fileExtension;
        
        public FileDataService(ISerializer serializer)
        {
            _serializer = serializer;
            _dataPath = Application.persistentDataPath;
            _savesPath = Path.Combine(_dataPath, "Saves");
            _fileExtension = ".json";

            EnsureSavesFolderExists();
        }

        private void EnsureSavesFolderExists()
        {
            if (!Directory.Exists(_savesPath))
                Directory.CreateDirectory(_savesPath);
        }

        private string GetPathToFile(string filename)
        {
            return Path.Combine(_savesPath, string.Concat(filename, _fileExtension));
        }

        public void Save(GameDataDTO data, bool overwrite = true)
        {
            string fileLocation = GetPathToFile(data.saveName);

            if (!overwrite && File.Exists(fileLocation))
                throw new IOException($"The File '{data.saveName}.{_fileExtension}' already exists and cannot be overwritten.");
            
            File.WriteAllText(fileLocation, _serializer.Serialize(data));
        }

        public GameDataDTO Load(string name)
        {
            string fileLocation = GetPathToFile(name);
            
            if (!File.Exists(fileLocation))
                throw new ArgumentException($"No GameData with the name '{name}' was found.");
            
            return _serializer.Deserialize<GameDataDTO>(File.ReadAllText(fileLocation));
        }

        public void Delete(string name)
        {
            string fileLocation = GetPathToFile(name);
            
            if (File.Exists(fileLocation))
                File.Delete(fileLocation);
        }

        public void DeleteAll()
        {
            // CAREFUL!! THIS DELETES EVERYTHING AT THIS LOCATION WITHOUT CHECKS!!
            foreach (string file in Directory.GetFiles(_savesPath))
                Delete(file);
        }

        public IEnumerable<string> ListSaves()
        {
            return from path in Directory.EnumerateFiles(_savesPath)
                where Path.GetExtension(path) == _fileExtension
                select Path.GetFileNameWithoutExtension(path);
        }
    }
}