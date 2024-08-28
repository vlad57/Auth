namespace API_Custom.Models.DTOs.ErrorHandling
{
    public class NotFoundError
    {
        public int Status { get; set; }
        public string Title { get; set; }
        public string? Detail { get; set; }
    }

    public class BadRequestError
    {
        public int Status { get; set; }
        public string Errors { get; set; }
    }
}
