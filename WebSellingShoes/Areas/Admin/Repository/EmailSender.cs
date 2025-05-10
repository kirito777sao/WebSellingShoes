using System.Net;
using System.Net.Mail;

namespace WebSellingShoes.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("duchoan772003@gmail.com", "tiepnatjourpkkzt") // mã này là mã 2 lớp của gmail mật khẩu ứng dụng
            };

            return client.SendMailAsync(
                new MailMessage(from: "duchoan772003@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}

