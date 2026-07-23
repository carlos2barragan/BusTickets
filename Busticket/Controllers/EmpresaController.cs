using Busticket.Data;
using Busticket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Busticket.Controllers
{
    [Authorize(Roles = "Empresa,Admin")]
    public class EmpresaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmpresaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // LISTAR (SOLO LA EMPRESA DEL USUARIO)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresas = await _context.Empresa
                .Where(e => e.UserId == userId)
                .ToListAsync();

            return View(empresas);
        }

        // ===============================
        // CREAR
        // ===============================
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Empresa());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Empresa empresa)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (!ModelState.IsValid)
                return View(empresa);

            empresa.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            empresa.FechaRegistro = DateTime.Now;

            _context.Empresa.Add(empresa);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // ===============================
        // EDITAR
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.EmpresaId == id && e.UserId == userId);

            if (empresa == null)
                return NotFound();

            return View(empresa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Empresa empresa)
        {
            if (!ModelState.IsValid)
                return View(empresa);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresaDb = await _context.Empresa
                .FirstOrDefaultAsync(e => e.EmpresaId == empresa.EmpresaId && e.UserId == userId);

            if (empresaDb == null)
                return NotFound();

            empresaDb.Nombre = empresa.Nombre;
            empresaDb.Nit = empresa.Nit;
            empresaDb.Email = empresa.Email;
            empresaDb.Pais = empresa.Pais;
            empresaDb.Telefono = empresa.Telefono;

            _context.Empresa.Update(empresaDb);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // ELIMINAR
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.EmpresaId == id && e.UserId == userId);

            if (empresa == null)
                return NotFound();

            return View(empresa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresa = await _context.Empresa
                .FirstOrDefaultAsync(e => e.EmpresaId == id && e.UserId == userId);

            if (empresa == null)
                return NotFound();

            var rutas = await _context.Ruta.Where(r => r.EmpresaId == id).ToListAsync();
            foreach (var ruta in rutas)
            {
                var asientos = _context.Asiento.Where(a => a.RutaId == ruta.RutaId);
                _context.Asiento.RemoveRange(asientos);

                var boletos = _context.Boleto.Where(b => b.RutaId == ruta.RutaId);
                _context.Boleto.RemoveRange(boletos);

                var ventas = _context.Venta.Where(v => v.RutaId == ruta.RutaId);
                _context.Venta.RemoveRange(ventas);
            }
            _context.Ruta.RemoveRange(rutas);

            var buses = _context.Bus.Where(b => b.EmpresaId == id);
            _context.Bus.RemoveRange(buses);

            var ofertas = _context.Oferta.Where(o => o.EmpresaId == id);
            _context.Oferta.RemoveRange(ofertas);

            var ventasEmpresa = _context.Venta.Where(v => v.EmpresaId == id);
            _context.Venta.RemoveRange(ventasEmpresa);

            _context.Empresa.Remove(empresa);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}