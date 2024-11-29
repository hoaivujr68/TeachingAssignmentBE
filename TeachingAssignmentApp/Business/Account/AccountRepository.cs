using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Helper;
using TeachingAssignmentApp.Model;

namespace TeachingAssignmentApp.Business.Account
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountRepository(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<SignInResponse> SignInAsync(SignInModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            if (user == null || !passwordValid)
            {
                return null;
            }

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role.ToString()));
            }

            var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(20),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature)
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            return new SignInResponse
            {
                Token = jwtToken,
                Username = user.UserName,  // Hoặc bạn có thể dùng user.Email nếu muốn trả về email
                Roles = userRoles.ToList()  // Trả về danh sách vai trò
            };
        }

        public async Task<IdentityResult> SignUpAsync(SignUpModel model)
        {
            var user = new User
            {
                Email = model.Email,
                UserName = model.UserName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Tạo vai trò Teacher nếu chưa tồn tại
                if (!await _roleManager.RoleExistsAsync(AppRole.Teacher))
                {
                    await _roleManager.CreateAsync(new IdentityRole(AppRole.Teacher));
                }

                // Gán vai trò Teacher cho tất cả người dùng
                await _userManager.AddToRoleAsync(user, AppRole.Teacher);
            }

            return result;
        }

    }
}
