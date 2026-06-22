using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Busticket.Data;
using Busticket.Models;
using Busticket.DTOs;
namespace Busticket.Controllers
{
    public class PerfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PerfilController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // 🔐 Usuario logueado
            var user = await _userManager.GetUserAsync(User);

            var cliente = await _context.Cliente
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            var model = new PerfilViewModel
            {
                Nombre = cliente?.Nombre ?? "",
                Documento = cliente?.Documento ?? "",
                Telefono = cliente?.Telefono ?? "",
                Correo = user.Email!,
            };

            // 🚌 Historial de viajes (últimos 5)
            var viajes = await _context.Boleto
                .Include(b => b.Ruta)
                    .ThenInclude(r => r.CiudadOrigen)
                .Include(b => b.Ruta)
                    .ThenInclude(r => r.CiudadDestino)
                .Where(b => b.UserId == user.Id)
               
                .Take(5)
                .Select(b => new ViajeViewModel
                {
                    Origen  = b.Ruta.CiudadOrigen.Nombre,
                    Destino = b.Ruta.CiudadDestino.Nombre,
                    Fecha   = b.Ruta.FechaSalida,
                })
                .ToListAsync();

            model.HistorialViajes = viajes;

            return View(model);
        }
    }
}