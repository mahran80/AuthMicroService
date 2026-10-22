using Microsoft.AspNetCore.Mvc;
using Users.Application.UseCases.Commands;
using Users.Application.DTOs.Requests;

namespace Users.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly CreateUserCommand _createUserCommand;

    public UsersController(CreateUserCommand createUserCommand)
    {
        _createUserCommand = createUserCommand;
    }

  [HttpPost]
public async Task<IActionResult> Create(
    [FromBody] CreateUserRequestDto request)
{
    var result = await _createUserCommand.ExecuteAsync(request);

    return Ok(result);
}
}