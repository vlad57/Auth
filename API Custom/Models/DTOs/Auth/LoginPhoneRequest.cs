namespace API_Custom.Models.DTOs.Auth
{
    public class LoginPhoneRequest
    {
        public required string PhoneNumber { get; set; }
        public required string Code { get; set; }
    }
}
