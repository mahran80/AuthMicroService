// using Users.Application.DTOs.Requests;
// using Users.Application.Ports.Outbound;
// using Users.Domain.Entities;

// namespace Users.Application.UseCases.Commands;

// public sealed class CreateUserCommand
// {
//     private readonly IUserRepository _repository;
//     private readonly IEmailService _emailService;

//     public CreateUserCommand(
//         IUserRepository repository,
//         IEmailService emailService)
//     {
//         _repository = repository;
//         _emailService = emailService;
//     }


//     public async Task<User> ExecuteAsync(CreateUserRequestDto request)
//     {
//         if (!string.IsNullOrWhiteSpace(request.Email))
// {
//     if (await _repository.EmailExistsAsync(request.Email))
//     {
//         throw new Exception("Email already exists.");
//     }
// }


//         if (await _repository.PhoneExistsAsync(
//                 request.CountryCode,
//                 request.PhoneNumber))
//         {
//             throw new Exception("Phone already exists.");
//         }


//         var userId = Guid.NewGuid();


//         var user = new User
//         {
//             UserId = userId
//         };


//         var email = new UserEmail
//         {
//             EmailId = Guid.NewGuid(),
//             UserId = userId,
//             Email = request.Email,
//             IsPrimary = true,
//             IsVerified = false
//         };


//         var phone = new UserPhone
//         {
//             PhoneId = Guid.NewGuid(),
//             UserId = userId,
//             CountryCode = request.CountryCode,
//             PhoneNumber = request.PhoneNumber,
//             IsPrimary = true,
//             IsVerified = false
//         };


//         // Create User
//         var createdUser =
//             await _repository.CreateAsync(
//                 user,
//                 email,
//                 phone);
// // await _authGrpcClient.SendWelcomePasswordLinkAsync(
// //     userId,
// //     request.Email);


//         // Generate Magic Link Token
//         var token =
//             Guid.NewGuid().ToString();



// var setPasswordToken = new SetPasswordToken
// {
//     SetPasswordTokenId = Guid.NewGuid(),
//     UserId = userId,
//     Token = token,
//     CreatedAt = DateTime.UtcNow,
//     ExpireAt = DateTime.UtcNow.AddHours(24),
//     IsUsed = false
// };

// await _repository.CreateSetPasswordTokenAsync(setPasswordToken);



//         // Send Magic Link

//         var link =
//             $"http://localhost:4200/#/set-password?token={token}";


//         await _emailService.SendAsync(
//             request.Email,
//             "Set your password",
//             $"""
//             <h2>Welcome to Lady Driver</h2>

//             <p>Your account has been created.</p>

//             <p>
//             Click the link below to set your password:
//             </p>

//             <a href="{link}">
//                 Set Password
//             </a>

//             <p>
//             This link expires after 24 hours.
//             </p>
//             """);


//         return createdUser;
//     }
// }
using Users.Application.DTOs.Requests;
using Users.Application.Ports.Outbound;
using Users.Domain.Entities;

namespace Users.Application.UseCases.Commands;

public sealed class CreateUserCommand
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;

    public CreateUserCommand(
        IUserRepository repository,
        IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public async Task<User> ExecuteAsync(CreateUserRequestDto request)
    {
        // ============================
        // Check Email only if provided
        // ============================

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await _repository.EmailExistsAsync(request.Email))
            {
                throw new Exception("Email already exists.");
            }
        }

        // ============================
        // Check Phone
        // ============================

        if (await _repository.PhoneExistsAsync(
                request.CountryCode,
                request.PhoneNumber))
        {
            throw new Exception("Phone already exists.");
        }

        // ============================
        // Create User
        // ============================

        var userId = Guid.NewGuid();

        var user = new User
        {
            UserId = userId
        };

        // ============================
        // Create Phone
        // ============================

    var phone = new UserPhone
{
    PhoneId = Guid.NewGuid(),
    UserId = userId,
    Name = request.Name,
    CountryCode = request.CountryCode,
    PhoneNumber = request.PhoneNumber,
    IsPrimary = true,
    IsVerified = false
};

        // ============================
        // Create Email only if provided
        // ============================

        UserEmail? email = null;

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            email = new UserEmail
            {
                EmailId = Guid.NewGuid(),
                UserId = userId,
                Email = request.Email,
                IsPrimary = true,
                IsVerified = false
            };
        }

        // ============================
        // Save User
        // ============================

        var createdUser =
            await _repository.CreateAsync(
                user,
                email,
                phone);

        // ============================
        // Email-dependent operations
        // ============================

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Generate Magic Link Token
            var token = Guid.NewGuid().ToString();

            var setPasswordToken = new SetPasswordToken
            {
                SetPasswordTokenId = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            };

            await _repository.CreateSetPasswordTokenAsync(
                setPasswordToken);

            // Send Magic Link
            var link =
                $"http://localhost:4200/#/set-password?token={token}";

            await _emailService.SendAsync(
                request.Email,
                "Set your password",
                $"""
                <h2>Welcome to Lady Driver</h2>

                <p>Your account has been created.</p>

                <p>
                    Click the link below to set your password:
                </p>

                <a href="{link}">
                    Set Password
                </a>

                <p>
                    This link expires after 24 hours.
                </p>
                """);
        }

        return createdUser;
    }
}