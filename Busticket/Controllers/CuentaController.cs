using Busticket.Data;
using Busticket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

public class CuentaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _config;

    public CuentaController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        IConfiguration config)
    {
        _context = context;
        _userManager = userManager;
        _config = config;
    }

    // ==========================
    // OLVIDAR PASSWORD
    // ==========================
    [HttpGet]
    public IActionResult OlvidarPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> OlvidarPassword(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var token = Guid.NewGuid();

                _context.PasswordReset.Add(new PasswordReset
                {
                    UsuarioId = user.Id,
                    Token = token,
                    IdentityToken = identityToken,
                    FechaExpiracion = DateTime.Now.AddMinutes(30)
                });

                await _context.SaveChangesAsync();

                var link = Url.Action(
                    "ResetPassword",
                    "Cuenta",
                    new { token = token },
                    Request.Scheme
                );

                await EnviarCorreoAsync(email, link);
            }

            ViewBag.Mensaje =
                "Si el correo existe, se enviará un enlace de recuperación.";

            return View();
        }
        catch (Exception ex)
        {
            return Content("ERROR REAL: " + ex.Message);
        }
    }


    // ==========================
    // MOSTRAR RESET PASSWORD
    // ==========================
    [HttpGet]
    public IActionResult ResetPassword(Guid token)
    {
        var reset = _context.PasswordReset.FirstOrDefault(x =>
            x.Token == token &&
            !x.Usado &&
            x.FechaExpiracion > DateTime.Now);

        if (reset == null)
            return BadRequest("Token inválido o expirado");

        ViewBag.Token = token;
        return View();
    }

    // ==========================
    // GUARDAR NUEVA PASSWORD
    // ==========================
    [HttpPost]
    public async Task<IActionResult> ResetPassword(
        Guid token,
        string password,
        string confirm)
    {
        if (password != confirm)
        {
            ViewBag.Error = "Las contraseñas no coinciden";
            ViewBag.Token = token;
            return View();
        }

        var reset = _context.PasswordReset.FirstOrDefault(x =>
            x.Token == token &&
            !x.Usado &&
            x.FechaExpiracion > DateTime.Now);

        if (reset == null)
            return BadRequest("Token inválido");

        var user = await _userManager.FindByIdAsync(reset.UsuarioId);

        if (user == null)
            return BadRequest("Usuario no encontrado");

        var result =
            await _userManager.ResetPasswordAsync(
                user,
                reset.IdentityToken ?? await _userManager.GeneratePasswordResetTokenAsync(user),
                password
            );

        if (!result.Succeeded)
        {
            ViewBag.Error = "No se pudo cambiar la contraseña";
            ViewBag.Token = token;
            return View();
        }

        reset.Usado = true;
        await _context.SaveChangesAsync();

        return RedirectToAction("Login", "Cuenta");
    }
    private async Task EnviarCorreoAsync(string destino, string link)
    {
        try
        {
            var mail = new MailMessage();
            mail.From = new MailAddress("busticket.soporte@gmail.com", "Busticket Soporte");
            mail.To.Add(destino);
            mail.Subject = "🔐 Recupera tu contraseña - Busticket";

            // HTML
            mail.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
</head>
<body style='font-family: Arial, sans-serif; background-color:#f3f4f6; padding:20px;'>

    <div style='max-width:600px; margin:auto; background:#ffffff; padding:24px; border-radius:8px;'>

        <h2 style='color:#1f2937; text-align:center;'>Recuperación de contraseña</h2>

        <p style='color:#374151;'>
            Hola,
        </p>

        <p style='color:#374151;'>
            Recibimos una solicitud para restablecer la contraseña de tu cuenta en <b>Busticket</b>.
        </p>

        <p style='text-align:center; margin:30px 0;'>
            <a href='{link}'
               style='background:#2563eb; color:white; padding:12px 24px;
                      border-radius:6px; text-decoration:none; font-weight:bold;'>
                Restablecer contraseña
            </a>
        </p>

        <p style='color:#374151;'>
            Este enlace es válido por <b>30 minutos</b>.
        </p>

        <p style='color:#6b7280; font-size:12px;'>
            Si no solicitaste este cambio, puedes ignorar este correo con seguridad.
        </p>

        <hr style='margin:20px 0;' />

        <p style='color:#9ca3af; font-size:12px; text-align:center;'>
            © {DateTime.Now.Year} Busticket · Soporte
        </p>

    </div>
</body>
</html>
";

            // Indica que el cuerpo es HTML
            mail.IsBodyHtml = true;

            var correoEmisor = _config["Email:Correo"];
            var clave = _config["Email:Clave"];

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(correoEmisor, clave),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
        catch (Exception ex)
        {
            throw new Exception("ERROR AL ENVIAR CORREO: " + ex.Message);
        }
    }


}
