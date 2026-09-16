using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Services.Implementations
{
    public class DocumentStorageService : IDocumentStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<SharedResource> _L;
        private readonly ILogger<DocumentStorageService> _logger;

        private static readonly string PrivateRootFolder = Path.Combine("App_Data", "private-documents");

        public DocumentStorageService(IWebHostEnvironment env, IStringLocalizer<SharedResource> localizer, ILogger<DocumentStorageService> logger)
        {
            _env = env;
            _L = localizer;
            _logger = logger;
        }

        public async Task<(bool Success, string? PrivatePath, string? Error)> SavePrivateAsync(IFormFile file, string userId, string category)
        {
            var error = ImageFileHelper.Validate(file, _L);
            if (error != null) return (false, null, error);

            // Additional MIME/magic-byte check (defense in depth, ImageFileHelper already does)
            var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
            var allowedMime = new HashSet<string> { "image/jpeg", "image/jpg", "image/png", "image/webp" };
            if (!string.IsNullOrEmpty(contentType) && !allowedMime.Contains(contentType))
            {
                return (false, null, _L["Err.ImgInvalidExtension"]);
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            // Sanitize category
            var safeCategory = string.Join("_", category.Split(Path.GetInvalidFileNameChars()));
            safeCategory = safeCategory.Replace("/", "_").Replace("\\", "_");
            if (string.IsNullOrWhiteSpace(safeCategory)) safeCategory = "general";
            // Sanitize userId (it's a GUID-like string)
            var safeUserId = string.Join("_", userId.Split(Path.GetInvalidFileNameChars()));
            safeUserId = safeUserId.Replace("/", "_").Replace("\\", "_");

            var basePath = Path.Combine(_env.ContentRootPath, PrivateRootFolder, safeCategory, safeUserId);
            Directory.CreateDirectory(basePath);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(basePath, fileName);

            try
            {
                await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, useAsync: true);
                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save private document for user {UserId} category {Category}", userId, category);
                return (false, null, _L["Err.UploadFailed"]);
            }

            // Store as relative private path with forward slashes, never expose absolute path
            var relative = Path.Combine("private", safeCategory, safeUserId, fileName).Replace("\\", "/");
            return (true, relative, null);
        }

        public void DeletePrivate(string? privatePath)
        {
            if (string.IsNullOrWhiteSpace(privatePath)) return;
            // Only delete private paths
            if (privatePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                ImageFileHelper.Delete(privatePath, _env);
                return;
            }
            if (!privatePath.StartsWith("private/", StringComparison.OrdinalIgnoreCase)) return;

            try
            {
                var fullPath = Path.Combine(_env.ContentRootPath, PrivateRootFolder, privatePath.Substring("private/".Length).Replace("/", Path.DirectorySeparatorChar.ToString()));
                // Prevent path traversal: ensure fullPath is under private root
                var root = Path.GetFullPath(Path.Combine(_env.ContentRootPath, PrivateRootFolder));
                var target = Path.GetFullPath(fullPath);
                if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase)) return;
                if (File.Exists(target)) File.Delete(target);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete private document {Path}", privatePath);
            }
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(string privatePath)
        {
            if (string.IsNullOrWhiteSpace(privatePath)) return null;

            // Legacy public path fallback (for migration period)
            if (IsLegacyPublicPath(privatePath))
            {
                var legacyFileName = Path.GetFileName(privatePath);
                var legacyFull = Path.Combine(_env.WebRootPath, "uploads", legacyFileName);
                if (!File.Exists(legacyFull)) return null;
                var ext = Path.GetExtension(legacyFull).ToLowerInvariant();
                var ct = GetContentType(ext);
                var stream = new FileStream(legacyFull, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                return (stream, ct, legacyFileName);
            }

            if (!privatePath.StartsWith("private/", StringComparison.OrdinalIgnoreCase)) return null;

            var relative = privatePath.Substring("private/".Length);
            var fullPath = Path.Combine(_env.ContentRootPath, PrivateRootFolder, relative.Replace("/", Path.DirectorySeparatorChar.ToString()));
            var root = Path.GetFullPath(Path.Combine(_env.ContentRootPath, PrivateRootFolder));
            var target = Path.GetFullPath(fullPath);
            if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase)) return null;
            if (!File.Exists(target)) return null;

            var fileExt = Path.GetExtension(target).ToLowerInvariant();
            var contentType = GetContentType(fileExt);
            var fileStream = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
            return (fileStream, contentType, Path.GetFileName(target));
        }

        public bool IsLegacyPublicPath(string? path) =>
            !string.IsNullOrWhiteSpace(path) && path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase);

        public async Task<string?> MigrateLegacyIfNeededAsync(string? legacyPath, string userId, string category)
        {
            if (string.IsNullOrWhiteSpace(legacyPath)) return legacyPath;
            if (!IsLegacyPublicPath(legacyPath)) return legacyPath;

            var fileName = Path.GetFileName(legacyPath);
            var legacyFull = Path.Combine(_env.WebRootPath, "uploads", fileName);
            if (!File.Exists(legacyFull)) return legacyPath;

            // Read legacy file and re-save via private storage to apply validation and secure naming
            try
            {
                await using var fs = new FileStream(legacyFull, FileMode.Open, FileAccess.Read, FileShare.Read);
                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                var safeCategory = string.Join("_", category.Split(Path.GetInvalidFileNameChars())).Replace("/", "_").Replace("\\", "_");
                if (string.IsNullOrWhiteSpace(safeCategory)) safeCategory = "general";
                var safeUserId = string.Join("_", userId.Split(Path.GetInvalidFileNameChars())).Replace("/", "_").Replace("\\", "_");
                var basePath = Path.Combine(_env.ContentRootPath, PrivateRootFolder, safeCategory, safeUserId);
                Directory.CreateDirectory(basePath);
                var newFileName = $"{Guid.NewGuid():N}{ext}";
                var newFull = Path.Combine(basePath, newFileName);
                await using var dest = new FileStream(newFull, FileMode.CreateNew);
                await fs.CopyToAsync(dest);

                // Move legacy file to backup outside wwwroot to close anon static access, preserve for rollback
                try
                {
                    var backupRoot = Path.Combine(_env.ContentRootPath, "App_Data", "legacy-backup", "uploads");
                    Directory.CreateDirectory(backupRoot);
                    var backupPath = Path.Combine(backupRoot, fileName);
                    // If backup already exists, keep original legacy for idempotency until DB updated
                    if (!File.Exists(backupPath))
                    {
                        File.Move(legacyFull, backupPath);
                        _logger.LogInformation("Moved legacy file {Legacy} to backup {Backup}", legacyFull, backupPath);
                    }
                    else
                    {
                        // Backup exists, safe to delete legacy after successful private copy
                        File.Delete(legacyFull);
                        _logger.LogInformation("Deleted legacy file {Legacy} after backup exists", legacyFull);
                    }
                }
                catch (Exception backupEx)
                {
                    _logger.LogWarning(backupEx, "Failed to backup legacy file {Legacy} — leaving in place but will block via middleware", legacyFull);
                    // Do not return legacy path; still return new private path — static access will be blocked by middleware
                }

                var newRelative = Path.Combine("private", safeCategory, safeUserId, newFileName).Replace("\\", "/");
                _logger.LogInformation("Migrated legacy document {Legacy} to {New} for user {UserId}", legacyPath, newRelative, userId);
                return newRelative;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate legacy document {Path}", legacyPath);
                return legacyPath;
            }
        }

        public async Task<bool> IsAuthorizedAsync(string privatePath, string requesterUserId, bool isAdmin)
        {
            if (isAdmin) return true;
            if (string.IsNullOrWhiteSpace(privatePath)) return false;
            // Private path format: private/{category}/{userId}/{file}
            var parts = privatePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return false; // private/category/userId/file
            var ownerUserId = parts[2];
            return string.Equals(ownerUserId, requesterUserId, StringComparison.Ordinal);
        }

        private static string GetContentType(string ext) => ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
