using System.Net.Mail;
using System.Net;

namespace WebApplication1.HelperClasses
{
    public class EmailSender
    {
        public void SendVerificationEmail(string toEmail, string code)
        {
            //var fromAddress = new MailAddress("politechsender@gmail.com", "PolitechAPP");
            //var toAddress = new MailAddress(toEmail);
            //const string fromPassword = "Politechsender1234..";
            //const string subject = "Էլ. հասցեի հաստատման կոդ";

            //string body = $@"
            //                <html>
            //                    <body>
            //                        <h2>Բարև ձեզ,</h2>
            //                        <p>Ձեր հաստատման կոդն է՝ <strong style='font-size:18px;'>{code}</strong></p>
            //                        <p>Կոդը վավեր է 10 րոպե։</p>
            //                    </body>
            //                </html>";

            //var smtp = new SmtpClient
            //{
            //    Host = "smtp.gmail.com",
            //    Port = 587,
            //    EnableSsl = true,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = false,
            //    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            //};

            //using var message = new MailMessage(fromAddress, toAddress)
            //{
            //    Subject = subject,
            //    Body = body,
            //    IsBodyHtml = true
            //};
            //smtp.Send(message);
            // Ձեր Gmail հասցեն
        private const string FromEmail = "politechsender@gmail.com";
        // Այստեղ պետք է դնեք Google-ից ստացված App Password (16 նիշ)
        private const string AppPassword = "abcdefghijklmnop";

        public void SendVerificationEmail(string toEmail, string code)
        {
            // Պատրաստում ենք հաղորդագրությունը
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("PolitechAPP", FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Էլ. հասցեի հաստատման կոդ";

            message.Body = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                        <body>
                            <h2>Բարև ձեզ,</h2>
                            <p>Ձեր հաստատման կոդն է՝ <strong style='font-size:18px;'>{code}</strong></p>
                            <p>Կոդը վավեր է 10 րոպե։</p>
                        </body>
                    </html>"
            }.ToMessageBody();

            // Կիրառվում է MailKit-ի SMTP հաճախորդը
            using var smtp = new SmtpClient();

            // Միանում ենք Gmail-ի SMTP սերվերին՝ STARTTLS անիվայթմամբ
            smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

            // Ավտենթիկացում՝ օգտագործելով App Password
            smtp.Authenticate(FromEmail, AppPassword);

            // Հաղորդագրության ուղարկում
            smtp.Send(message);

            // Դուրս գալ սերվերից
            smtp.Disconnect(true);
        }
    }
    }
}
