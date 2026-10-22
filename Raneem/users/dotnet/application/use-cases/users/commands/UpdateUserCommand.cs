using Users.Application.DTOs.Requests;
using Users.Application.DTOs.Responses;
using Users.Application.Ports.Outbound;

namespace Users.Application.UseCases.Commands;

public sealed class UpdateUserCommand
{
    private readonly IUserRepository _repository;

    public UpdateUserCommand(
        IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateUserResponseDto> ExecuteAsync(
        UpdateUserRequestDto request)
    {
        // ================================
        // Get User
        // ================================

        var user =
            await _repository.GetByIdAsync(
                request.UserId);

        if (user is null)
            throw new Exception("User not found.");


        // ================================
        // Check Email
        // ================================

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (await _repository.EmailExistsAsync(
                    request.Email,
                    request.UserId))
            {
                throw new Exception(
                    "Email already exists.");
            }
        }


        // ================================
        // Check Phone
        // ================================

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            if (await _repository.PhoneExistsAsync(
                    request.CountryCode,
                    request.PhoneNumber,
                    request.UserId))
            {
                throw new Exception(
                    "Phone already exists.");
            }
        }


        // ================================
        // Get Authentication Data
        // ================================

        var email =
            await _repository.GetEmailByUserIdAsync(
                request.UserId);

        var phone =
            await _repository.GetPhoneByUserIdAsync(
                request.UserId);


        // ================================
        // Phone must exist
        // ================================

        if (phone is null)
        {
            throw new Exception(
                $"Phone not found for UserId: {request.UserId}");
        }


        // ================================
        // Update Email
        // ================================

        if (email is not null &&
            !string.IsNullOrWhiteSpace(request.Email))
        {
            email.Email = request.Email;
        }


        // ================================
        // Update Phone
        // ================================

        if (!string.IsNullOrWhiteSpace(request.CountryCode))
        {
            phone.CountryCode =
                request.CountryCode;
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            phone.PhoneNumber =
                request.PhoneNumber;
        }


        // ================================
        // Save
        // ================================

        await _repository.UpdateAsync(
            email,
            phone);


        // ================================
        // Response
        // ================================

        return new UpdateUserResponseDto
        {
            UserId = request.UserId,

            Email = email?.Email ?? string.Empty,

            CountryCode =
                phone.CountryCode,

            PhoneNumber =
                phone.PhoneNumber,

            Message =
                "User updated successfully."
        };
    }
}