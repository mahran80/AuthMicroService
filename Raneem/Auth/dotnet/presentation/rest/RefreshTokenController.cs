using Auth.Application.DTOs.RefreshToken.Request;
using Auth.Application.UseCases.RefreshToken.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class RefreshTokenController : ControllerBase
{
    private readonly RefreshTokenCommand _refreshTokenCommand;

    public RefreshTokenController(
        RefreshTokenCommand refreshTokenCommand)
    {
        _refreshTokenCommand = refreshTokenCommand;
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        Console.WriteLine("Refresh endpoint called");

        var refreshToken =
            Request.Cookies["refreshToken"];


        Console.WriteLine("Refresh Token From Cookie:");
        Console.WriteLine(refreshToken);


        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Refresh token missing");
        }


        var result =
            await _refreshTokenCommand.ExecuteAsync(
                new RefreshTokenRequest
                {
                    RefreshToken = refreshToken
                });


        // Update Cookie with the new refresh token
        Response.Cookies.Append(
            "refreshToken",
            result.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // localhost
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });


        return Ok(new
        {
            accessToken = result.AccessToken
        });
    }
}