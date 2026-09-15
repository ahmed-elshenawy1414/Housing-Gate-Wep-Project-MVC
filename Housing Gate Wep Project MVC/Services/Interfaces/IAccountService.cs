using StudentHousing.ViewModels.Account;

namespace StudentHousing.Services.Interfaces
{
    public interface IAccountService
    {
        /// <summary>
        /// Creates the user, assigns the chosen role (Student / Owner) and creates
        /// the matching profile record.
        /// </summary>
        Task<(bool Success, string Error)> RegisterAsync(RegisterViewModel model);
    }
}
