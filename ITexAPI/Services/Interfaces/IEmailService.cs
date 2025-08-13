using ITexAPI.Models.DTOs;

namespace ITexAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOrderNotificationAsync(OrderDto order);
        Task SendCustomerOrderConfirmationAsync(OrderDto order);
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}