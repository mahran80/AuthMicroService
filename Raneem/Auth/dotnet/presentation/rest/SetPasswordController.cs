using Auth.Application.DTOs.ChangePassword.Request;
using Auth.Application.UseCases.ChangePassword.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Presentation.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class ChangePasswordController : ControllerBase
{
    private readonly ChangePasswordCommand _command;

    public ChangePasswordController(
        ChangePasswordCommand command)
    {
        _command = command;
    }

    [HttpPost("password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        var response =
            await _command.ExecuteAsync(request);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}