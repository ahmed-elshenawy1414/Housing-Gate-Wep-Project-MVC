using StudentHousing.DTOs;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;

namespace StudentHousing.Services.Implementations
{
    public class MatchingService : IMatchingService
    {
        private readonly IUnitOfWork _uow;

        public MatchingService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IReadOnlyList<RoommateMatchDto>> GetMatchesAsync(int myStudentProfileId)
        {
            var me = await _uow.StudentProfiles.GetByIdWithDetailsAsync(myStudentProfileId);
            if (me == null)
            {
                return new List<RoommateMatchDto>();
            }

            var all = await _uow.StudentProfiles.GetAllWithDetailsAsync();
            var others = all.Where(p => p.Id != myStudentProfileId && p.User.IsActive
                && !IsGenderMismatch(me.Gender, p.Gender)).ToList();

            // Batch reviews for all others in one query (Phase 4 fix N+1)
            var otherIds = others.Select(o => o.UserId).ToHashSet();
            var allReviews = await _uow.Users.GetReviewsAboutUsersAsync(otherIds);
            var reviewsByUser = allReviews.GroupBy(r => r.ReviewedUserId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var matches = new List<RoommateMatchDto>();
            foreach (var other in others)
            {
                var score = Score(me, other);
                if (score < 30)
                {
                    continue; // Hide obvious mismatches so the list stays useful.
                }

                reviewsByUser.TryGetValue(other.UserId, out var aboutReviews);
                aboutReviews ??= new List<UserReview>();

                matches.Add(new RoommateMatchDto
                {
                    OtherProfileId = other.Id,
                    OtherUserId = other.UserId,
                    FirstName = other.User.FirstName,
                    LastName = other.User.LastName,
                    University = other.University,
                    Major = other.Major,
                    Score = score,
                    BudgetScore = BudgetScore(me, other),
                    GenderScore = GenderScore(me, other),
                    LifestyleScore = LifestyleScore(me, other),
                    IsVerified = other.VerificationStatus == VerificationStatus.Verified,
                    VerifiedStayCount = other.VerifiedStayCount,
                    AverageRating = aboutReviews.Count == 0 ? 0 : Math.Round(aboutReviews.Average(r => r.Rating), 1),
                    Notes = other.Preference?.Notes
                });
            }

            return matches.OrderByDescending(m => m.Score).Take(20).ToList();
        }

        /// <summary>
        /// Symmetric score: how well "a wants b" averaged with "b wants a".
        /// Each directional score is a weighted 0-100.
        /// </summary>
        public int Score(StudentProfile a, StudentProfile b)
        {
            var aToB = DirectionalScore(a, b);
            var bToA = DirectionalScore(b, a);
            return (int)Math.Round((aToB + bToA) / 2.0);
        }

        private int DirectionalScore(StudentProfile want, StudentProfile person)
        {
            var wantPref = want.Preference;

            var budget = BudgetScore(want, person);
            var gender = GenderScore(want, person);
            var lifestyle = LifestyleScore(want, person);

            // Weights: budget 30, gender 10, lifestyle 60 (smoking 10, sleep 15, noise 15, clean 15, pets 5).
            return (int)Math.Round(
                budget * 0.30 +
                gender * 0.10 +
                lifestyle * 0.60);
        }

        private static int BudgetScore(StudentProfile want, StudentProfile person)
        {
            var wantMin = want.Preference?.MinBudget ?? want.BudgetMin;
            var wantMax = want.Preference?.MaxBudget > 0 ? want.Preference.MaxBudget : want.BudgetMax;
            var personMin = person.BudgetMin;
            var personMax = person.BudgetMax;

            var overlapBottom = Math.Max(wantMin, personMin);
            var overlapTop = Math.Min(wantMax, personMax);
            var overlap = overlapTop - overlapBottom;
            if (overlap <= 0)
            {
                return 0;
            }

            var combinedBottom = Math.Min(wantMin, personMin);
            var combinedTop = Math.Max(wantMax, personMax);
            var combinedRange = Math.Max(1, combinedTop - combinedBottom);

            return (int)Math.Min(100, Math.Round(100.0 * overlap / combinedRange));
        }

        private static int GenderScore(StudentProfile want, StudentProfile person)
        {
            var preferred = want.Preference?.PreferredGender;
            return preferred == null || preferred == person.Gender ? 100 : 0;
        }

        private static bool IsGenderMismatch(Gender a, Gender b)
        {
            // Strict: Male and Female should never be paired
            if (a == Gender.Male && b == Gender.Female) return true;
            if (a == Gender.Female && b == Gender.Male) return true;
            return false;
        }

        private static int LifestyleScore(StudentProfile want, StudentProfile person)
        {
            var wantPref = want.Preference;
            var score = 0;

            // Smoking: 10 pts
            score += (wantPref?.AcceptsSmokers == true || !person.IsSmoker) ? 10 : 0;

            // Sleep schedule: 15 pts
            score += (wantPref?.PreferredSleepSchedule == null || wantPref.PreferredSleepSchedule == person.SleepSchedule) ? 15 : 0;

            // Noise level: 15 pts
            score += (wantPref?.PreferredNoiseLevel == null || wantPref.PreferredNoiseLevel == person.NoiseLevel) ? 15 : 0;

            // Cleanliness: 15 pts
            score += (wantPref?.PreferredCleanliness == null || wantPref.PreferredCleanliness == person.Cleanliness) ? 15 : 0;

            // Pets: 5 pts
            score += (wantPref?.AcceptsPets == true || !person.HasPets) ? 5 : 0;

            return score;
        }
    }
}
