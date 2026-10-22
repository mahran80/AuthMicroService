using Identity.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.LoginMobile;

public sealed record LoginMobileCommand(
    string CountryCode,
    string PhoneNumber) : ICommand<OtpDispatchedResponse>;
