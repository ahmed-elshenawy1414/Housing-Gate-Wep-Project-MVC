using StudentHousing.Models;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Mappings
{
    public static class StudentMapper
    {
        public static StudentProfileViewModel ToProfileViewModel(StudentProfile profile)
        {
            return new StudentProfileViewModel
            {
                University = profile.University,
                Major = profile.Major,
                Governorate = profile.Governorate,
                District = profile.District,
                AcademicYear = profile.AcademicYear,
                DateOfBirth = profile.DateOfBirth,
                Gender = profile.Gender,
                Bio = profile.Bio,
                BudgetMin = profile.BudgetMin,
                BudgetMax = profile.BudgetMax,
                MoveInDate = profile.MoveInDate,
                MinLeaseMonths = profile.MinLeaseMonths,
                IsSmoker = profile.IsSmoker,
                SleepSchedule = profile.SleepSchedule,
                NoiseLevel = profile.NoiseLevel,
                Cleanliness = profile.Cleanliness,
                HasPets = profile.HasPets,
                VerificationStatus = profile.VerificationStatus,
                VerifiedStayCount = profile.VerifiedStayCount,
                VerificationRejectReason = profile.VerificationRejectReason,
                HasNationalIdDocument = !string.IsNullOrEmpty(profile.NationalIdDocumentUrl),
                HasUniversityIdDocument = !string.IsNullOrEmpty(profile.UniversityIdDocumentUrl),
                ProfilePictureUrl = profile.User?.ProfilePictureUrl,
                Email = profile.User?.Email,
                FirstName = profile.User?.FirstName ?? string.Empty,
                LastName = profile.User?.LastName ?? string.Empty
            };
        }

        public static RoommatePreferenceViewModel ToPreferenceViewModel(RoommatePreference? preference)
        {
            if (preference == null)
            {
                return new RoommatePreferenceViewModel();
            }

            return new RoommatePreferenceViewModel
            {
                PreferredGender = preference.PreferredGender,
                MinBudget = preference.MinBudget,
                MaxBudget = preference.MaxBudget,
                AcceptsSmokers = preference.AcceptsSmokers,
                PreferredSleepSchedule = preference.PreferredSleepSchedule,
                PreferredNoiseLevel = preference.PreferredNoiseLevel,
                PreferredCleanliness = preference.PreferredCleanliness,
                AcceptsPets = preference.AcceptsPets,
                Notes = preference.Notes
            };
        }
    }
}
