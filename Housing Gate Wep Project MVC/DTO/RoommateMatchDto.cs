namespace StudentHousing.DTOs
{
    /// <summary>
    /// The output of the roommate matching engine: two profiles, an overall score
    /// and a per-criterion breakdown so the score is easy to understand.
    /// </summary>
    public class RoommateMatchDto
    {
        public int OtherProfileId { get; set; }
        public string OtherUserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string University { get; set; } = string.Empty;
        public string? Major { get; set; }
        public int Score { get; set; }
        public int BudgetScore { get; set; }
        public int GenderScore { get; set; }
        public int LifestyleScore { get; set; }
        public bool IsVerified { get; set; }
        public int VerifiedStayCount { get; set; }
        public double AverageRating { get; set; }
        public string? Notes { get; set; }
    }
}
