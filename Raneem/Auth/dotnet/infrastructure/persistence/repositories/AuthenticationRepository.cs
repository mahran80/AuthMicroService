using Auth.Application.DTOs.Login.Response;
using Auth.Application.Ports.Outbound;
using Auth.Domain.Entities;
using Auth.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Auth.Application.DTOs;
namespace Auth.Infrastructure.Persistence.Repositories;

public sealed class AuthenticationRepository : IAuthenticationRepository
{
    private readonly AuthenticationDbContext _context;

    public AuthenticationRepository(AuthenticationDbContext context)
    {
        _context = context;
    }
    //==========================================================================
public async Task<AuthUserInfoDto?> GetUserAuthInfoAsync(Guid userId)
{
    var phone = await _context.Phones
        .FirstOrDefaultAsync(x => x.UserId == userId);


    var email = await _context.Emails
        .FirstOrDefaultAsync(x => x.UserId == userId);


    if(phone == null)
        return null;


    return new AuthUserInfoDto
    {
        Name = phone.Name,

        CountryCode = phone.CountryCode,

        PhoneNumber = phone.PhoneNumber,

        Email = email?.EmailAddress ?? ""
    };
}
//=====================Register========================
public async Task RegisterAsync(
    Guid userId,
    string? name,
    string? email,
    string? countryCode,
    string? phoneNumber,
    string? passwordHash)
{
    await _context.Database.ExecuteSqlInterpolatedAsync($@"
        EXEC dbo.usp_RegisterUser
            @UserId={userId},
            @Name={name},
            @Email={email},
            @CountryCode={countryCode},
            @PhoneNumber={phoneNumber},
            @PasswordHash={passwordHash}");
}
//==========================================================================
public async Task<Email?> GetPrimaryEmailByUserIdAsync(Guid userId)
{
    return await _context.Emails
        .FirstOrDefaultAsync(x => x.UserId == userId && x.IsPrimary);
}
//==========================================================================
public async Task<Phone?> GetPrimaryPhoneByUserIdAsync(Guid userId)
{
    return await _context.Phones
        .FirstOrDefaultAsync(x => x.UserId == userId && x.IsPrimary);
}

//==========================================================================
public async Task<Otp?> GetResetPasswordOtpAsync(
    Guid userId)
{
    var forgotPasswordTypeId =
        await GetOtpTypeIdAsync("ForgotPassword");

    return await _context.Otps
        .Where(x =>
            x.UserId == userId &&
            x.TypeId == forgotPasswordTypeId &&
            x.CanResetPassword)
        .OrderByDescending(x => x.ExpireAt)
        .FirstOrDefaultAsync();
}
//==========================================================================
public async Task SaveSetPasswordTokenAsync(
    SetPasswordToken token)
{
    await _context.SetPasswordTokens.AddAsync(token);

    await _context.SaveChangesAsync();
}
//==========================================================================
public async Task UpdateSetPasswordTokenAsync(
    SetPasswordToken token)
{
    _context.SetPasswordTokens.Update(token);

    await _context.SaveChangesAsync();
}//==========================================================================
public async Task<string?> GetUserNameAsync(Guid userId)
{
    return await _context.Phones
        .Where(x => x.UserId == userId)
        .Select(x => x.Name)
        .FirstOrDefaultAsync();
}
//==========================================================================
public async Task<SetPasswordToken?> GetSetPasswordTokenAsync(
    string token)
{
    Console.WriteLine($"Request Token = '{token}'");

    var all = await _context.SetPasswordTokens.ToListAsync();

    Console.WriteLine($"Count = {all.Count}");

    foreach (var item in all)
    {
        Console.WriteLine($"DB Token = '{item.Token}', IsUsed = {item.IsUsed}");
    }

    return await _context.SetPasswordTokens
        .FirstOrDefaultAsync(x =>
            x.Token == token &&
            !x.IsUsed);
}
//==========================================================================

public async Task<IDbContextTransaction> BeginTransactionAsync()
{
    return await _context.Database.BeginTransactionAsync();
}
//==========================================================================
public async Task<PendingRegistration?> GetPendingRegistrationByPhoneAsync(
    string countryCode,
    string phoneNumber)
{
    return await _context.PendingRegistrations
        .FirstOrDefaultAsync(x =>
            x.CountryCode == countryCode &&
            x.PhoneNumber == phoneNumber);
}
//==========================================================================
public async Task SaveOtpAsync(Otp otp)
{
    await _context.Otps.AddAsync(otp);

    await _context.SaveChangesAsync();
}
//==========================================================================
public async Task UpdatePendingRegistrationAsync(
    PendingRegistration pendingRegistration)
{
    _context.PendingRegistrations.Update(
        pendingRegistration);

    await _context.SaveChangesAsync();
}
//==========================================================================
public async Task<bool> HasActivePasswordAsync(
    Guid userId)
{
    return await _context.Passwords
        .AnyAsync(x =>
            x.UserId == userId &&
            x.IsActive);
}
//==========================================================================
public async Task SetPasswordAsync(
    Guid userId,
    string passwordHash)
{
    await _context.Passwords.AddAsync(
        new Password
        {
            PasswordId = Guid.NewGuid(),
            UserId = userId,
            PasswordHash = passwordHash,
            PasswordAlgorithm = "BCrypt",
            IsActive = true
        });

    await _context.SaveChangesAsync();
}
//==========================================================================
public async Task<LoginUser?> GetByPhoneAsync(
    string countryCode,
    string phoneNumber)
{ Console.WriteLine($"CountryCode = '{countryCode}'");
Console.WriteLine($"PhoneNumber = '{phoneNumber}'");
    return await _context.Phones
        .Where(x =>
            x.CountryCode == countryCode &&
            x.PhoneNumber == phoneNumber)
        .Select(x => new LoginUser
        {
            UserId = x.UserId
        })
        .FirstOrDefaultAsync();
}
//==========================================================================
    public async Task<LoginUser?> GetByEmailAsync(string email)
    {
        return await
        (
            from e in _context.Emails
            join p in _context.Passwords
                on e.UserId equals p.UserId
            where e.EmailAddress == email
                  && p.IsActive
            select new LoginUser
            {
                UserId = e.UserId,
                Email = e.EmailAddress,
                PasswordHash = p.PasswordHash
            }
        ).FirstOrDefaultAsync();
    }
    //==========================================================================
public async Task SaveRefreshTokenAsync(
    RefreshToken refreshToken)
{
    await _context.RefreshTokens.AddAsync(refreshToken);

var rows = await _context.SaveChangesAsync();

    Console.WriteLine($"Rows Saved = {rows}");
    Console.WriteLine($"Token = {refreshToken.Token}");
}
//==========================================================================
public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
{
    return await _context.RefreshTokens
        .FirstOrDefaultAsync(x =>
            x.Token == token &&
            !x.IsRevoked);
}
//==========================================================================
public async Task UpdateRefreshTokenAsync(
    RefreshToken token)
{
    _context.RefreshTokens.Update(token);

    var rows = await _context.SaveChangesAsync();

    Console.WriteLine($"Rows Affected = {rows}");
}
//==========================================================================
    public async Task SaveEmailAsync(Email email)
{
    await _context.Emails.AddAsync(email);
    await _context.SaveChangesAsync();
}
//==========================================================================
public async Task SavePasswordAsync(Password password)
{
    await _context.Passwords.AddAsync(password);
    await _context.SaveChangesAsync();
}
//==========================================================================
public async Task<Otp?> GetOtpAsync(
    Guid userId,
    string code,
    int typeId)
{
    var all = await _context.Otps
        .Where(x => x.UserId == userId)
        .ToListAsync();

    foreach (var item in all)
    {
        Console.WriteLine(
            $"DB -> Code={item.Code}, TypeId={item.TypeId}, IsUsed={item.IsUsed}, Project={item.ProjectType}");
    }

    return await _context.Otps
        .FirstOrDefaultAsync(x =>
            x.UserId == userId &&
            x.Code == code &&
            x.TypeId == typeId &&
            !x.IsUsed);
}
//==========================================================================
public async Task VerifyContactAsync(Guid userId)
{
    var email = await _context.Emails
        .FirstOrDefaultAsync(x =>
            x.UserId == userId &&
            x.IsPrimary);

    if (email is not null)
    {
        email.IsVerified = true;

        await _context.SaveChangesAsync();

        return;
    }

    var phone = await _context.Phones
        .FirstOrDefaultAsync(x =>
            x.UserId == userId &&
            x.IsPrimary);

    if (phone is not null)
    {
        phone.IsVerified = true;

        await _context.SaveChangesAsync();
    }
}
//==========================================================================
public async Task ResetPasswordAsync(
    Guid userId,
    string passwordHash)
{
    var activePassword = await _context.Passwords
        .FirstAsync(x =>
            x.UserId == userId &&
            x.IsActive);

    activePassword.IsActive = false;

    await _context.Passwords.AddAsync(
        new Password
        {
            PasswordId = Guid.NewGuid(),
            UserId = userId,
            PasswordHash = passwordHash,
            PasswordAlgorithm = "BCrypt",
            IsActive = true
        });

    await _context.SaveChangesAsync();
}
//==========================================================================
public async Task SavePendingRegistrationAsync(
    PendingRegistration pendingRegistration)
{
    await _context.PendingRegistrations.AddAsync(
        pendingRegistration);

    await _context.SaveChangesAsync();
}
public async Task<PendingRegistration?> GetPendingRegistrationAsync(
    string? email,
    string? countryCode,
    string? phoneNumber,
    string otpCode)
{
    return await _context.PendingRegistrations
        .FirstOrDefaultAsync(x =>

            x.OtpCode == otpCode &&

            (

                (!string.IsNullOrWhiteSpace(email)
                    && x.Email == email)

                ||

                (!string.IsNullOrWhiteSpace(phoneNumber)
                    && x.CountryCode == countryCode
                    && x.PhoneNumber == phoneNumber)

            ));
}
public async Task DeletePendingRegistrationAsync(
    PendingRegistration pendingRegistration)
{
    _context.PendingRegistrations.Remove(
        pendingRegistration);

    await _context.SaveChangesAsync();
}
public async Task UpdateOtpAsync(Otp otp)
{
    _context.Otps.Update(otp);

    await _context.SaveChangesAsync();
}
public async Task<int> GetOtpTypeIdAsync(string name)
{
    return await _context.OtpTypes
        .Where(x => x.Name == name)
        .Select(x => x.TypeId)
        .FirstAsync();
}
public async Task SavePasswordResetTokenAsync(
    PasswordResetToken token)
{
    await _context.PasswordResetTokens.AddAsync(token);

    await _context.SaveChangesAsync();
}
public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(
    string token)
{
    return await _context.PasswordResetTokens
        .FirstOrDefaultAsync(x =>
            x.Token == token &&
            !x.IsUsed);
}
public async Task UpdatePasswordResetTokenAsync(
    PasswordResetToken token)
{
    _context.PasswordResetTokens.Update(token);

    await _context.SaveChangesAsync();
}
public async Task InvalidatePasswordResetTokensAsync(
    Guid userId)
{
    var tokens =
        await _context.PasswordResetTokens
            .Where(x =>
                x.UserId == userId &&
                !x.IsUsed)
            .ToListAsync();

    foreach (var token in tokens)
    {
        token.IsUsed = true;
    }

    await _context.SaveChangesAsync();
}
public async Task VerifyEmailAsync(Guid userId)
{
    var email = await _context.Emails
        .FirstAsync(x =>
            x.UserId == userId &&
            x.IsPrimary);

    email.IsVerified = true;

    await _context.SaveChangesAsync();
}
public async Task<Guid?> GetUserIdByEmailAsync(string email)
{
    return await _context.Emails
        .Where(x => x.EmailAddress == email)
        .Select(x => (Guid?)x.UserId)
        .FirstOrDefaultAsync();
}
public async Task VerifyPhoneAsync(Guid userId)
{
    var phone = await _context.Phones
        .FirstAsync(x =>
            x.UserId == userId &&
            x.IsPrimary);

    phone.IsVerified = true;

    await _context.SaveChangesAsync();
}
public async Task<Guid?> GetUserIdByPhoneAsync(
    string countryCode,
    string phoneNumber)
{
    return await _context.Phones
        .Where(x =>
            x.CountryCode == countryCode &&
            x.PhoneNumber == phoneNumber 
          
            )
        .Select(x => (Guid?)x.UserId)
        .FirstOrDefaultAsync();
}
public async Task InvalidateActiveOtpsAsync(
    Guid userId,
    int typeId)
{
    var otps = await _context.Otps
        .Where(x =>
            x.UserId == userId &&
            x.TypeId == typeId &&
            !x.IsUsed)
        .ToListAsync();

    foreach (var otp in otps)
    {
        otp.IsUsed = true;
    }

    await _context.SaveChangesAsync();
}
public async Task DeleteAuthenticationAsync(Guid userId)
{
    var phones = await _context.Phones
        .Where(x => x.UserId == userId)
        .ToListAsync();

    foreach (var phone in phones)
    {
        phone.IsDeleted = true;
        phone.DeletedAt = DateTime.UtcNow;
    }

    var emails = await _context.Emails
        .Where(x => x.UserId == userId)
        .ToListAsync();

    foreach (var email in emails)
    {
        email.IsDeleted = true;
        email.DeletedAt = DateTime.UtcNow;
    }

    var passwords = await _context.Passwords
        .Where(x => x.UserId == userId)
        .ToListAsync();

    foreach (var password in passwords)
    {
        password.IsDeleted = true;
        password.IsActive = false;
        password.DeletedAt = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();
}
}