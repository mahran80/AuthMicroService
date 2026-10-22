namespace Users.Application.DTOs.Responses;

public sealed class CreateUserResponseDto
{
    public Guid UserId { get; set; }

    public string Message { get; set; } = string.Empty;
}