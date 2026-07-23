using Busticket.Data;
using Busticket.Models;
using Busticket.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Busticket.DTOs;
namespace Busticket.Controllers
{
    [Authorize(Roles = "Empresa,Admin")]
    public class PanelEmpresaController : Controller


    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PanelEmpresaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // DASHBOARD
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var empresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (empresa == null)
            {
                TempData["ErrorMessage"] = "No tienes una empresa registrada.";
                return RedirectToAction("Index", "Home");
            }

            var ventas = await _context.Venta
                .Where(v => v.EmpresaId == empresa.EmpresaId)
                .Include(v => v.User)
                .Include(v => v.Empresa)
                .ToListAsync();

            return View(new PanelEmpresaVM
            {
                Empresa = empresa,
                Ventas = ventas
            });
        }

        // LISTAR RUTAS
        public async Task<IActionResult> Rutas()
        {
            var userId = _userManager.GetUserId(User);

            var empresa = await _context.Empresa
      .FirstOrDefaultAsync(e => e.UserId == userId);

            if (empresa == null)
            {
                TempData["ErrorMessage"] = "No existe una empresa asociada a este usuario.";
                return RedirectToAction("Index", "Home");
            }
            var rutas = await _context.Ruta
                .Where(r => r.EmpresaId == empresa.EmpresaId)
                .Include(r => r.CiudadOrigen)
                .Include(r => r.CiudadDestino)
                .ToListAsync();

            return View(rutas);
        }

        // CREAR RUTA GET
        public IActionResult CrearRuta()
        {
            ViewBag.Ciudades = _context.Ciudad.ToList();
            return View(new Ruta { FechaSalida = DateTime.Now, FechaLlegada = DateTime.Now.AddHours(1) });
        }
        [HttpGet]
        public async Task<IActionResult> EditarRuta(int id)
        {
            var ruta = await _context.Ruta.FindAsync(id);

            if (ruta == null)
                return NotFound();

            ViewBag.Ciudades = _context.Ciudad.ToList();
            ViewBag.Empresas = _context.Empresa.ToList();

            return View(ruta);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRuta(Ruta ruta)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Ciudades = _context.Ciudad.ToList();
                ViewBag.Empresas = _context.Empresa.ToList();
                return View(ruta);
            }

            var existingRuta = await _context.Ruta.FindAsync(ruta.RutaId);
            if (existingRuta == null)
                return NotFound();

            existingRuta.CiudadOrigenId = ruta.CiudadOrigenId;
            existingRuta.CiudadDestinoId = ruta.CiudadDestinoId;
            existingRuta.Precio = ruta.Precio;
            existingRuta.DuracionMin = ruta.DuracionMin;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ruta actualizada correctamente";

            return RedirectToAction("Rutas");
        }


        // GET
        [HttpGet]
        public async Task<IActionResult> EliminarRuta(int id)
        {
            var ruta = await _context.Ruta
                .Include(r => r.CiudadOrigen)
                .Include(r => r.CiudadDestino)
                .FirstOrDefaultAsync(r => r.RutaId == id);

            if (ruta == null)
                return NotFound();

            return View(ruta);
        }

        // POST

        [HttpPost, ActionName("EliminarRuta")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminarRuta(int RutaId)
        {
            var ruta = await _context.Ruta.FindAsync(RutaId);

            if (ruta == null)
                return RedirectToAction("Rutas");

            // 🔥 ELIMINAR ASIENTOS PRIMERO
            var asientos = _context.Asiento.Where(a => a.RutaId == RutaId);
            _context.Asiento.RemoveRange(asientos);

            // Luego eliminar la ruta
            _context.Ruta.Remove(ruta);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ruta eliminada correctamente";

            return RedirectToAction("Rutas");
        }
        // CREAR RUTA POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearRuta(Ruta ruta)
        {
            ModelState.Remove("EmpresaId");

            if (ruta.CiudadOrigenId == ruta.CiudadDestinoId)
                ModelState.AddModelError("", "Origen y destino no pueden ser iguales");

            if (!ModelState.IsValid)
            {
                ViewBag.Ciudades = _context.Ciudad.ToList();
                return View(ruta);
            }

            var userId = _userManager.GetUserId(User);

            var empresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (empresa == null)
            {
                TempData["ErrorMessage"] = "No tienes una empresa registrada.";
                return RedirectToAction("Index", "Home");
            }

            ruta.EmpresaId = empresa.EmpresaId;
            ruta.TipoBus ??= "Normal";
            ruta.NumAsientos ??= 20;

            _context.Ruta.Add(ruta);
            await _context.SaveChangesAsync();

            var asientos = Enumerable.Range(1, ruta.NumAsientos.Value).Select(i => new Asiento
            {
                Numero = i,
                Codigo = i.ToString(),
                RutaId = ruta.RutaId,
                Disponible = true
            });

            _context.Asiento.AddRange(asientos);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Ruta creada con {ruta.NumAsientos} asientos ({ruta.TipoBus}).";
            return RedirectToAction(nameof(Rutas));
        }
    }
}