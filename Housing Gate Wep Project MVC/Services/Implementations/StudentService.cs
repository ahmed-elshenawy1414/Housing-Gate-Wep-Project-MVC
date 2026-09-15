using Microsoft.Extensions.Localization;
using StudentHousing.Helpers;
using StudentHousing.Mappings;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Resources;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Student;

namespace StudentHousing.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<SharedResource> _L;

        public StudentService(IUnitOfWork uow, IWebHostEnvironment env, IStringLocalizer<SharedResource> L)
        {
            _uow = uow;
            _env = env;
            _L = L;
        }

        public async Task<StudentProfile?> GetMyProfileAsync(string userId)
            => await _uow.StudentProfiles.GetByUserIdAsync(userId);

        public async Task<(bool Success, string Error)> UpdateProfileAsync(string userId, StudentProfileViewModel model)
        {
            var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (profile == null)
            {
                return (false, _L["Err.StudentProfileNotFoundFull"]);
            }

            profile.University = model.University;
            profile.Major = model.Major;
            profile.Governorate = model.Governorate;
            profile.District = model.District;
            profile.AcademicYear = model.AcademicYear;
            profile.DateOfBirth = model.DateOfBirth;
            profile.Gender = model.Gender;
            profile.Bio = model.Bio;
            profile.BudgetMin = model.BudgetMin;
            profile.BudgetMax = model.BudgetMax;
            profile.MoveInDate = model.MoveInDate;
            profile.MinLeaseMonths = model.MinLeaseMonths;
            profile.IsSmoker = model.IsSmoker;
            profile.SleepSchedule = model.SleepSchedule;
            profile.NoiseLevel = model.NoiseLevel;
            profile.Cleanliness = model.Cleanliness;
            profile.HasPets = model.HasPets;

            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> UpdatePreferenceAsync(string userId, RoommatePreferenceViewModel model)
        {
            var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (profile == null)
            {
                return (false, _L["Err.StudentProfileNotFound"]);
            }

            var preference = profile.Preference ?? new RoommatePreference { StudentProfileId = profile.Id };

            preference.PreferredGender = model.PreferredGender;
            preference.MinBudget = model.MinBudget;
            preference.MaxBudget = model.MaxBudget;
            preference.AcceptsSmokers = model.AcceptsSmokers;
            preference.PreferredSleepSchedule = model.PreferredSleepSchedule;
            preference.PreferredNoiseLevel = model.PreferredNoiseLevel;
            preference.PreferredCleanliness = model.PreferredCleanliness;
            preference.AcceptsPets = model.AcceptsPets;
            preference.Notes = model.Notes;
            preference.UpdatedAt = DateTime.UtcNow;

            if (profile.Preference == null)
            {
                await _uow.RoommatePreferences.AddAsync(preference);
            }
            else
            {
                _uow.RoommatePreferences.Update(preference);
            }

            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> SubmitVerificationAsync(string userId, IFormFile nationalId, IFormFile universityId)
        {
            if (nationalId == null || nationalId.Length == 0)
            {
                return (false, _L["Err.NationalIdRequired"]);
            }

            if (universityId == null || universityId.Length == 0)
            {
                return (false, _L["Err.UniversityIdRequired"]);
            }

            var nationalError = ImageFileHelper.Validate(nationalId, _L);
            if (nationalError != null)
            {
                return (false, nationalError);
            }

            var universityError = ImageFileHelper.Validate(universityId, _L);
            if (universityError != null)
            {
                return (false, universityError);
            }

            var profile = await _uow.StudentProfiles.GetByUserIdAsync(userId);
            if (profile == null)
            {
                return (false, _L["Err.StudentProfileNotFound"]);
            }

            string nationalPath;
            string universityPath;
            try
            {
                nationalPath = await ImageFileHelper.SaveAsync(nationalId, _env);
                universityPath = await ImageFileHelper.SaveAsync(universityId, _env);
            }
            catch (Exception)
            {
                return (false, _L["Err.UploadFailed"]);
            }

            if (profile.NationalIdDocumentUrl != null)
            {
                ImageFileHelper.Delete(profile.NationalIdDocumentUrl, _env);
            }

            if (profile.UniversityIdDocumentUrl != null)
            {
                ImageFileHelper.Delete(profile.UniversityIdDocumentUrl, _env);
            }

            profile.NationalIdDocumentUrl = nationalPath;
            profile.UniversityIdDocumentUrl = universityPath;
            profile.VerificationRejectReason = null;
            profile.VerificationStatus = VerificationStatus.Pending;
            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<IReadOnlyList<Stay>> GetCompletedStaysWithoutReviewAsync(int studentProfileId)
        {
            var stays = await _uow.Stays.GetByStudentProfileAsync(studentProfileId);
            var result = new List<Stay>();

            foreach (var stay in stays.Where(s => s.Status == StayStatus.Completed))
            {
                var reviewed = await _uow.Reviews.AlreadyReviewedStayAsync(stay.Id, stay.StudentProfile.UserId);
                if (!reviewed)
                {
                    result.Add(stay);
                }
            }

            return result;
        }
    }
}
