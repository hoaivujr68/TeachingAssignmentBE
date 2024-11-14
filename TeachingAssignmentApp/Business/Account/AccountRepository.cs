using Microsoft.AspNetCore.Identity;
using TeachingAssignmentApp.Data;

namespace TeachingAssignmentApp.Business.Account
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountRepository(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<SignInResult> SignInAsync(string email, string password)
        {
            return await _signInManager.PasswordSignInAsync(email, password, false, false);
        }

        public async Task<IdentityResult> SignUpAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }
    }
}
