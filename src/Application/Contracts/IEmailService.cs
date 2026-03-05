using Application.DTOs.Auth;

namespace Application.Contracts;

public interface IEmailService
{
    Task SendEmailAsync(MailRequest mailRequest);
}
