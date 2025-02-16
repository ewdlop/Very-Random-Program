using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using 亂七八糟.Shared.Interfaces;

namespace 亂七八糟.Fluxor.Razor.Services;

public class ProtectedStorageService : IProtectedStorageService
{
    private readonly ProtectedLocalStorage _protectedLocalStorage;

    public ProtectedStorageService(ProtectedLocalStorage protectedLocalStorage)
    {
        _protectedLocalStorage = protectedLocalStorage;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<T>(key);
            return result.Success ? result.Value : default;
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        await _protectedLocalStorage.SetAsync(key, value);
    }

    public async Task DeleteAsync(string key)
    {
        await _protectedLocalStorage.DeleteAsync(key);
    }
}