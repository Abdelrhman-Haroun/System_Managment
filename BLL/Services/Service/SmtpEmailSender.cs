using BLL.Services.IService;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BLL.Services.Service
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly string _host;
        private readonly int _port;
        private readonly bool _enableSSL;
        private readonly string _userName;
        private readonly string _password;
        private readonly string _senderName;

        public SmtpEmailSender(string host, int port, bool enableSSL, string userName, string password, string senderName)
        {
            _host = host;
            _port = port;
            _enableSSL = enableSSL;
            _userName = userName;
            _password = password;
            _senderName = senderName;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_userName, _password),
                EnableSsl = _enableSSL
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_userName, _senderName),
                Subject = subject,
                Body = $"<div style='font-family: Arial; text-align: center;'>" +
                       $"<img src=\"~/files/images/logo.jpg\" alt='AlSalem Group' style='width: 150px;' />" +
                       $"<h3>{htmlMessage}</h3>" +
                       $"<footer>Contact: info@alsalemgroup.com/footer></div>",
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}