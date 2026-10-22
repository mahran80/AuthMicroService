using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
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
    private readonly IUserProfilePort _userProfilePort;
    private readonly IOtpCodeGeneratorPort _otpCodeGenerator;
    private readonly IWhatsAppNotificationPort _whatsAppNotificationPort;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasherPort passwordHasher,
        IRbacAccessPort rbacAccessPort,
        IUserProfilePort userProfilePort,
        IOtpCodeGeneratorPort otpCodeGenerator,
        IWhatsAppNotificationPort whatsAppNotificationPort)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _rbacAccessPort = rbacAccessPort;
        _userProfilePort = userProfilePort;
        _otpCodeGenerator = otpCodeGenerator;
        _whatsAppNotificationPort = whatsAppNotificationPort;
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

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            if (await _userRepository.ExistsByPhoneAsync(request.CountryCode ?? string.Empty, request.PhoneNumber, cancellationToken))
            {
                return Result.Failure<RegisterUserResponse>(IdentityErrors.PhoneAlreadyInUse);
            }
        }

        string passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = User.Create(
            normalizedEmail,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.CountryCode,
            request.PhoneNumber,
            isEmailVerified: false);

        await _userRepository.AddUserAsync(user, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            string otpCode = _otpCodeGenerator.GenerateSixDigitCode();
            string codeHash = _otpCodeGenerator.ComputeSha256Hash(otpCode);

            var otp = OtpVerification.Create(
                user.Id,
                request.CountryCode,
                request.PhoneNumber,
                user.Email,
                codeHash,
                OtpPurpose.Register);

            await _userRepository.AddOtpVerificationAsync(otp, cancellationToken);
            await _whatsAppNotificationPort.SendOtpMessageAsync(
                request.CountryCode ?? string.Empty,
                request.PhoneNumber,
                otpCode,
                cancellationToken);
        }

        await _userRepository.SaveChangesAsync(cancellationToken);

        const string defaultRole = "StandardUser";
        var rbacResult = await _rbacAccessPort.AssignDefaultRoleAsync(user.Id, defaultRole, cancellationToken);

        if (rbacResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(rbacResult.Error);
        }

        await _userProfilePort.CreateOrSyncProfileAsync(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            request.CountryCode,
            request.PhoneNumber,
            cancellationToken);

        var response = new RegisterUserResponse(
            user.Id,
            user.Email,
            defaultRole,
            user.CreatedAtUtc);

        return Result.Success(response);
    }
}
