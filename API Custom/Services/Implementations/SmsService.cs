using API_Custom.Services.Interfaces;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace API_Custom.Services.Implementations
{
    public class SmsService : ISmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;

        public SmsService(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _fromPhoneNumber = configuration["Twilio:FromPhoneNumber"];
        }

        public void SendSms(string toPhoneNumber, string message)
        {
            Twilio.TwilioClient.Init(_accountSid, _authToken);

            var messageOptions = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
            {
                From = new PhoneNumber(_fromPhoneNumber),
                Body = message
            };

            try
            {
                MessageResource.Create(messageOptions);
            } catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
