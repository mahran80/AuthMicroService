// using Auth.Application.DTOs.Login.Request;
// using Auth.Application.UseCases.Login.Commands;
// using Microsoft.AspNetCore.Mvc;

// namespace Auth.Presentation.Controllers;

// [ApiController]
// [Route("api/auth")]
// public sealed class AuthController : ControllerBase
// {
//     private readonly LoginCommand _loginCommand;

//     public AuthController(LoginCommand loginCommand)
//     {
//         _loginCommand = loginCommand;
//     }

//     [HttpPost("login")]
//     public async Task<IActionResult> Login(LoginRequest request)
//     {
//         var result = await _loginCommand.ExecuteAsync(request);

//         return Ok(result);
//     }
// }