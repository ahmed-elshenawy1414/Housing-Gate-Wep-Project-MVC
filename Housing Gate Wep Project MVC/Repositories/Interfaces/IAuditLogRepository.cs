using StudentHousing.Models;

namespace StudentHousing.Repositories.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        /// <summary>
        /// Search the audit trail. Every term is optional; when null it is ignored.
        /// </summary>
        Task<IReadOnlyList<AuditLog>> SearchAsync(
            string? query = null,
            string? action = null,
            int take = 200);
    }
}
