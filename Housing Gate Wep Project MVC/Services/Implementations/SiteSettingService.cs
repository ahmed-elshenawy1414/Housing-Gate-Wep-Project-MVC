using Microsoft.EntityFrameworkCore;
using StudentHousing.Data;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Services.Implementations
{
    public class SiteSettingService : ISiteSettingService
    {
        private readonly ApplicationDbContext _db;
        public SiteSettingService(ApplicationDbContext db) => _db = db;

        public async Task<string> GetAsync(string key, string fallback)
        {
            var s = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key);
            return s?.Value ?? fallback;
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            return await _db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        }

        public async Task SetAsync(string key, string value)
        {
            var s = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (s == null)
            {
                s = new Models.SiteSetting { Key = key, Value = value };
                _db.SiteSettings.Add(s);
            }
            else
            {
                s.Value = value;
                s.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
        }
    }
}
