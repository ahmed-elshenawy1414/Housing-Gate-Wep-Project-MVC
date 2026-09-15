using Microsoft.AspNetCore.Identity;
using StudentHousing.Helpers;
using StudentHousing.Models;
using StudentHousing.Repositories.Interfaces;
using StudentHousing.Services.Interfaces;
using StudentHousing.ViewModels.Account;

namespace StudentHousing.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _uow;

        public AccountService(UserManager<ApplicationUser> userManager, IUnitOfWork uow)
        {
            _userManager = userManager;
            _uow = uow;
        }

        public async Task<(bool Success, string Error)> RegisterAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            var role = model.AccountType == AppRoles.Owner ? AppRoles.Owner : AppRoles.Student;
            await _userManager.AddToRoleAsync(user, role);

            if (role == AppRoles.Owner)
            {
                await _uow.OwnerProfiles.AddAsync(new OwnerProfile { UserId = user.Id });
            }
            else
            {
                await _uow.StudentProfiles.AddAsync(new StudentProfile { UserId = user.Id });
            }

            await _uow.SaveChangesAsync();
            return (true, string.Empty);
        }
    }
}
