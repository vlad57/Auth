using API_Custom.Models;

namespace API_Custom.Services.Interfaces
{
    public interface IUserService
    {
        public Task<User?> FindByPhone(string phoneNumber);
    }
}
