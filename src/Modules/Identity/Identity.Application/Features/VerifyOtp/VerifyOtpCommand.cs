using Identity.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.VerifyOtp;

public sealed record VerifyOtpCommand(
    string? Email,
    string? CountryCode,
    string? PhoneNumber,
    string Code,
    string Purpose,
    string? DeviceName = null,
    string? DeviceType = null,
    string? IpAddress = null,
    string? UserAgent = null) : ICommand<AuthTokenResponse>;
