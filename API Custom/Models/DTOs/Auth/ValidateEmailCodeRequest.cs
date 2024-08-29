namespace API_Custom.Models.DTOs.Auth
{
    public class ValidateEmailCodeRequest
    {
        public required int Code { get; set; }
        public required string Email { get; set; }
    }
}
