namespace API_Custom.Services.Interfaces
{
    public interface ISmsService
    {
        public void SendSms(string toPhoneNumber, string message);
    }
}
