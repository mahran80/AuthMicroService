using Identity.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.Register;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password) : ICommand<RegisterUserResponse>;
