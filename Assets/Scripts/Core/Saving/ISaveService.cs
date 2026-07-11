namespace RCWorld.Saving
{
    public interface ISaveService
    {
        void Save<T>(string key, T data) where T : class, ISaveData;
        bool TryLoad<T>(string key, out T data) where T : class, ISaveData;
        T LoadOrCreate<T>(string key) where T : class, ISaveData, new();
        bool Delete(string key);
    }
}
