using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public class AuditLogsController : Controller
    {
        private readonly IAuditLogService _auditLog;
        private readonly IStringLocalizer<SharedResource> _L;

        public AuditLogsController(IAuditLogService auditLog, IStringLocalizer<SharedResource> L)
        {
            _auditLog = auditLog;
            _L = L;
        }

        public static readonly string[] KnownActions =
        {
            "Student.Verify",
            "Student.Reject",
            "Student.RequestDocuments",
            "Owner.Verify",
            "Owner.Reject",
            "Property.Approve",
            "Property.Reject",
            "Property.Delete",
            "User.Suspend",
            "User.Activate",
            "User.Delete",
            "Complaint.Respond",
            "Review.Approve",
            "Review.Remove"
        };

        public async Task<IActionResult> Index(string? q, [FromQuery(Name = "action")] string? filterAction)
        {
            ViewBag.Query = q;
            ViewBag.FilterAction = filterAction;
            ViewBag.ActionOptions = KnownActions.Select(a => new SelectListItem
            {
                Value = a,
                Text = _L[$"Audit.Action.{a}"]
            }).ToList();

            return View(await _auditLog.SearchAsync(q, filterAction));
        }
    }
}
