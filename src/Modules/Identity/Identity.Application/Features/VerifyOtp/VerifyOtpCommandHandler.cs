using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;

namespace Identity.Application.Features.VerifyOtp;

public sealed class VerifyOtpCommandHandler : ICommandHandler<VerifyOtpCommand, AuthTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpCodeGeneratorPort _otpCodeGenerator;
    private readonly IRbacAccessPort _rbacAccessPort;
    private readonly IJwtProviderPort _jwtProvider;

    public VerifyOtpCommandHandler(
        IUserRepository userRepository,
        IOtpCodeGeneratorPort otpCodeGenerator,
        IRbacAccessPort rbacAccessPort,
        IJwtProviderPort jwtProvider)
    {
        _userRepository = userRepository;
        _otpCodeGenerator = otpCodeGenerator;
        _rbacAccessPort = rbacAccessPort;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<AuthTokenResponse>> Handle(
        VerifyOtpCommand request,
        CancellationToken cancellationToken)
    {
        User? user = null;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.CountryCode) && !string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            user = await _userRepository.GetByPhoneAsync(
                request.CountryCode.Trim(),
                request.PhoneNumber.Trim(),
                cancellationToken);
        }

        if (user is null)
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.UserNotFound);
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.AccountNotActive);
        }

        if (!Enum.TryParse<OtpPurpose>(request.Purpose, true, out var purpose))
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.InvalidOrExpiredOtp);
        }

        var otp = await _userRepository.GetLatestActiveOtpAsync(user.Id, purpose, cancellationToken);
        if (otp is null || !otp.IsValid)
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.InvalidOrExpiredOtp);
        }

        string inputHash = _otpCodeGenerator.ComputeSha256Hash(request.Code.Trim());
        if (!string.Equals(inputHash, otp.CodeHash, StringComparison.OrdinalIgnoreCase))
        {
            otp.IncrementFailedAttempts();
            await _userRepository.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthTokenResponse>(IdentityErrors.InvalidOrExpiredOtp);
        }

        otp.MarkAsUsed();

        if (purpose == OtpPurpose.Register)
        {
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) || !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                user.VerifyPhone();
            }

            if (!string.IsNullOrWhiteSpace(request.Email) || !string.IsNullOrWhiteSpace(user.Email))
            {
                user.VerifyEmail();
            }
        }
        else if (purpose == OtpPurpose.MobileLogin)
        {
            user.VerifyPhone();
        }

        var accessProfile = await _rbacAccessPort.GetUserAccessProfileAsync(user.Id, cancellationToken);

        var tokenPair = _jwtProvider.GenerateTokenPair(
            user,
            accessProfile.Roles,
            accessProfile.Permissions);

        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenPair.RefreshToken,
            tokenPair.JwtId,
            tokenPair.RefreshTokenExpiresAtUtc);

        var session = LoginSession.Create(
            user.Id,
            refreshToken.Id,
            request.DeviceName,
            request.DeviceType,
            request.IpAddress,
            request.UserAgent);

        await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _userRepository.AddLoginSessionAsync(session, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var userContext = new UserContextResponse(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            accessProfile.Roles,
            accessProfile.Permissions);

        var response = new AuthTokenResponse(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiresAtUtc,
            tokenPair.RefreshTokenExpiresAtUtc,
            userContext);

        return Result.Success(response);
    }
}
