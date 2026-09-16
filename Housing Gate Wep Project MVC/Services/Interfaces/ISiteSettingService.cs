namespace StudentHousing.Services.Interfaces
{
    public interface ISiteSettingService
    {
        Task<string> GetAsync(string key, string fallback);
        Task<Dictionary<string, string>> GetAllAsync();
        Task SetAsync(string key, string value);
    }
}
