using API_Custom.Services.Interfaces;

namespace API_Custom.Services.Implementations
{
    public class UtilsService : IUtilsService
    {
        public int GenerateCode()
        {
            Random random = new Random();
            int code = random.Next(1000, 10000);

            return code;
        }
    }
}
