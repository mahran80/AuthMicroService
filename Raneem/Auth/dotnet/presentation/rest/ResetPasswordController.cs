// using Auth.Application.DTOs.ResetPassword.Request;
// using Auth.Application.UseCases.ResetPassword.Commands;
// using Microsoft.AspNetCore.Mvc;

// namespace Auth.Presentation.Api.Controllers;

// [ApiController]
// [Route("api/auth")]
// public sealed class ResetPasswordController : ControllerBase
// {
//     private readonly ResetPasswordCommand _resetPasswordCommand;

//     public ResetPasswordController(
//         ResetPasswordCommand resetPasswordCommand)
//     {
//         _resetPasswordCommand = resetPasswordCommand;
//     }

//     [HttpPost("reset-password")]
//     public async Task<IActionResult> ResetPassword(
//         ResetPasswordRequest request)
//     {
//         var response =
//             await _resetPasswordCommand.ExecuteAsync(request);

//         if (!response.Success)
//         {
//             return BadRequest(response);
//         }

//         return Ok(response);
//     }
// }