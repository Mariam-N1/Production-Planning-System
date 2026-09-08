using System.Net;
using System.Net.Mail;

namespace BackendProject.Services;

// Sends the password reset code over Gmail.
// SmtpClient is part of .NET, so there is no package to install.
public class EmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendCodeAsync(string to, string name, string code)
    {
        var host = _config["Smtp:Host"];
        var user = _config["Smtp:User"];
        var pass = _config["Smtp:Password"];       // user-secrets, never appsettings

        // Not set up yet? Print it, so the flow still works while you develop.
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            _logger.LogWarning(
                "\n----------------------------------------\n" +
                " Email is not configured.\n" +
                " Reset code for {To} is {Code}\n" +
                "----------------------------------------\n", to, code);
            return;
        }

        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
        var first = string.IsNullOrWhiteSpace(name) ? "there" : name.Split(' ')[0];

        using var message = new MailMessage
        {
            From = new MailAddress(_config["Smtp:From"] ?? user, "Production Planning"),
            Subject = $"{code} is your password reset code",
            IsBodyHtml = true,
            Body = Html(first, code),
        };
        message.To.Add(to);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,                       // Gmail requires STARTTLS on 587
            Credentials = new NetworkCredential(user, pass),
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Reset code emailed to {To}", to);
    }

    // kept plain and inline - email clients strip stylesheets
    private static string Html(string first, string code) => $@"
<div style=""font-family:-apple-system,Segoe UI,Roboto,sans-serif;max-width:440px;margin:0 auto;padding:28px"">
  <h2 style=""margin:0 0 6px;font-size:19px;color:#101828"">Reset your password</h2>
  <p style=""margin:0 0 22px;font-size:14px;line-height:1.6;color:#667085"">
    Hi {WebUtility.HtmlEncode(first)}, use this code to set a new password.
  </p>

  <div style=""padding:18px;border-radius:12px;background:#F1EDFF;text-align:center"">
    <div style=""font-size:32px;font-weight:700;letter-spacing:9px;color:#4E52A6"">{code}</div>
  </div>

  <p style=""margin:22px 0 0;font-size:13px;line-height:1.6;color:#667085"">
    It expires in 15 minutes and can only be used once.<br>
    If you didn't ask to reset your password, you can ignore this email.
  </p>

  <p style=""margin:26px 0 0;font-size:11.5px;color:#98A2B3"">Production Planning</p>
</div>";
}
