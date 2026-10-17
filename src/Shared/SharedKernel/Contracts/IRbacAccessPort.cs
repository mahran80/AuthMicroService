using SharedKernel.Application;

namespace SharedKernel.Contracts;

public interface IRbacAccessPort
{
    Task<UserAccessProfileDto> GetUserAccessProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result> AssignDefaultRoleAsync(Guid userId, string defaultRoleName, CancellationToken cancellationToken = default);
}
