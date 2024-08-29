using System.ComponentModel.DataAnnotations;

namespace API_Custom.Models.DTOs.Auth
{
    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
