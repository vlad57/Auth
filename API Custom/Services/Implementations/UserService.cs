using API_Custom.Models;
using API_Custom.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API_Custom.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly DatabaseContext _databaseContext;
        public UserService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<User?> FindByPhone(string phoneNumber)
        {
            var user = await _databaseContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            return user;
        }
    }
}
