using Auth.Application.DTOs.Logout.Request;
using Auth.Application.UseCases.Logout.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class LogoutController : ControllerBase
{
    private readonly LogoutCommand _logoutCommand;

    public LogoutController(
        LogoutCommand logoutCommand)
    {
        _logoutCommand = logoutCommand;
    }

[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    var refreshToken = Request.Cookies["refreshToken"];

    if (string.IsNullOrEmpty(refreshToken))
    {
        return Unauthorized("Refresh token missing.");
    }

    await _logoutCommand.ExecuteAsync(refreshToken);

    Response.Cookies.Delete("refreshToken");

    return Ok(new
    {
        Message = "Logged out successfully."
    });
}
}