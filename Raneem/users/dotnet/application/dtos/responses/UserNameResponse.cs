namespace Users.Application.DTOs.Responses;
public sealed class UserNameResponse
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}