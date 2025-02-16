namespace 亂七八糟.Shared.Interfaces
{
    public interface IProtectedStorageService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value);
        Task DeleteAsync(string key);
    }
}