using Application.DTOs;
using Application.Results;

namespace Application.Interfaces;

public interface IEmailService
{
    Task<Result> SendAsync(EmailDto email);
    Task<Result> SendWelcomeEmailAsync(string toEmail, string userName);
    Task<Result> SendPasswordChangedEmailAsync(string toEmail, string userName);
}