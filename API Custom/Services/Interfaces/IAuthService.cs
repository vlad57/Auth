using API_Custom.Models;
using API_Custom.Models.DTOs.Auth;
using System.IdentityModel.Tokens.Jwt;

namespace API_Custom.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<JwtSecurityToken> GenerateTokenAsync(User user);
        public Task<JwtSecurityToken> GenerateTokenFromGoogleValidationAsync(LoginGoogleAuthRequest request);
    }
}
