
namespace Authentication.Application.DTOs;

public sealed class LinkedInUserDto
{
    public required string Subject { get; init; }

    public string? Email { get; init; }

    public string? Name { get; init; }

    public string? PictureUrl { get; init; }
}
