using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly IDocumentStorageService _docs;
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentsController(IDocumentStorageService docs, IUnitOfWork uow, UserManager<ApplicationUser> userManager)
        {
            _docs = docs;
            _uow = uow;
            _userManager = userManager;
        }

        /// <summary>
        /// Securely serves private verification documents. Path is the stored private relative path (e.g. private/verify/{userId}/guid.jpg)
        /// For legacy /uploads/ paths it also authorizes via DB lookup.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Download(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return NotFound();

            // Prevent path traversal / absolute paths
            if (path.Contains("..", StringComparison.Ordinal) || path.Contains('\\') || path.Contains(':') || path.Contains("%2e", StringComparison.OrdinalIgnoreCase))
                return BadRequest();

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var isAdmin = User.IsInRole(AppRoles.Admin);

            // Check authorization
            bool authorized = false;
            if (_docs.IsLegacyPublicPath(path))
            {
                // Legacy: lookup owner via DB
                var studentByNational = await _uow.StudentProfiles.FirstOrDefaultAsync(s => s.NationalIdDocumentUrl == path);
                if (studentByNational != null && studentByNational.UserId == userId) authorized = true;
                else
                {
                    var studentByUni = await _uow.StudentProfiles.FirstOrDefaultAsync(s => s.UniversityIdDocumentUrl == path);
                    if (studentByUni != null && studentByUni.UserId == userId) authorized = true;
                    else
                    {
                        var owner = await _uow.OwnerProfiles.FirstOrDefaultAsync(o => o.VerificationDocumentUrl == path);
                        if (owner != null && owner.UserId == userId) authorized = true;
                    }
                }
                if (isAdmin) authorized = true;
            }
            else
            {
                authorized = await _docs.IsAuthorizedAsync(path, userId, isAdmin);
            }

            if (!authorized) return Forbid();

            var result = await _docs.OpenReadAsync(path);
            if (result == null) return NotFound();

            var (stream, contentType, fileName) = result.Value;

            // Security headers: no cache, no sniff
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "private, no-store, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Content-Security-Policy"] = "default-src 'none'; img-src 'self';";

            // Return file; FileStreamResult will dispose stream
            return File(stream, contentType, fileName);
        }

        /// <summary>
        /// Admin-only secure view for any document (used for admin review pages).
        /// Reuses Download but enforces admin role.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> AdminDownload(string? path)
        {
            // Delegates to Download which already allows admin
            return await Download(path);
        }
    }
}
