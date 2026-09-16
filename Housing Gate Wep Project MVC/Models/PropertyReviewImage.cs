namespace StudentHousing.Models
{
    public class PropertyReviewImage
    {
        public int Id { get; set; }
        public int PropertyReviewId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public PropertyReview PropertyReview { get; set; } = null!;
    }
}
