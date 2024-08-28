#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace API_Custom.Models
{
    [MetadataType(typeof(UserRoleMetadata))]
    public class UserRole : IdentityUserRole<Guid>
    {
        public IdentityRole<Guid> Role;
    }

    internal class UserRoleMetadata
    {
        [Key]
        public Guid UserId { get; set; }
        [Key]
        public Guid RoleId { get; set; }
    }
}
