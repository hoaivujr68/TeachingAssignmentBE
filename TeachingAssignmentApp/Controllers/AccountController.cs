using Microsoft.AspNetCore.Mvc;
using TeachingAssignmentApp.Business.Account;
using TeachingAssignmentApp.Model;

namespace MyApiNetCore6.Controllers
{
    [Microsoft.AspNetCore.Components.Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp(SignUpModel signUpModel)
        {
            var result = await _accountService.SignUpAsync(signUpModel);
            if (result.Succeeded)
            {
                return Ok(new { Success = true, Message = "Registration successful" });
            }

            return BadRequest(new { Success = false, Errors = result.Errors.Select(e => e.Description) });
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(SignInModel signInModel)
        {
            var token = await _accountService.SignInAsync(signInModel);

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { Success = false, Message = "Invalid credentials" });
            }

            return Ok(new { Success = true, Token = token });
        }
    }
}

