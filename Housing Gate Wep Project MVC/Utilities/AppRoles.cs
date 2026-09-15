namespace StudentHousing.Helpers
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Owner = "Owner";
        public const string Student = "Student";

        public static readonly string[] All = { Admin, Owner, Student };
    }
}
