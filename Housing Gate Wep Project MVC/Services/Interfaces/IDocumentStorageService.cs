namespace StudentHousing.Services.Interfaces
{
    /// <summary>
    /// Secure private storage for identity / verification documents.
    /// Files are stored outside wwwroot (App_Data/private-documents) and
    /// served only via an authorized controller.
    /// </summary>
    public interface IDocumentStorageService
    {
        /// <summary>Validates and saves a private document. Returns the private relative path (e.g. "private/verify/{userId}/{guid}.jpg").</summary>
        Task<(bool Success, string? PrivatePath, string? Error)> SavePrivateAsync(IFormFile file, string userId, string category);

        /// <summary>Deletes a private document if it exists. No throw.</summary>
        void DeletePrivate(string? privatePath);

        /// <summary>Gets file info for authorized serving. Returns null if not found.</summary>
        Task<(Stream Stream, string ContentType, string FileName)?> OpenReadAsync(string privatePath);

        /// <summary>Returns true if the stored path is a legacy public /uploads/ path.</summary>
        bool IsLegacyPublicPath(string? path);

        /// <summary>Migrates a legacy /uploads/ file to private storage if it exists. Returns new private path or original.</summary>
        Task<string?> MigrateLegacyIfNeededAsync(string? legacyPath, string userId, string category);

        /// <summary>Checks whether a user is authorized to access a given private document.</summary>
        Task<bool> IsAuthorizedAsync(string privatePath, string requesterUserId, bool isAdmin);
    }
}
