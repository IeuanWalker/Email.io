using EmailApi.Models;

namespace EmailApi.Services;

public interface IEmailService
{
	void SendEmail(EmailModel request);
}
