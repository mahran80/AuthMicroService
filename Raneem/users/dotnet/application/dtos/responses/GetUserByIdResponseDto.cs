namespace Users.Application.DTOs.Responses;
public sealed class GetUserByIdResponseDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string CountryCode { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
}