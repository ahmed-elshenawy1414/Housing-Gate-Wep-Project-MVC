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
        private readonly StudentHousing.Services.Interfaces.IDocumentStorageService _docs;

        public StudentService(IUnitOfWork uow, IWebHostEnvironment env, IStringLocalizer<SharedResource> L, StudentHousing.Services.Interfaces.IDocumentStorageService docs)
        {
            _uow = uow;
            _env = env;
            _L = L;
            _docs = docs;
        }

        public async Task<StudentProfile?> GetMyProfileAsync(string userId)
            => await _uow.StudentProfiles.GetByUserIdAsync(userId);

        public async Task<(bool Success, string Error)> UpdateProfileAsync(string userId, StudentProfileViewModel model)
        {
            if (model.Gender != Gender.Male && model.Gender != Gender.Female)
                return (false, _L["Err.GenderRequired"]);
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

            var nationalRes = await _docs.SavePrivateAsync(nationalId, userId, "student-verify");
            if (!nationalRes.Success) return (false, nationalRes.Error ?? _L["Err.UploadFailed"]);
            var universityRes = await _docs.SavePrivateAsync(universityId, userId, "student-verify");
            if (!universityRes.Success)
            {
                // Rollback first file
                _docs.DeletePrivate(nationalRes.PrivatePath);
                return (false, universityRes.Error ?? _L["Err.UploadFailed"]);
            }
            string nationalPath = nationalRes.PrivatePath!;
            string universityPath = universityRes.PrivatePath!;

            // Delete old private documents (or legacy public files)
            _docs.DeletePrivate(profile.NationalIdDocumentUrl);
            // Fallback delete for legacy public path via ImageFileHelper
            if (!string.IsNullOrEmpty(profile.NationalIdDocumentUrl) && profile.NationalIdDocumentUrl.StartsWith("/uploads/"))
                ImageFileHelper.Delete(profile.NationalIdDocumentUrl, _env);

            _docs.DeletePrivate(profile.UniversityIdDocumentUrl);
            if (!string.IsNullOrEmpty(profile.UniversityIdDocumentUrl) && profile.UniversityIdDocumentUrl.StartsWith("/uploads/"))
                ImageFileHelper.Delete(profile.UniversityIdDocumentUrl, _env);

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
            var completed = stays.Where(s => s.Status == StayStatus.Completed).ToList();
            if (completed.Count == 0) return Array.Empty<Stay>();

            // Batch: one query for all stays (fix N+1)
            var stayIds = completed.Select(s => s.Id).ToHashSet();
            var reviewerId = completed.First().StudentProfile.UserId;
            var reviewedIds = (await _uow.Reviews.ListAsync(r => r.StayId != 0 && stayIds.Contains(r.StayId) && r.ReviewerId == reviewerId))
                .Select(r => r.StayId).ToHashSet();

            return completed.Where(s => !reviewedIds.Contains(s.Id)).ToList();
        }
    }
}
