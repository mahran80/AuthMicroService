using Auth.Application.DTOs.Register.Request;
using Auth.Application.UseCases.Register.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class RegisterController : ControllerBase
{
    private readonly RegisterCommand _registerCommand;

    public RegisterController(RegisterCommand registerCommand)
    {
        _registerCommand = registerCommand;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            var result = await _registerCommand.ExecuteAsync(request);

            Console.WriteLine(result.OtpCode);

            return Ok(result);
        }
        catch (Exception ex)
        {
            if (ex.Message == "Phone number already exists.")
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }

            if (ex.Message == "Email already exists.")
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message
                });
            }

            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}