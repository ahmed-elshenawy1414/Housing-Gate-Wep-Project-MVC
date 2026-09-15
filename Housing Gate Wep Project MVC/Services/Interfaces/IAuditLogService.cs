using StudentHousing.Models;

namespace StudentHousing.Services.Interfaces
{
    public interface IAuditLogService
    {
        /// <summary>
        /// Appends an entry to the audit trail using the current request's user and IP.
        /// </summary>
        Task LogAsync(
            string action,
            string? entityType = null,
            string? entityId = null,
            string? description = null,
            string? reason = null);

        Task<IReadOnlyList<AuditLog>> SearchAsync(string? query = null, string? action = null, int take = 200);
    }
}
