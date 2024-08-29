namespace API_Custom.ViewsModels
{
    public class RegisterConfirmationViewModel
    {
        public string? FullName { get; set; }

        public string? Email { get; set; }
        public required string Code { get; set; }
    }
}
