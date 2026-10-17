using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;

namespace Identity.Application.Features.Register;

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherPort _passwordHasher;
    private readonly IRbacAccessPort _rbacAccessPort;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasherPort passwordHasher,
        IRbacAccessPort rbacAccessPort)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _rbacAccessPort = rbacAccessPort;
    }

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            return Result.Failure<RegisterUserResponse>(IdentityErrors.EmailAlreadyInUse);
        }

        string passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = User.Create(
            normalizedEmail,
            passwordHash,
            request.FirstName,
            request.LastName);

        await _userRepository.AddUserAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        const string defaultRole = "StandardUser";
        var rbacResult = await _rbacAccessPort.AssignDefaultRoleAsync(user.Id, defaultRole, cancellationToken);

        if (rbacResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(rbacResult.Error);
        }

        var response = new RegisterUserResponse(
            user.Id,
            user.Email,
            defaultRole,
            user.CreatedAtUtc);

        return Result.Success(response);
    }
}
