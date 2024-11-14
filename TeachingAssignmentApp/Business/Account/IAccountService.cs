using Microsoft.AspNetCore.Identity;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Account
{
    public interface IAccountService
    {
        Task<string> SignInAsync(SignInModel model);
        Task<IdentityResult> SignUpAsync(SignUpModel model);
    }
}
