using Identity.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.ResendOtp;

public sealed record ResendOtpCommand(
    string? Email,
    string? CountryCode,
    string? PhoneNumber,
    string Purpose) : ICommand<OtpDispatchedResponse>;
