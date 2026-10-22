using Auth.Application.DTOs.LoginWeb.Request;
using Auth.Application.UseCases.LoginWeb.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Presentation.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class LoginWebController : ControllerBase
{
    private readonly LoginWebCommand _loginWebCommand;

    public LoginWebController(
        LoginWebCommand loginWebCommand)
    {
        _loginWebCommand = loginWebCommand;
    }


    [HttpPost("login/web")]
    public async Task<IActionResult> LoginWeb(
        LoginWebRequest request)
    {
        var response =
            await _loginWebCommand.ExecuteAsync(request);


        if (!response.Success)
        {
            return BadRequest(response);
        }


        Response.Cookies.Append(
            "refreshToken",
            response.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                  Expires = response.RememberMe
            ? response.RefreshTokenExpiresAt
            : null
            });


        return Ok(new
        {
            response.Success,
            response.Message,
            response.AccessToken,
            response.AccessTokenExpiresAt
        });
    }
}