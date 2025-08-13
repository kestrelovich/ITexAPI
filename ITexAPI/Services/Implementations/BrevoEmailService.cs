using System.Text;
using System.Text.Json;
using ITexAPI.Models.DTOs;
using ITexAPI.Services.Interfaces;

namespace ITexAPI.Services.Implementations
{
    public class BrevoEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BrevoEmailService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public BrevoEmailService(IConfiguration configuration, ILogger<BrevoEmailService> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendOrderNotificationAsync(OrderDto order)
        {
            Console.WriteLine("=== BREVO EMAIL SERVICE - ORDER NOTIFICATION DEBUG ===");
            Console.WriteLine($"SendOrderNotificationAsync called at: {DateTime.Now}");
            Console.WriteLine($"Order ID: {order.Id}");
            Console.WriteLine($"Order Total: {order.TotalAmount}");
            Console.WriteLine($"Order Items Count: {order.OrderItems?.Count ?? 0}");
            Console.WriteLine($"Customer: {order.CustomerFirstName} {order.CustomerLastName}");
            Console.WriteLine($"Customer Email: {order.CustomerEmail}");

            var adminEmail = _configuration["EmailSettings:AdminEmail"];
            Console.WriteLine($"Admin Email from config: {adminEmail}");

            if (string.IsNullOrEmpty(adminEmail))
            {
                Console.WriteLine("ERROR: Admin email not configured!");
                _logger.LogWarning("Admin email not configured. Skipping order notification.");
                return;
            }

            var subject = $"Nova naračka #{order.Id} - ITex";
            Console.WriteLine($"Email subject: {subject}");

            Console.WriteLine("Generating email body...");
            var body = GenerateOrderEmailBody(order);
            Console.WriteLine($"Email body length: {body.Length} characters");
            Console.WriteLine($"Email body preview (first 200 chars): {body.Substring(0, Math.Min(200, body.Length))}...");

            try
            {
                Console.WriteLine("Attempting to send email via Brevo...");
                await SendEmailAsync(adminEmail, subject, body);
                Console.WriteLine("Email sent successfully!");
                _logger.LogInformation("Order notification sent successfully for order {OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR sending email: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _logger.LogError(ex, "Failed to send order notification for order {OrderId}", order.Id);
                // Don't throw - order should still be created even if email fails
            }

            Console.WriteLine("=== END BREVO EMAIL DEBUG ===");
        }

        public async Task SendCustomerOrderConfirmationAsync(OrderDto order)
        {
            Console.WriteLine("=== BREVO CUSTOMER EMAIL DEBUG ===");
            Console.WriteLine($"SendCustomerOrderConfirmationAsync called at: {DateTime.Now}");
            Console.WriteLine($"Order ID: {order.Id}");
            Console.WriteLine($"Customer Email: {order.CustomerEmail}");

            if (string.IsNullOrEmpty(order.CustomerEmail))
            {
                Console.WriteLine("INFO: Customer email not provided. Skipping customer confirmation email.");
                _logger.LogInformation("Customer email not provided for order {OrderId}. Skipping customer confirmation.", order.Id);
                return;
            }

            var subject = $"Потврда за нарачка #{order.Id} - ITex";
            Console.WriteLine($"Customer email subject: {subject}");

            Console.WriteLine("Generating customer email body...");
            var body = GenerateCustomerOrderConfirmationBody(order);
            Console.WriteLine($"Customer email body length: {body.Length} characters");

            try
            {
                Console.WriteLine("Attempting to send customer confirmation email via Brevo...");
                await SendEmailAsync(order.CustomerEmail, subject, body);
                Console.WriteLine("Customer confirmation email sent successfully!");
                _logger.LogInformation("Customer order confirmation sent successfully for order {OrderId} to {CustomerEmail}", order.Id, order.CustomerEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR sending customer confirmation email: {ex.Message}");
                _logger.LogError(ex, "Failed to send customer order confirmation for order {OrderId} to {CustomerEmail}", order.Id, order.CustomerEmail);
                // Don't throw - order should still be created even if email fails
            }

            Console.WriteLine("=== END CUSTOMER EMAIL DEBUG ===");
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            Console.WriteLine("=== BREVO SEND EMAIL DEBUG ===");
            Console.WriteLine($"SendEmailAsync called at: {DateTime.Now}");
            Console.WriteLine($"To: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Is HTML: {isHtml}");

            try
            {
                var apiKey = _configuration["EmailSettings:BrevoApiKey"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];

                Console.WriteLine($"Brevo API Key configured: {!string.IsNullOrEmpty(apiKey)} (length: {apiKey?.Length ?? 0})");
                Console.WriteLine($"Sender Email: {senderEmail}");
                Console.WriteLine($"Sender Name: {senderName}");

                _logger.LogInformation("Attempting to send email via Brevo to {To}", to);

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderEmail))
                {
                    var missingSettings = new List<string>();
                    if (string.IsNullOrEmpty(apiKey)) missingSettings.Add("BrevoApiKey");
                    if (string.IsNullOrEmpty(senderEmail)) missingSettings.Add("SenderEmail");

                    Console.WriteLine($"ERROR: Missing Brevo settings: {string.Join(", ", missingSettings)}");
                    _logger.LogError("Brevo settings not properly configured. Missing: {MissingSettings}", string.Join(", ", missingSettings));
                    throw new InvalidOperationException($"Brevo settings not properly configured. Missing: {string.Join(", ", missingSettings)}");
                }

                var emailData = new
                {
                    sender = new
                    {
                        name = senderName ?? "ITex Online Shop",
                        email = senderEmail
                    },
                    to = new[]
                    {
                        new
                        {
                            email = to,
                            name = to.Split('@')[0] // Use part before @ as name
                        }
                    },
                    subject = subject,
                    htmlContent = isHtml ? body : $"<p>{body}</p>",
                    textContent = isHtml ? null : body
                };

                var jsonContent = JsonSerializer.Serialize(emailData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                Console.WriteLine("Preparing Brevo API request...");
                Console.WriteLine($"JSON payload length: {jsonContent.Length} characters");
                Console.WriteLine($"JSON payload preview: {jsonContent.Substring(0, Math.Min(300, jsonContent.Length))}...");

                // Create a new HttpClient from factory to avoid disposal issues
                using var httpClient = _httpClientFactory.CreateClient();

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending email via Brevo from {From} to {To} with subject: {Subject}", senderEmail, to, subject);
                Console.WriteLine("Making HTTP POST request to Brevo API...");

                var response = await httpClient.PostAsync("https://api.brevo.com/v3/smtp/email", httpContent);

                Console.WriteLine($"Brevo API response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"SUCCESS: Brevo email sent successfully!");
                    Console.WriteLine($"Response content: {responseContent}");
                    _logger.LogInformation("Email sent successfully via Brevo to {To}. Status: {StatusCode}, Response: {Response}",
                        to, response.StatusCode, responseContent);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"ERROR: Brevo API failed with status {response.StatusCode}");
                    Console.WriteLine($"Error content: {errorContent}");
                    _logger.LogError("Brevo email failed with status {StatusCode}: {ErrorContent}",
                        response.StatusCode, errorContent);
                    throw new InvalidOperationException($"Brevo email failed: {response.StatusCode} - {errorContent}");
                }

                Console.WriteLine("=== END BREVO SEND EMAIL DEBUG ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in Brevo SendEmailAsync: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("=== END BREVO SEND EMAIL DEBUG (WITH ERROR) ===");
                _logger.LogError(ex, "Failed to send email via Brevo to {To}. Error: {ErrorMessage}", to, ex.Message);
                throw new InvalidOperationException($"Failure sending mail via Brevo.", ex);
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