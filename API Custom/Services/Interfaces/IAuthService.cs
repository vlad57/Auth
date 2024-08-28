using API_Custom.Models;
using System.IdentityModel.Tokens.Jwt;

namespace API_Custom.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<JwtSecurityToken> GenerateTokenAsync(User user);
    }
}
