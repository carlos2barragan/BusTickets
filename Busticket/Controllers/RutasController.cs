using Busticket.Data;
using Busticket.DTOs;
using Busticket.Extensions;
using Busticket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Busticket.Controllers
{
    [AllowAnonymous]
    public class RutasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RutasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // LANDING - DESTINOS
        // GET: /Rutas
        // =========================
        public async Task<IActionResult> Index()
        {
            var rutas = await _context.Ruta
                .Include(r => r.CiudadDestino)
                .ToListAsync();

            return View(rutas);
        }

        // =========================
        // LISTADO REAL DE RUTAS
        // GET: /Rutas/Listado
        // =========================
        public async Task<IActionResult> Listado(string origen, string destino)
        {
            var rutas = _context.Ruta
                .Include(r => r.CiudadOrigen)
                .Include(r => r.CiudadDestino)
                .Include(r => r.Empresa)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(origen))
                rutas = rutas.Where(r => r.CiudadOrigen.Nombre == origen);

            if (!string.IsNullOrWhiteSpace(destino))
                rutas = rutas.Where(r => r.CiudadDestino.Nombre == destino);

            return View(await rutas.ToListAsync());
        }

        // =========================
        // DETALLE DE RUTA / ASIENTOS
        // GET: /Rutas/Info/5
        // =========================
        public async Task<IActionResult> Info(int id)
        {
            var ruta = await _context.Ruta
                .Include(r => r.CiudadOrigen)
                .Include(r => r.CiudadDestino)
                .Include(r => r.Empresa)
                .FirstOrDefaultAsync(r => r.RutaId == id);

            if (ruta == null)
                return NotFound();

            var asientos = await _context.Asiento
                .Where(a => a.RutaId == id)
                .Select(a => new Asiento
                {
                    AsientoId = a.AsientoId,
                    Numero = a.Numero,
                    Disponible = a.Disponible
                })
                .ToListAsync();

            var vm = new ReporteVentaVM
            {
                Ruta = ruta,
                Asientos = asientos
            };

            return View(vm);
        }

        // =========================
        // SELECCI�N INDIVIDUAL
        // =========================
        [HttpPost]
        public IActionResult SeleccionarAsiento(int asientoNumero, int precio)
        {
            var carrito = HttpContext.Session
                .GetObjectFromJson<List<int>>("Carrito") ?? new List<int>();

            if (carrito.Contains(asientoNumero))
                carrito.Remove(asientoNumero);
            else
                carrito.Add(asientoNumero);

            var total = carrito.Count * precio;

            HttpContext.Session.SetObjectAsJson("Carrito", carrito);
            HttpContext.Session.SetInt32("Total", total);

            return Json(new
            {
                success = true,
                cantidad = carrito.Count,
                total = total
            });
        }

        // =========================
        // AGREGAR VARIOS ASIENTOS
        // =========================
        

    }
}
