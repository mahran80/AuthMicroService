using Auth.Application.DTOs.ForgotPassword.Request;
using Auth.Application.UseCases.ForgotPassword.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Presentation.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class ForgotPasswordController : ControllerBase
{
    private readonly ForgotPasswordCommand _forgotPasswordCommand;

    public ForgotPasswordController(
        ForgotPasswordCommand forgotPasswordCommand)
    {
        _forgotPasswordCommand = forgotPasswordCommand;
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request)
    {
        var response =
            await _forgotPasswordCommand.ExecuteAsync(request);

        return Ok(response);
    }
}