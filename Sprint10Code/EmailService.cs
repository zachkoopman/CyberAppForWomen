using System.Net;
using System.Net.Mail;

namespace CyberApp_FIA.Services
{
    public static class EmailService
    {
        public static void SendEmail(string to, string subject, string body)
        {
            var message = new MailMessage();
            message.To.Add(to);
            message.Subject = subject;
            message.Body = body;
            message.From = new MailAddress("no-reply@fia.org");

            var client = new SmtpClient("smtp.yourprovider.com")
            {
                Credentials = new NetworkCredential("username", "password"),
                EnableSsl = true
            };

            client.Send(message);
        }
    }
}