using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Timers;

namespace AdminReminder
{
    public class Program
    {
        static Timer timer;

        static void Main(string[] args)
        {
            timer = new Timer(10 * 60 * 1000); // 10 րոպեն մեկ
            timer.Elapsed += TimerElapsed;
            timer.Start();

            Console.WriteLine("Ծրագիրը ակտիվ է։ Սպասում եմ սեղմել որևէ բան՝ դուրս գալու համար։");
            Console.ReadLine();
        }

        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now;
            if (now.Hour >= 8 && now.Hour < 18)
            {
                CheckAndNotifyAdmins();
            }
        }

        static void CheckAndNotifyAdmins()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            int gradeLimit = int.Parse(ConfigurationManager.AppSettings["GradeLimit"]);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var cmd = new SqlCommand("SELECT COUNT(*) FROM Grades WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)", connection);
                int gradeCount = (int)cmd.ExecuteScalar();

                if (gradeCount >= gradeLimit)
                {
                    var adminCmd = new SqlCommand("SELECT Email FROM Users WHERE Role = 'admin'", connection);
                    var reader = adminCmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string email = reader.GetString(0);
                        SendEmailToAdmin(email);
                    }

                    reader.Close();
                }
            }
        }

        static void SendEmailToAdmin(string email)
        {
            string senderEmail = ConfigurationManager.AppSettings["EmailSender"];
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string smtpUser = ConfigurationManager.AppSettings["SmtpUsername"];
            string smtpPass = ConfigurationManager.AppSettings["SmtpPassword"];

            var mail = new MailMessage(senderEmail, email)
            {
                Subject = "Նոր գնահատականներ",
                Body = "Հարգելի ադմին, գնահատականների թիվը այսօր հասել է նշված շեմին։ Խնդրում ենք ստուգել։"
            };

            var client = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            client.Send(mail);
            Console.WriteLine($"Ուղարկվեց նամակ {email} հասցեին");
        }

    }
}
