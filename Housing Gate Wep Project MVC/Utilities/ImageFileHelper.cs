using Microsoft.Extensions.Localization;

namespace StudentHousing.Helpers
{
    /// <summary>
    /// Saves uploaded image files to wwwroot/uploads and returns the relative URL.
    /// </summary>
    public static class ImageFileHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxBytes = 3 * 1024 * 1024; // 3 MB

        /// <summary>
        /// Validates an uploaded image file against the allowed extensions/size.
        /// Returns a localized error message, or null when the file is acceptable.
        /// Callers should run this before SaveAsync so invalid uploads fail cleanly.
        /// </summary>
        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/jpg", "image/png", "image/webp"
        };

        /// <summary>
        /// Extension point for future antivirus scanning. Return error string if infected.
        /// </summary>
        public static Func<IFormFile, Task<string?>>? AntivirusScanner { get; set; }

        public static string? Validate(IFormFile file, IStringLocalizer localizer)
        {
            if (file == null) return localizer["Err.ImgInvalidExtension"];

            // Filename safety: prevent path traversal and control chars
            var originalName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(originalName) || originalName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return localizer["Err.ImgInvalidExtension"];
            }

            var extension = Path.GetExtension(originalName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return localizer["Err.ImgInvalidExtension"];
            }

            if (file.Length == 0 || file.Length > MaxBytes)
            {
                return localizer["Err.ImgTooLarge"];
            }

            // MIME type validation (do not trust extension alone)
            var contentType = file.ContentType?.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(contentType) && !AllowedMimeTypes.Contains(contentType))
            {
                // Fallback: allow generic octet-stream if extension is valid but content-type missing? strict: reject
                return localizer["Err.ImgInvalidExtension"];
            }

            // Magic bytes / file signature validation
            var sigError = ValidateFileSignature(file, extension, localizer);
            if (sigError != null) return sigError;

            // Async antivirus hook (sync wrapper - run synchronously for Validate)
            if (AntivirusScanner != null)
            {
                try
                {
                    var task = AntivirusScanner(file);
                    task.Wait(2000);
                    if (task.IsCompletedSuccessfully && task.Result != null) return task.Result;
                }
                catch { /* ignore scanner errors, fail open but log */ }
            }

            return null;
        }

        private static string? ValidateFileSignature(IFormFile file, string extension, IStringLocalizer localizer)
        {
            try
            {
                // Need to peek first bytes; ensure stream is readable and reset
                using var stream = file.OpenReadStream();
                if (!stream.CanRead || stream.Length < 4) return localizer["Err.ImgInvalidExtension"];
                Span<byte> header = stackalloc byte[12];
                int read = 0;
                while (read < 12)
                {
                    int n = stream.Read(header.Slice(read));
                    if (n == 0) break;
                    read += n;
                }
                // Reset if possible (MemoryStream supports Seek, but FileBufferingReadStream may not)
                if (stream.CanSeek) stream.Position = 0;

                return extension switch
                {
                    ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF ? null : localizer["Err.ImgInvalidExtension"],
                    ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 ? null : localizer["Err.ImgInvalidExtension"],
                    ".webp" => header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50 ? null : localizer["Err.ImgInvalidExtension"],
                    _ => localizer["Err.ImgInvalidExtension"]
                };
            }
            catch
            {
                return localizer["Err.ImgInvalidExtension"];
            }
        }

        public static async Task<string> SaveAsync(IFormFile file, IWebHostEnvironment env)
        {
            var safeName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(safeName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only .jpg, .jpeg, .png and .webp images are allowed.");
            }

            if (file.Length == 0 || file.Length > MaxBytes)
            {
                throw new InvalidOperationException("Image must be between 1 byte and 3 MB.");
            }

            var contentType = file.ContentType?.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(contentType) && !AllowedMimeTypes.Contains(contentType))
            {
                throw new InvalidOperationException("Invalid image MIME type.");
            }

            // Validate magic bytes before writing to disk
            // We use a dummy localizer that returns the key itself for exception path
            var dummyLocalizer = new DummyLocalizer();
            var sigError = ValidateFileSignature(file, extension, dummyLocalizer);
            if (sigError != null) throw new InvalidOperationException(sigError);

            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            // Ensure we copy from start
            if (file is IFormFile formFile)
            {
                await formFile.CopyToAsync(stream);
            }
            else
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }

        private sealed class DummyLocalizer : IStringLocalizer
        {
            public LocalizedString this[string name] => new(name, name);
            public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));
            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Enumerable.Empty<LocalizedString>();
        }

        public static void Delete(string? relativePath, IWebHostEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || !relativePath.StartsWith("/uploads/"))
            {
                return;
            }

            var fileName = Path.GetFileName(relativePath);
            var filePath = Path.Combine(env.WebRootPath, "uploads", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
