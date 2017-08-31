using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace WebUnits.Util
{
    public class Logger
    {
        public static void SendEmail(string subject, string body)
        {
#if DEBUG
            throw new Exception();
#endif
            try
            {
                var fromAddress = new MailAddress("mynotsafeacc@gmail.com", "Not Safe");
                var toAddress = new MailAddress("robin@kock-hamburg.de", "Robin");
                const string fromPassword = "RobinIstToll";

                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                })
                {
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = "WebUnits: " + subject,
                        Body = body + "\n\n" + File.ReadAllText("tmp.json")
                    })
                    {
                        smtp.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static void Log(string message, LogLevel level, string path = "log.txt")
        {
            var header = $"{(char)level} <{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}>: "; //Example: m <2017-08-31 23:01:15>: 
            var lines = message
                .Split(new[] { '\r', '\n' })
                .Where(l => !string.IsNullOrEmpty(l))
                .ToList();

            if (!File.Exists(path))
                File.Create(path);

            File.AppendAllLines(path, lines
                .Take(1)
                .Select(l => header + l));
            File.AppendAllLines(path, lines
                .Skip(1)
                .Select(l => "                         " + l));
        }
    }

    public enum LogLevel
    {
        Verbose = 'v',
        Message = 'm',
        Warning = 'w',
        Error = 'e',
    }
}


