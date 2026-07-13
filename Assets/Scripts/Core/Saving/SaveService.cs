using System;
using System.IO;
using UnityEngine;

namespace RCWorld.Saving
{
    /// <summary>
    /// File-based storage for serializable data transfer objects. It intentionally has no
    /// knowledge of scenes, vehicles, or other gameplay objects.
    /// </summary>
    public sealed class SaveService : MonoBehaviour, ISaveService
    {
        public static SaveService Instance { get; private set; }

        [SerializeField] private string saveFolderName = "Saves";
        public string SaveDirectory => Path.Combine(Application.persistentDataPath, saveFolderName);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Directory.CreateDirectory(SaveDirectory);
        }

        public void Save<T>(string key, T data) where T : class, ISaveData
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            string destination = GetFilePath(key);
            string temporary = destination + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
            File.Copy(temporary, destination, true);
            File.Delete(temporary);
        }

        public bool TryLoad<T>(string key, out T data) where T : class, ISaveData
        {
            string path = GetFilePath(key);
            if (!File.Exists(path))
            {
                data = null;
                return false;
            }

            try
            {
                data = JsonUtility.FromJson<T>(File.ReadAllText(path));
                return data != null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not read save '{key}': {exception.Message}", this);
                data = null;
                return false;
            }
        }

        public T LoadOrCreate<T>(string key) where T : class, ISaveData, new()
        {
            if (TryLoad<T>(key, out T data))
                return data;

            return new T();
        }

        public bool Delete(string key)
        {
            string path = GetFilePath(key);
            if (!File.Exists(path))
                return false;

            File.Delete(path);
            return true;
        }

        private string GetFilePath(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A save key is required.", nameof(key));

            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
                key = key.Replace(invalidCharacter, '_');

            return Path.Combine(SaveDirectory, key + ".json");
        }
    }
}
