using Microsoft.AspNetCore.Identity;
using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Repositories
{
    public interface IAccountRepository
    {
        Task<SignInResult> SignInAsync(string email, string password);
        Task<IdentityResult> SignUpAsync(User user, string password);
    }
}
