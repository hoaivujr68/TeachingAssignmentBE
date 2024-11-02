using Microsoft.AspNetCore.Identity;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Services
{
    public interface IAccountService
    {
        Task<string> SignInAsync(SignInModel model);
        Task<IdentityResult> SignUpAsync(SignUpModel model);
    }
}
