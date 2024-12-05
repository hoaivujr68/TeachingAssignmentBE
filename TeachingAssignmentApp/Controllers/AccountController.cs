using Microsoft.AspNetCore.Mvc;
using TeachingAssignmentApp.Business.Account;
using TeachingAssignmentApp.Model;

namespace MyApiNetCore6.Controllers
{
    [Microsoft.AspNetCore.Components.Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public AccountsController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp(SignUpModel signUpModel)
        {
            var result = await _accountRepository.SignUpAsync(signUpModel);
            if (result.Succeeded)
            {
                return Ok(new { Success = true, Message = "Registration successful" });
            }

            return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            var signInResponse = await _accountRepository.SignInAsync(signInModel);

            if (signInResponse == null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid credentials" });
            }

            return Ok(new { Success = true, Data = signInResponse });
        }

        [HttpGet("create")]
        public async Task<IActionResult> SignIn()
        {
            await _accountRepository.CreateAccountWithRoleAsync();
            return Ok(new { Success = true, Message = "Account created" });
        }
    }
}

