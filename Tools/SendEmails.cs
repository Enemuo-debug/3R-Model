using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using NiRAProject.Dtos;

public class SendEmails
{
    private readonly IConfiguration _configuration;
    public SendEmails(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(SendEmailDto sD)
    {
        try
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_configuration["SuperSecrets:Email"]),
                Subject = "Domain Management Service",
                Body = sD.Message,
                IsBodyHtml = true
            };
            mail.To.Add(sD.Email);

            using var smtp = new SmtpClient(_configuration["SuperSecrets:SmtpHost"], int.Parse(_configuration["SuperSecrets:SmtpPort"]))
            {
                Credentials = new NetworkCredential(_configuration["SuperSecrets:Email"], _configuration["SuperSecrets:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
