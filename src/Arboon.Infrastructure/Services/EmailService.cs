using Arboon.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MimeKit;

namespace Arboon.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly bool _isDevelopment;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }

    public async Task SendMagicLinkAsync(string buyerEmail, string escrowId, Guid buyerToken, string escrowTitle)
    {
        var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://arboon.app";
        var magicLink = $"{frontendUrl}/release/{escrowId}?token={buyerToken}";

        // In development, log to console instead of sending email
        if (_isDevelopment)
        {
            _logger.LogInformation(
                "📧 [DEV] Magic Link Email\n" +
                "   To: {BuyerEmail}\n" +
                "   Escrow: {EscrowId}\n" +
                "   Title: {EscrowTitle}\n" +
                "   Link: {MagicLink}",
                buyerEmail, escrowId, escrowTitle, magicLink);
            return;
        }

        // Production: send via SMTP
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
        var port = int.Parse(emailSettings["Port"] ?? "587");
        var username = emailSettings["Username"] ?? "";
        var password = emailSettings["Password"] ?? "";
        var fromEmail = emailSettings["FromEmail"] ?? "noreply@arboon.app";
        var fromName = emailSettings["FromName"] ?? "عَرْبُون";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress("", buyerEmail));
        message.Subject = "رابط تأكيد الاستلام — عَرْبُون";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; direction: rtl; text-align: right; background-color: #f5f5f5; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 12px; padding: 40px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .logo {{ text-align: center; font-size: 28px; font-weight: bold; color: #2563eb; margin-bottom: 30px; }}
        .title {{ font-size: 20px; color: #1f2937; margin-bottom: 15px; }}
        .escrow-info {{ background: #f0f9ff; border-radius: 8px; padding: 20px; margin: 20px 0; border-right: 4px solid #2563eb; }}
        .btn {{ display: inline-block; background: #2563eb; color: white !important; text-decoration: none; padding: 14px 40px; border-radius: 8px; font-size: 16px; font-weight: bold; margin: 25px 0; }}
        .footer {{ color: #6b7280; font-size: 13px; margin-top: 30px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='logo'>عَرْبُون</div>
        <h2 class='title'>مرحباً، تم استلام الدفعة بنجاح! 🎉</h2>
        <p>لقد تم تجميد المبلغ المدفوع في عُهدة آمنة. عند استلامك للخدمة أو المنتج، يرجى تأكيد الاستلام عبر الزر أدناه:</p>
        
        <div class='escrow-info'>
            <strong>عنوان العُهدة:</strong> {escrowTitle}<br/>
            <strong>رقم العُهدة:</strong> {escrowId}
        </div>
        
        <div style='text-align: center;'>
            <a href='{magicLink}' class='btn'>✅ تأكيد الاستلام وتحرير المبلغ</a>
        </div>
        
        <p style='color: #6b7280; font-size: 14px;'>⚠️ لا تشارك هذا الرابط مع أي شخص آخر. هذا الرابط مخصص لك فقط لتأكيد استلام الخدمة.</p>
        
        <div class='footer'>
            <p>عَرْبُون — منصة الضمان المالي للعمل الحر</p>
        </div>
    </div>
</body>
</html>"
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Magic link email sent to {BuyerEmail} for escrow {EscrowId}", buyerEmail, escrowId);
    }
}
