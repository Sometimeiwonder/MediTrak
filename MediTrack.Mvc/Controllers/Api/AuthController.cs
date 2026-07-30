using MediTrack.Mvc.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediTrack.Mvc.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Ok(new { authenticated = false });

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Ok(new { authenticated = false });

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            authenticated = true,
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            roles
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(
            request.email, request.password, request.rememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Unauthorized(new { error = "Invalid email or password" });

        var user = await _userManager.FindByEmailAsync(request.email);
        if (user == null)
            return Unauthorized(new { error = "Invalid email or password" });

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            authenticated = true,
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            roles
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.password))
            return BadRequest(new { error = "Email and password are required" });

        var user = new ApplicationUser
        {
            UserName = request.email,
            Email = request.email,
            FullName = request.fullName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.password);
        if (!result.Succeeded)
            return BadRequest(new { error = string.Join(", ", result.Errors.Select(e => e.Description)) });

        await _userManager.AddToRoleAsync(user, "User");
        await _signInManager.SignInAsync(user, isPersistent: false);

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            roles = new[] { "User" }
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logged out" });
    }
}

public class LoginRequest
{
    public string email { get; set; } = "";
    public string password { get; set; } = "";
    public bool rememberMe { get; set; }
}

public class RegisterRequest
{
    public string email { get; set; } = "";
    public string password { get; set; } = "";
    public string? fullName { get; set; }
}
