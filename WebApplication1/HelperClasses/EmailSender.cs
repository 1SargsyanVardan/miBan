using System.Net.Mail;
using System.Net;

namespace WebApplication1.HelperClasses
{
    public class EmailSender
    {
        public void SendVerificationEmail(string toEmail, string code)
        {
            var fromAddress = new MailAddress("politechsender@gmail.com", "PolitechAPP");
            var toAddress = new MailAddress(toEmail);
            const string fromPassword = "kikbtqtxmykzdnwe";
            const string subject = "Էլ. հասցեի հաստատման կոդ";

            string body = $@"
                <html>
                    <body>
                        <h2>Բարև ձեզ,</h2>
                        <p>Ձեր հաստատման կոդն է՝ 
                           <strong style='font-size:18px;'>{code}</strong>
                        </p>
                        <p>Կոդը վավեր է 10 րոպե։</p>
                    </body>
                </html>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            smtp.Send(message);
        }

    }
}
