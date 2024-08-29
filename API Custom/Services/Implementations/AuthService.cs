using API_Custom.Models;
using API_Custom.Models.DTOs.Auth;
using API_Custom.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Custom.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;

        private readonly IConfiguration _configuration;
        private readonly DatabaseContext _databaseContext;

        public AuthService(UserManager<User> userManager, IConfiguration configuration, DatabaseContext databaseContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _databaseContext = databaseContext;
        }

        public async Task<JwtSecurityToken> GenerateTokenAsync(User user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            if (user.UserName != null)
            {
                authClaims.Add(new Claim(ClaimTypes.Name, user.UserName));
            }

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }

        public async Task<JwtSecurityToken> GenerateTokenFromGoogleValidationAsync(LoginGoogleAuthRequest request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _configuration["GoogleAPI:ClientID"] }
                };

                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, settings);

                if (payload != null)
                {
                    var email = payload.Email;

                    var user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        var newUser = new User
                        {
                            Email = email,
                            EmailConfirmed = true,
                            IsGoogleAuth = true
                        };

                        await _databaseContext.AddAsync(newUser);
                        await _databaseContext.SaveChangesAsync();

                        user = newUser;
                    }

                    if (user != null && user.IsGoogleAuth == false)
                    {
                        throw new Exception("User with provided email already exists.");
                    }

                    var token = await GenerateTokenAsync(user!);

                    return token;

                }

                throw new Exception("Payload not valid.");
            }
            catch (InvalidJwtException ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
