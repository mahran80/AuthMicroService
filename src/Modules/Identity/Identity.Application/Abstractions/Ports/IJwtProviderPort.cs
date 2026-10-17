using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Abstractions.Ports;

public interface IJwtProviderPort
{
    TokenPairResult GenerateTokenPair(User user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions);
}
