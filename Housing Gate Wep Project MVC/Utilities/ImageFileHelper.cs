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
        public static string? Validate(IFormFile file, IStringLocalizer localizer)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return localizer["Err.ImgInvalidExtension"];
            }

            if (file.Length == 0 || file.Length > MaxBytes)
            {
                return localizer["Err.ImgTooLarge"];
            }

            return null;
        }

        public static async Task<string> SaveAsync(IFormFile file, IWebHostEnvironment env)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only .jpg, .jpeg, .png and .webp images are allowed.");
            }

            if (file.Length == 0 || file.Length > MaxBytes)
            {
                throw new InvalidOperationException("Image must be between 1 byte and 3 MB.");
            }

            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{fileName}";
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
