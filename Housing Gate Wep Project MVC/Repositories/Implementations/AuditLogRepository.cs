using Microsoft.EntityFrameworkCore;
using StudentHousing.Data;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;

namespace StudentHousing.Repositories.Implementations
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<IReadOnlyList<AuditLog>> SearchAsync(string? query = null, string? action = null, int take = 200)
        {
            var q = _db.AuditLogs.Include(a => a.PerformedBy).AsQueryable();

            if (!string.IsNullOrWhiteSpace(action))
            {
                q = q.Where(a => a.Action == action);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = query.Trim();
                q = q.Where(a =>
                    a.Action.Contains(term) ||
                    (a.EntityType != null && a.EntityType.Contains(term)) ||
                    (a.Description != null && a.Description.Contains(term)) ||
                    (a.Reason != null && a.Reason.Contains(term)) ||
                    (a.PerformedBy.Email != null && a.PerformedBy.Email.Contains(term)) ||
                    (a.PerformedBy.FirstName + " " + a.PerformedBy.LastName).Contains(term));
            }

            return await q
                .OrderByDescending(a => a.CreatedAt)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
