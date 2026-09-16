namespace StudentHousing.Helpers
{
    /// <summary>
    /// Conceptual data visibility levels for privacy enforcement.
    /// Used to reason about what a given caller may see.
    /// </summary>
    public enum PrivacyLevel
    {
        Public,
        AuthenticatedUser,
        RelatedParty,
        OwnerOnly,
        StudentOnly,
        AdminOnly,
        Sensitive
    }

    public static class PrivacyHelper
    {
        /// <summary>
        /// Returns true if the caller (ownerId) is the owner of the property.
        /// </summary>
        public static bool IsOwnerOfProperty(string callerUserId, string propertyOwnerId) =>
            string.Equals(callerUserId, propertyOwnerId, StringComparison.Ordinal);

        public static bool IsOwnerOfApplication(string callerUserId, string applicationOwnerId) =>
            string.Equals(callerUserId, applicationOwnerId, StringComparison.Ordinal);

        public static bool IsStudentOfApplication(string callerUserId, string studentUserId) =>
            string.Equals(callerUserId, studentUserId, StringComparison.Ordinal);

        public static bool IsRelatedPartyForStay(string callerUserId, string stayStudentUserId, string propertyOwnerId, bool isAdmin)
        {
            if (isAdmin) return true;
            return string.Equals(callerUserId, stayStudentUserId, StringComparison.Ordinal)
                || string.Equals(callerUserId, propertyOwnerId, StringComparison.Ordinal);
        }

        /// <summary>
        /// Sensitive fields must never be serialized to public DTOs. Central check for logging.
        /// </summary>
        public static bool IsSensitiveField(string propertyName) =>
            propertyName is nameof(Models.StudentProfile.NationalIdDocumentUrl)
                or nameof(Models.StudentProfile.UniversityIdDocumentUrl)
                or nameof(Models.OwnerProfile.VerificationDocumentUrl)
                or nameof(Models.ApplicationUser.PasswordHash)
                or nameof(Models.ApplicationUser.SecurityStamp)
                or nameof(Models.ApplicationUser.ConcurrencyStamp);
    }
}
