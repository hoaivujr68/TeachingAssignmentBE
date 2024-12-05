using Microsoft.AspNetCore.Identity;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Account
{
    public interface IAccountRepository
    {
        Task<SignInResponse> SignInAsync(SignInModel model);
        Task<IdentityResult> SignUpAsync(SignUpModel model);
        Task CreateAccountWithRoleAsync();
    }
}
