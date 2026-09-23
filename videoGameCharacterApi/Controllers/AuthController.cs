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
        => Ok(await authService.RegisterAsync(request));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        return result is null
            ? Problem(detail: "Invalid username or password.", statusCode: StatusCodes.Status401Unauthorized)
            : Ok(result);
    }
}
