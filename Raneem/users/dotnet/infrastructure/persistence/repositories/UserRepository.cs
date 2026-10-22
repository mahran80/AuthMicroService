using Microsoft.EntityFrameworkCore;
using Users.Application.Ports.Outbound;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence.Context;

namespace Users.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UsersDbContext _context;

    public UserRepository(UsersDbContext context)
    {
        _context = context;
    }
//====================================================================
public async Task<bool> EmailExistsAsync(string? email)
{
    if (string.IsNullOrWhiteSpace(email))
        return false;

    return await _context.UserEmails
        .AnyAsync(x => x.Email == email);
}
//====================================================================    
public async Task DeleteAsync(Guid userId)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(x => x.UserId == userId);

    if (user is null)
        throw new Exception("User not found.");

    user.IsDeleted = true;

    await _context.SaveChangesAsync();
}
//====================================================================
    public async Task<bool> PhoneExistsAsync(
        string countryCode,
        string phoneNumber)
    {
        return await _context.UserPhones
            .AnyAsync(x =>
                x.CountryCode == countryCode &&
                x.PhoneNumber == phoneNumber);
    }
   //==================================================================== 
public async Task CreateSetPasswordTokenAsync(
    SetPasswordToken token)
{
    Console.WriteLine("Saving SetPasswordToken...");

    _context.SetPasswordTokens.Add(token);

    var rows = await _context.SaveChangesAsync();

    Console.WriteLine($"Rows = {rows}");
}
//====================================================================
public async Task<User?> GetByIdAsync(Guid userId)
{
    return await _context.Users
        .FirstOrDefaultAsync(x => x.UserId == userId);
}
//====================================================================
public async Task<UserEmail?> GetEmailByUserIdAsync(Guid userId)
{
    return await _context.UserEmails
        .FirstOrDefaultAsync(x => x.UserId == userId);
}
//====================================================================
public async Task<UserPhone?> GetPhoneByUserIdAsync(Guid userId)
{
    return await _context.UserPhones
        .FirstOrDefaultAsync(x => x.UserId == userId);
}
//====================================================================
public async Task<bool> EmailExistsAsync(
    string email,
    Guid userId)
{
    return await _context.UserEmails.AnyAsync(x =>
        x.Email == email &&
        x.UserId != userId);
}
//====================================================================
public async Task<bool> PhoneExistsAsync(
    string countryCode,
    string phoneNumber,
    Guid userId)
{
    return await _context.UserPhones.AnyAsync(x =>
        x.CountryCode == countryCode &&
        x.PhoneNumber == phoneNumber &&
        x.UserId != userId);
}
//====================================================================
public async Task UpdateAsync(
    UserEmail email,
    UserPhone phone)
{
    // _context.UserEmails.Update(email);
    // _context.UserPhones.Update(phone);

    await _context.SaveChangesAsync();
}
//====================================================================
    // public async Task<User> CreateAsync(
    //     User user,
    //     UserEmail email,
    //     UserPhone phone)
    // {
    //     await using var transaction =
    //         await _context.Database.BeginTransactionAsync();

    //     try
    //     {
    //         _context.Users.Add(user);
    //         _context.UserEmails.Add(email);
    //         _context.UserPhones.Add(phone);

    //         await _context.SaveChangesAsync();

    //         await transaction.CommitAsync();

    //         return user;
    //     }
    //     catch
    //     {
    //         await transaction.RollbackAsync();
    //         throw;
    //     }
    // }
    public async Task<User> CreateAsync(
    User user,
    UserEmail? email,
    UserPhone phone)
{
    await using var transaction =
        await _context.Database.BeginTransactionAsync();

    try
    {
        _context.Users.Add(user);

        if (email != null)
        {
            _context.UserEmails.Add(email);
        }

        _context.UserPhones.Add(phone);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return user;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
//====================================================================    
    public async Task<User?> GetByPhoneAsync(string phoneNumber)
{
    var phone = await _context.UserPhones
        .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);

    if (phone is null)
        return null;

    return await _context.Users
        .FirstOrDefaultAsync(x => x.UserId == phone.UserId);
}
}