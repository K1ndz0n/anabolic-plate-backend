using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipesAPI.DTOs;
using RecipesAPI.Models;
using RecipesAPI.Services;
using RecipesAPI.Services.Utils;

namespace RecipesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly JwtService jwtService;
        private readonly IAuthService authService;

        public AuthController
            (UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            JwtService jwtService, IAuthService authService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.jwtService = jwtService;
            this.authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await authService.IsCaptchaValid(dto.CaptchaToken) == false)
                return BadRequest("Weryfikacja reCAPTCHA nie powiodła się");

            if (await userManager.FindByNameAsync(dto.UserName) != null)
                return BadRequest("Nazwa użytkownika już istnieje");

            if (await userManager.FindByEmailAsync(dto.Email) != null)
                return BadRequest("Email już istnieje");

            if (ProfanityFilter.ContainsProfanity(dto.UserName))
                return BadRequest("Nazwa użytkownika zawiera wulgaryzmy");

            var user = new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };

            var result = await userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized("Nieprawidłowy email lub hasło");

            var result = await signInManager.CheckPasswordSignInAsync(
                user, loginDto.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Nieprawidłowy email lub hasło");

            var token = jwtService.GenerateToken(user);
            return Ok(new { token });
        }
    }
}
