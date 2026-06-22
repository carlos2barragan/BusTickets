using Busticket.Data;
using Busticket.Models;
using Busticket.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
    
namespace Busticket.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var reporte = await _context.Venta
                .Include(v => v.User)
                .Include(v => v.Empresa)
                .Include(v => v.Ruta)
                .GroupBy(v => new
                {
                    v.UserId,
                    v.EmpresaId,
                    v.RutaId,
                    Fecha = v.Fecha.Date
                })
                .Select(g => new ReporteVentaVM1
                {
                    VentaId = g.Min(x => x.VentaId),
                    Usuario = g.First().User.UserName,
                    Empresa = g.First().Empresa.Nombre,
                    Precio = g.Sum(x => x.Total), // ✅ TOTAL REAL
                    Fecha = g.Key.Fecha
                })
                .OrderByDescending(x => x.Fecha)
                .ToListAsync();

            return View(reporte);
        }






        public async Task<IActionResult> Rutas()
        {
            var rutas = await _context.Ruta
                .Include(r => r.Empresa)
                .Include(r => r.CiudadOrigen)
                .Include(r => r.CiudadDestino)
                .ToListAsync();

            return View(rutas);
        }


        private void CargarViewBags()
        {
            ViewBag.Ciudades = _context.Ciudad.ToList();
            ViewBag.Empresas = _context.Empresa.ToList();
        }

        public IActionResult CrearRuta()
        {
            CargarViewBags();
            return View(new Ruta { FechaSalida = DateTime.Now, FechaLlegada = DateTime.Now.AddHours(1) });
        }

        [HttpPost]
        public IActionResult CrearRuta(Ruta ruta)
        {
            if (!ModelState.IsValid)
            {
                CargarViewBags();

                var errores = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                TempData["ErrorMessage"] = string.Join(", ", errores);

                return View(ruta);
            }

            ruta.TipoBus ??= "Normal";
            ruta.NumAsientos ??= 20;

            _context.Ruta.Add(ruta);
            _context.SaveChanges();

            var asientos = GenerarAsientos(ruta.RutaId, ruta.TipoBus, ruta.NumAsientos.Value);
            _context.Asiento.AddRange(asientos);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Ruta creada con {ruta.NumAsientos} asientos ({ruta.TipoBus}).";
            return RedirectToAction("Rutas");
        }


        public async Task<IActionResult> EditarRuta(int id)
        {
            var ruta = await _context.Ruta.FindAsync(id);
            if (ruta == null) return NotFound();

            CargarViewBags();
            return View(ruta);
        }


        [HttpPost]
        public async Task<IActionResult> EditarRuta(Ruta ruta)
        {
            if (!ModelState.IsValid)
            {
                CargarViewBags();

                var errores = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                TempData["ErrorMessage"] = string.Join(", ", errores);

                return View(ruta);
            }

            _context.Ruta.Update(ruta);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ruta actualizada correctamente.";
            return RedirectToAction("Rutas");
        }


        public async Task<IActionResult> EliminarRuta(int id)
        {
            var ruta = await _context.Ruta
                .Include(r => r.Empresa)
                .Include(r => r.CiudadOrigen)
                .Include(r => r.CiudadDestino)
                .FirstOrDefaultAsync(r => r.RutaId == id);

            if (ruta == null) return NotFound();

            return View(ruta);
        }

        [HttpPost, ActionName("EliminarRuta")]
        public async Task<IActionResult> EliminarRutaConfirmado(int id)
        {
            // Traer la ruta con sus asientos
            var ruta = await _context.Ruta
                .Include(r => r.Asientos)   // asientos de la ruta
                .FirstOrDefaultAsync(r => r.RutaId == id);

            if (ruta == null) return NotFound();

            // -----------------------------
            // 1️⃣ Borrar boletos asociados a cada asiento
            // -----------------------------
            var boletos = _context.Boleto
                .Where(b => ruta.Asientos.Select(a => a.AsientoId).Contains(b.AsientoId));
            _context.Boleto.RemoveRange(boletos);

            // -----------------------------
            // 2️⃣ Borrar ventas asociadas (opcional)
            // -----------------------------
            var ventas = _context.Venta.Where(v => v.RutaId == id);
            _context.Venta.RemoveRange(ventas);

            // -----------------------------
            // 3️⃣ Borrar asientos
            // -----------------------------
            if (ruta.Asientos != null && ruta.Asientos.Any())
                _context.Asiento.RemoveRange(ruta.Asientos);

            // -----------------------------
            // 4️⃣ Borrar la ruta
            // -----------------------------
            _context.Ruta.Remove(ruta);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ruta eliminada correctamente.";
            return RedirectToAction("Rutas");
        }

        private static List<Asiento> GenerarAsientos(int rutaId, string tipoBus, int num)
        {
            return Enumerable.Range(1, num).Select(i => new Asiento
            {
                Numero = i,
                Codigo = i.ToString(),
                RutaId = rutaId,
                Disponible = true
            }).ToList();
        }

        public IActionResult Dashboard()
        {
            ViewBag.TotalClientes = _context.Users.Count();
            ViewBag.TotalEmpresas = _context.Empresa.Count();
            ViewBag.TotalRutas = _context.Ruta.Count();
            ViewBag.TotalVentas = _context.Venta.Count();
            ViewBag.TotalBoletos = _context.Boleto.Count();

            return View();
        }
    }
}
