using Arboon.Domain.Entities;

namespace Arboon.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
