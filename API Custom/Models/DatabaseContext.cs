using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace API_Custom.Models
{
    public class DatabaseContext : IdentityDbContext<User, IdentityRole<Guid>, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(builder =>
            {
                builder.ToTable("Users");
            });

            builder.Entity<IdentityRole<Guid>>(builder => { builder.ToTable(name: "Auth_Roles"); });
            builder.Entity<UserRole>(builder => { builder.ToTable("Auth_UserRoles"); });
            builder.Entity<IdentityUserClaim<Guid>>(builder => { builder.ToTable("Auth_UserClaims"); });
            builder.Entity<IdentityUserLogin<Guid>>(builder => { builder.ToTable("Auth_UserLogins"); });
            builder.Entity<IdentityUserToken<Guid>>(builder => { builder.ToTable("Auth_UserTokens"); });
            builder.Entity<IdentityRoleClaim<Guid>>(builder => { builder.ToTable("Auth_RoleClaims"); });
        }
    }
}
