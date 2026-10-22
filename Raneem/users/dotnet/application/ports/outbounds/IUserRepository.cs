using Users.Domain.Entities;

namespace Users.Application.Ports.Outbound;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);

    Task CreateSetPasswordTokenAsync(
        SetPasswordToken token);

    Task<User?> GetByIdAsync(Guid userId);

    Task<User?> GetByPhoneAsync(
        string phoneNumber);
    Task<UserEmail?> GetEmailByUserIdAsync(Guid userId);

    Task<UserPhone> GetPhoneByUserIdAsync(Guid userId);

    Task<bool> EmailExistsAsync(
        string email,
        Guid excludeUserId);

    Task DeleteAsync(Guid userId);

    Task<bool> PhoneExistsAsync(
        string countryCode,
        string phoneNumber,
        Guid excludeUserId);

    Task UpdateAsync(
        UserEmail email,
        UserPhone phone);

    Task<bool> PhoneExistsAsync(
        string countryCode,
        string phoneNumber);

    Task<User> CreateAsync(
        User user,
        UserEmail email,
        UserPhone phone);
}