using AutoConfig.Core.Entities;

namespace AutoConfig.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
