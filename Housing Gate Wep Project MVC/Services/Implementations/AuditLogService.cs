using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _http;

        public AuditLogService(IUnitOfWork uow, IHttpContextAccessor http)
        {
            _uow = uow;
            _http = http;
        }

        public async Task LogAsync(string action, string? entityType = null, string? entityId = null,
            string? description = null, string? reason = null)
        {
            var user = _http.HttpContext?.User;
            var performerId = user?.FindFirstValue(ClaimTypes.NameIdentifier);

            var entry = new AuditLog
            {
                PerformedByUserId = string.IsNullOrWhiteSpace(performerId) ? "system" : performerId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                Reason = reason,
                IpAddress = _http.HttpContext?.Connection.RemoteIpAddress?.ToString()
            };

            await _uow.AuditLogs.AddAsync(entry);
            await _uow.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<AuditLog>> SearchAsync(string? query = null, string? action = null, int take = 200)
            => await _uow.AuditLogs.SearchAsync(query, action, take);
    }
}
