using Microsoft.AspNetCore.Mvc;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);

        return result.Status switch
        {
            RegisterStatus.Success => Ok(result.Response),
            RegisterStatus.UsernameTaken => Problem(
                detail: "Username is already taken.",
                statusCode: StatusCodes.Status409Conflict),
            _ => Problem(
                detail: $"Username must be at least {AuthService.MinUsernameLength} characters (spaces at the start and end are ignored).",
                statusCode: StatusCodes.Status400BadRequest)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        return result is null
            ? Problem(detail: "Invalid username or password.", statusCode: StatusCodes.Status401Unauthorized)
            : Ok(result);
    }
}
