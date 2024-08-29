using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace API_Custom.Models
{
    [MetadataType(typeof(AppUserMetadata))]
    public class User : IdentityUser<Guid>
    {
        public string? PhoneNumber { get; set; }
        public int? PhoneCode { get; set; }
        public int? EmailCode { get; set; }
        public bool? IsGoogleAuth { get; set; }
    }

    internal class AppUserMetadata
    {
        [Key]
        public Guid Id { get; set; }
    }
}
    