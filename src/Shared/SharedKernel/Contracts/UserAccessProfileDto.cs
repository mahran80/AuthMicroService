namespace SharedKernel.Contracts;

public sealed record UserAccessProfileDto(
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
