using Auth.Application.DTOs.Login.Response;
using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;
using Auth.Application.DTOs;
namespace 
Auth.Application.Ports.Outbound;
public interface IAuthenticationRepository
{
Task<Email?> GetPrimaryEmailByUserIdAsync(Guid userId);
 Task<AuthUserInfoDto?> GetUserAuthInfoAsync(Guid userId);
Task<Phone?> GetPrimaryPhoneByUserIdAsync(Guid userId);
Task RegisterAsync(
    Guid userId,
    string? name,
    string? email,
    string? countryCode,
    string? phoneNumber,
    string? passwordHash);
Task<IDbContextTransaction> BeginTransactionAsync();
    Task<LoginUser?> GetByEmailAsync(string email);
Task VerifyPhoneAsync(Guid userId);
    Task SaveRefreshTokenAsync(
        RefreshToken refreshToken);
Task SaveSetPasswordTokenAsync(
    SetPasswordToken token);
Task SavePendingRegistrationAsync(
    PendingRegistration pendingRegistration);

Task<PendingRegistration?> GetPendingRegistrationAsync(
    string? email,
    string? countryCode,
    string? phoneNumber,
    string otpCode);

Task DeletePendingRegistrationAsync(
    PendingRegistration pendingRegistration);
Task<SetPasswordToken?> GetSetPasswordTokenAsync(
    string token);

Task UpdateSetPasswordTokenAsync(
    SetPasswordToken token);

Task<bool> HasActivePasswordAsync(
    Guid userId);

Task SetPasswordAsync(
    Guid userId,
    string passwordHash);
    Task<RefreshToken?> GetRefreshTokenAsync(
        string token);
Task<Otp?> GetResetPasswordOtpAsync(Guid userId);
Task<PendingRegistration?> GetPendingRegistrationByPhoneAsync(
    string countryCode,
    string phoneNumber);

Task UpdatePendingRegistrationAsync(
    PendingRegistration pendingRegistration);
Task ResetPasswordAsync(
    Guid userId,
    string passwordHash);
    Task UpdateRefreshTokenAsync(
        RefreshToken refreshToken);
Task SavePasswordResetTokenAsync(
    PasswordResetToken token);

Task<PasswordResetToken?> GetPasswordResetTokenAsync(
    string token);

Task UpdatePasswordResetTokenAsync(
    PasswordResetToken token);
Task<LoginUser?> GetByPhoneAsync(
    string countryCode,
    string phoneNumber);
Task InvalidatePasswordResetTokensAsync(
    Guid userId);
    Task SaveEmailAsync(Email email);

    Task SavePasswordAsync(Password password);
    Task SaveOtpAsync(Otp otp);

Task<Otp?> GetOtpAsync(
    Guid userId,
    string code,
    int typeId);
Task VerifyContactAsync(Guid userId);
Task UpdateOtpAsync(Otp otp);

Task VerifyEmailAsync(Guid userId);
Task<int> GetOtpTypeIdAsync(string name);
Task<Guid?> GetUserIdByEmailAsync(string email);

Task<Guid?> GetUserIdByPhoneAsync(
    string countryCode,
    string phoneNumber);

// Task<int> GetOtpTypeIdAsync(string name);
Task<string?> GetUserNameAsync(Guid userId);

Task InvalidateActiveOtpsAsync(
    Guid userId,
    int typeId);
    Task DeleteAuthenticationAsync(Guid userId);
}