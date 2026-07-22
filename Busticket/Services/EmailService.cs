using System.Net;
using System.Net.Mail;

namespace Busticket.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarBoletoAsync(
            string destinatario,
            string asunto,
            string cuerpoHtml,
            byte[] pdf,
            string nombrePdf)
        {
            var correoEmisor = _config["Email:Correo"] ?? "soporte@busticket.com";
            var clave = _config["Email:Clave"] ?? "";

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(correoEmisor, clave)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(correoEmisor, "Busticket"),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mail.To.Add(destinatario);

            // 📎 Adjuntar PDF
            mail.Attachments.Add(
                new Attachment(
                    new MemoryStream(pdf),
                    nombrePdf,
                    "application/pdf"
                )
            );

            await smtp.SendMailAsync(mail);
        }
    }
}