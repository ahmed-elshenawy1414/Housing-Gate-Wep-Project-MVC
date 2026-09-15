using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using StudentHousing.Resources;

namespace StudentHousing.Helpers
{
    /// <summary>
    /// Localizes the error descriptions produced by ASP.NET Core Identity so that
    /// account/verification failures surface in the current UI culture instead of
    /// hardcoded English. Registered via AddIdentity(...).AddErrorDescriber().
    /// </summary>
    public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer<SharedResource> _L;

        public LocalizedIdentityErrorDescriber(IStringLocalizer<SharedResource> L)
        {
            _L = L;
        }

        public override IdentityError DefaultError()
            => Error(nameof(DefaultError), _L["Identity.DefaultError"]);

        public override IdentityError ConcurrencyFailure()
            => Error(nameof(ConcurrencyFailure), _L["Identity.ConcurrencyFailure"]);

        public override IdentityError PasswordMismatch()
            => Error(nameof(PasswordMismatch), _L["Identity.PasswordMismatch"]);

        public override IdentityError InvalidToken()
            => Error(nameof(InvalidToken), _L["Identity.InvalidToken"]);

        public override IdentityError RecoveryCodeRedemptionFailed()
            => Error(nameof(RecoveryCodeRedemptionFailed), _L["Identity.RecoveryCodeRedemptionFailed"]);

        public override IdentityError LoginAlreadyAssociated()
            => Error(nameof(LoginAlreadyAssociated), _L["Identity.LoginAlreadyAssociated"]);

        public override IdentityError InvalidUserName(string? userName)
            => Error(nameof(InvalidUserName), _L["Identity.InvalidUserName", userName ?? string.Empty]);

        public override IdentityError InvalidEmail(string? email)
            => Error(nameof(InvalidEmail), _L["Identity.InvalidEmail", email ?? string.Empty]);

        public override IdentityError DuplicateUserName(string userName)
            => Error(nameof(DuplicateUserName), _L["Identity.DuplicateUserName", userName]);

        public override IdentityError DuplicateEmail(string email)
            => Error(nameof(DuplicateEmail), _L["Identity.DuplicateEmail", email]);

        public override IdentityError InvalidRoleName(string? role)
            => Error(nameof(InvalidRoleName), _L["Identity.InvalidRoleName", role ?? string.Empty]);

        public override IdentityError DuplicateRoleName(string role)
            => Error(nameof(DuplicateRoleName), _L["Identity.DuplicateRoleName", role]);

        public override IdentityError UserAlreadyHasPassword()
            => Error(nameof(UserAlreadyHasPassword), _L["Identity.UserAlreadyHasPassword"]);

        public override IdentityError UserLockoutNotEnabled()
            => Error(nameof(UserLockoutNotEnabled), _L["Identity.UserLockoutNotEnabled"]);

        public override IdentityError UserAlreadyInRole(string role)
            => Error(nameof(UserAlreadyInRole), _L["Identity.UserAlreadyInRole", role]);

        public override IdentityError UserNotInRole(string role)
            => Error(nameof(UserNotInRole), _L["Identity.UserNotInRole", role]);

        public override IdentityError PasswordTooShort(int length)
            => Error(nameof(PasswordTooShort), _L["Identity.PasswordTooShort", length]);

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => Error(nameof(PasswordRequiresUniqueChars), _L["Identity.PasswordRequiresUniqueChars", uniqueChars]);

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => Error(nameof(PasswordRequiresNonAlphanumeric), _L["Identity.PasswordRequiresNonAlphanumeric"]);

        public override IdentityError PasswordRequiresDigit()
            => Error(nameof(PasswordRequiresDigit), _L["Identity.PasswordRequiresDigit"]);

        public override IdentityError PasswordRequiresLower()
            => Error(nameof(PasswordRequiresLower), _L["Identity.PasswordRequiresLower"]);

        public override IdentityError PasswordRequiresUpper()
            => Error(nameof(PasswordRequiresUpper), _L["Identity.PasswordRequiresUpper"]);

        private static IdentityError Error(string code, string description)
            => new IdentityError { Code = code, Description = description };
    }
}
