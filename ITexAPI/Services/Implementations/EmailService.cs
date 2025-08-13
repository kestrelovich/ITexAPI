using ITexAPI.Models.DTOs;
using ITexAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ITexAPI.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOrderNotificationAsync(OrderDto order)
        {
            var adminEmail = _configuration["EmailSettings:AdminEmail"];
            if (string.IsNullOrEmpty(adminEmail))
            {
                _logger.LogWarning("Admin email not configured. Skipping order notification.");
                return;
            }

            var subject = $"Nova naračka #{order.Id} - ITex";
            var body = GenerateOrderEmailBody(order);

            try
            {
                await SendEmailAsync(adminEmail, subject, body);
                _logger.LogInformation("Order notification sent successfully for order {OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order notification for order {OrderId}", order.Id);
                // Don't throw - order should still be created even if email fails
            }
        }

        public async Task SendCustomerOrderConfirmationAsync(OrderDto order)
        {
            if (string.IsNullOrEmpty(order.CustomerEmail))
            {
                _logger.LogInformation("Customer email not provided for order {OrderId}. Skipping customer confirmation.", order.Id);
                return;
            }

            var subject = $"Потврда за нарачка #{order.Id} - ITex";
            var body = GenerateCustomerOrderConfirmationBody(order);

            try
            {
                await SendEmailAsync(order.CustomerEmail, subject, body);
                _logger.LogInformation("Customer order confirmation sent successfully for order {OrderId} to {CustomerEmail}", order.Id, order.CustomerEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send customer order confirmation for order {OrderId} to {CustomerEmail}", order.Id, order.CustomerEmail);
                // Don't throw - order should still be created even if email fails
            }
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings");

                var smtpServer = smtpSettings["SmtpServer"];
                var smtpPortString = smtpSettings["SmtpPort"] ?? "587";
                var senderEmail = smtpSettings["SenderEmail"];
                var senderPassword = smtpSettings["SenderPassword"];

                _logger.LogInformation("Attempting to send email to {To} via {SmtpServer}:{SmtpPort}", to, smtpServer, smtpPortString);

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    var missingSettings = new List<string>();
                    if (string.IsNullOrEmpty(smtpServer)) missingSettings.Add("SmtpServer");
                    if (string.IsNullOrEmpty(senderEmail)) missingSettings.Add("SenderEmail");
                    if (string.IsNullOrEmpty(senderPassword)) missingSettings.Add("SenderPassword");

                    _logger.LogError("Email settings not properly configured. Missing: {MissingSettings}", string.Join(", ", missingSettings));
                    throw new InvalidOperationException($"Email settings not properly configured. Missing: {string.Join(", ", missingSettings)}");
                }

                var smtpPort = int.Parse(smtpPortString);

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    Timeout = 30000 // 30 seconds timeout
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, "ITex Online Shop"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(to);

                _logger.LogInformation("Sending email from {From} to {To} with subject: {Subject}", senderEmail, to, subject);

                await client.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}. Error: {ErrorMessage}", to, ex.Message);
                throw new InvalidOperationException($"Failure sending mail.", ex);
            }
        }

        private string GenerateOrderEmailBody(OrderDto order)
        {
            var itemsHtml = string.Join("", order.OrderItems.Select(item =>
                $"<tr><td>{item.ProductName}</td><td>{item.Quantity}</td><td>{item.UnitPrice:C}</td><td>{item.TotalPrice:C}</td></tr>"));

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Nova Naračka - ITex</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .header {{ background: #007bff; color: white; padding: 20px; border-radius: 5px; text-align: center; margin-bottom: 30px; }}
        .info-section {{ margin-bottom: 25px; }}
        .info-title {{ font-weight: bold; color: #333; font-size: 16px; margin-bottom: 10px; border-bottom: 2px solid #007bff; padding-bottom: 5px; }}
        .info-item {{ margin-bottom: 8px; }}
        .label {{ font-weight: bold; color: #555; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 15px; }}
        th, td {{ border: 1px solid #ddd; padding: 10px; text-align: left; }}
        th {{ background-color: #f8f9fa; font-weight: bold; }}
        .total {{ font-size: 18px; font-weight: bold; color: #007bff; text-align: right; margin-top: 15px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>НОВА НАРАЧКА - ITex</h1>
            <p>Нарачка број: #{order.Id}</p>
        </div>

        <div class='info-section'>
            <div class='info-title'>Информации за нарачката</div>
            <div class='info-item'><span class='label'>Датум:</span> {order.OrderDate:dd.MM.yyyy HH:mm}</div>
            <div class='info-item'><span class='label'>Статус:</span> {order.Status}</div>
            <div class='info-item'><span class='label'>Вкупно:</span> {order.TotalAmount:C}</div>
        </div>

        <div class='info-section'>
            <div class='info-title'>Информации за купувачот</div>
            <div class='info-item'><span class='label'>Име:</span> {order.CustomerFirstName} {order.CustomerLastName}</div>
            <div class='info-item'><span class='label'>Email:</span> {order.CustomerEmail}</div>
            <div class='info-item'><span class='label'>Телефон:</span> {order.CustomerPhone}</div>
            <div class='info-item'><span class='label'>Адреса за достава:</span> {order.ShippingAddress}</div>
            {(string.IsNullOrEmpty(order.Notes) ? "" : $"<div class='info-item'><span class='label'>Забелешки:</span> {order.Notes}</div>")}
        </div>

        <div class='info-section'>
            <div class='info-title'>Нарачани производи</div>
            <table>
                <thead>
                    <tr>
                        <th>Производ</th>
                        <th>Количина</th>
                        <th>Цена</th>
                        <th>Вкупно</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsHtml}
                </tbody>
            </table>
            <div class='total'>ВКУПНО: {order.TotalAmount:C}</div>
        </div>

        <div class='footer'>
            <p><strong>ITex</strong> - Онлајн продавница за текстил</p>
            <p>Оваа нарачка е автоматски генерирана од системот.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateCustomerOrderConfirmationBody(OrderDto order)
        {
            // Calculate total savings (for now we'll show 0 as we don't have discount info)
            var totalSavings = 0m;

            var itemsHtml = string.Join("", order.OrderItems.Select(item =>
                $@"<tr>
                    <td>{item.ProductName}</td>
                    <td>{item.ProductName}<br/><small class='text-muted'>ITex</small></td>
                    <td>{item.UnitPrice:N0}</td>
                    <td>0%</td>
                    <td>{item.UnitPrice:N0}<br/><small>МКД</small></td>
                    <td>{item.Quantity:N2}</td>
                    <td>{item.TotalPrice:N0}<br/><small>МКД</small></td>
                </tr>"));

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Потврда за нарачка - ITex</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f8f9fa; color: #333; }}
        .container {{ max-width: 700px; margin: 0 auto; background: white; }}
        .header {{ background: #007bff; color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .section {{ margin-bottom: 30px; }}
        .section-title {{ font-size: 18px; font-weight: bold; color: #007bff; margin-bottom: 15px; padding-bottom: 8px; border-bottom: 2px solid #007bff; }}
        .info-box {{ background: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .order-details {{ background: #e3f2fd; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .address-box {{ background: #fff3cd; padding: 15px; border-radius: 5px; border-left: 4px solid #ffc107; }}
        table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
        th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
        th {{ background-color: #f8f9fa; font-weight: bold; }}
        .totals {{ background: #f8f9fa; padding: 15px; margin: 15px 0; }}
        .total-row {{ display: flex; justify-content: space-between; margin: 8px 0; }}
        .final-total {{ font-size: 18px; font-weight: bold; color: #007bff; }}
        .footer {{ background: #343a40; color: white; padding: 30px; text-align: center; }}
        .social-links {{ margin: 15px 0; }}
        .social-links a {{ color: #007bff; margin: 0 10px; text-decoration: none; }}
        .contact-info {{ margin: 15px 0; font-size: 14px; }}
        .small {{ font-size: 12px; color: #666; }}
        .warning {{ background: #fff3e0; padding: 15px; border-left: 4px solid #ff9800; margin: 15px 0; }}
        .benefits {{ background: #e8f5e8; padding: 15px; border-left: 4px solid #4caf50; margin: 15px 0; }}
        .benefit-item {{ margin: 5px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Ви благодариме на довербата!</h1>
            <p>Вашата нарачка е евидентирана во нашиот систем</p>
        </div>

        <div class='content'>
            <div class='section'>
                <p>Почитувани <strong>{order.CustomerFirstName} {order.CustomerLastName}</strong>,</p>
                <p>Ви благодариме на довербата!</p>
                <p>Вашата нарачка е евидентирана во нашиот систем.</p>
            </div>

            <div class='warning'>
                <p>Ве известуваме дека по проверка на достапноста и исправноста на производите кои ги имате нарачано, во случај на неисправност или недостапност на нарачан производ го задржуваме правото да ја откажеме нарачката на некој од производите од нарачката поради неможност да се реализира нарачката, за што дополнително ќе бидете известени.</p>
                <p>Доколку нарачката е платена со платежна картичка, истата за смета за креирана тогаш кога ќе добиете потврден мејл за успешно извршено плаќање.</p>
                <p>Во период на зголемен обем на нарачки, можно е да се случи доставата на Вашата нарачка да го надмине утврдениот рок за достава од 3-5 работни дена.</p>
                <p><strong>Ви благодариме на разбирањето.</strong></p>
            </div>

            <div class='benefits'>
                <div class='section-title'>Напомени:</div>
                <div class='benefit-item'>✓ Право на повлекување/враќање на производ</div>
                <div class='benefit-item'>✓ Замена на производи</div>
                <div class='benefit-item'>✓ Рекламации</div>
                <div class='benefit-item'>✓ Вашето мислење ни значи - оценете го Вашето онлајн искуство</div>
            </div>

            <div class='order-details'>
                <div class='section-title'>Детали за нарачката</div>
                <div style='display: flex; justify-content: space-between;'>
                    <div><strong>Број на нарачка:</strong></div>
                    <div><strong>#{order.Id}</strong></div>
                </div>
                <div style='display: flex; justify-content: space-between;'>
                    <div><strong>Датум:</strong></div>
                    <div>{order.OrderDate:dd.MM.yyyy HH:mm}</div>
                </div>
            </div>

            <div class='address-box'>
                <div class='section-title'>Адреса за достава</div>
                <strong>{order.CustomerFirstName} {order.CustomerLastName}</strong><br/>
                {order.ShippingAddress}<br/>
                Македонија<br/>
                <strong>Телефон:</strong> {order.CustomerPhone}
                {(!string.IsNullOrEmpty(order.CustomerEmail) ? $"<br/><strong>Email:</strong> {order.CustomerEmail}" : "")}
            </div>

            <div class='info-box'>
                <div style='display: flex; justify-content: space-between;'>
                    <div><strong>Начин на достава:</strong></div>
                    <div>Курирска служба</div>
                </div>
                <div style='display: flex; justify-content: space-between;'>
                    <div><strong>Начин на плаќање:</strong></div>
                    <div>Плаќање по испорака</div>
                </div>
                <div style='display: flex; justify-content: space-between;'>
                    <div><strong>Коментар:</strong></div>
                    <div>{(string.IsNullOrEmpty(order.Notes) ? "-" : order.Notes)}</div>
                </div>
            </div>

            <div class='section'>
                <div class='section-title'>Нарачани производи</div>
                <table>
                    <thead>
                        <tr>
                            <th>Производ</th>
                            <th>Опис</th>
                            <th>Цена</th>
                            <th>Попуст</th>
                            <th>Цена со попуст</th>
                            <th>Количина</th>
                            <th>Вкупно</th>
                        </tr>
                    </thead>
                    <tbody>
                        {itemsHtml}
                    </tbody>
                </table>
            </div>

            <div class='totals'>
                <div class='total-row'>
                    <span><strong>Вкупно:</strong></span>
                    <span><strong>{order.TotalAmount:N0} МКД</strong></span>
                </div>
                <div class='total-row'>
                    <span>Заштеда:</span>
                    <span>{totalSavings:N0} МКД</span>
                </div>
                <div class='total-row'>
                    <span>Трошок за достава:</span>
                    <span>Превозот е бесплатен</span>
                </div>
                <div class='total-row final-total'>
                    <span>Вкупно за плаќање со ДДВ:</span>
                    <span>{order.TotalAmount:N0} МКД</span>
                </div>
            </div>
        </div>

        <div class='footer'>
            <div class='section-title' style='color: white; border-color: white;'>СЛЕДИ НÉ</div>
            <div class='social-links'>
                <a href='#'>Facebook</a> |
                <a href='#'>Instagram</a> |
                <a href='#'>YouTube</a>
            </div>
            
            <div class='contact-info'>
                <div style='margin: 20px 0;'>
                    <div><strong>Контакт</strong></div>
                    <div><strong>Помош при купување</strong></div>
                    <div><strong>За нас</strong></div>
                </div>
                
                <div style='margin-top: 30px;'>
                    <div><strong>ITex - Онлајн продавница за текстил</strong></div>
                    <div>ул. Никола Кљусев бр.6, кат 7, 1000, Скопје</div>
                    <div>071 297 676, 070 275 363</div>
                    <div>@2024 www.itex.com.mk</div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}