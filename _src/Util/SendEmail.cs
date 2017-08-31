using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace WebUnitsApiRipper.Util
{
    public class SendEmail
    {
        public static void SendNote(string subject, string body)
        {
#if DEBUG
            throw new Exception();
#endif
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
    }
}


