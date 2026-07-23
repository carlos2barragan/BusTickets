using Busticket.Data;
using Busticket.DTOs;
using Busticket.Extensions;
using Busticket.Models; // ✅ Aseguramos que use los modelos
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Busticket.Services;

namespace Busticket.Controllers
{
    [Route("Carrito")]
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Agregar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar([FromBody] EmailServiceDTO dto)
        {
            if (dto == null || dto.Asientos == null || !dto.Asientos.Any())
            {
                return BadRequest(new { mensaje = "No hay asientos seleccionados." });
            }

            var ruta = await _context.Ruta
                .FirstOrDefaultAsync(r => r.RutaId == dto.RutaId);

            if (ruta == null)
            {
                return BadRequest(new { mensaje = "Ruta no encontrada." });
            }

            // ✅ Usamos 'var' para que C# infiera el tipo correcto de la sesión
            var carrito = HttpContext.Session
                .GetObjectFromJson<List<CarritoItem>>("Carrito")
                ?? new List<CarritoItem>();

            foreach (var asiento in dto.Asientos)
            {
                if (!carrito.Any(a => a.AsientoId == asiento.AsientoId))
                {
                    carrito.Add(asiento);
                }
            }

            var total = carrito.Sum(a => a.Precio);

            HttpContext.Session.SetObjectAsJson("Carrito", carrito);
            HttpContext.Session.SetString("Total", total.ToString());

            return Ok(new
            {
                mensaje = "Asientos agregados al carrito",
                total,
                cantidad = carrito.Count
            });
        }

        [Authorize]
        [HttpGet("")]
        public IActionResult Index()
        {
            var carrito = HttpContext.Session
                .GetObjectFromJson<List<CarritoItem>>("Carrito")
                ?? new List<CarritoItem>();

            ViewBag.Total = carrito.Sum(a => a.Precio);
            return View(carrito);
        }

        [HttpPost("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar([FromBody] EliminarAsientoRequest request)
        {
            var carrito = HttpContext.Session.GetObjectFromJson<List<CarritoItem>>("Carrito")
                          ?? new List<CarritoItem>();

            var item = carrito.FirstOrDefault(c => c.AsientoId == request.AsientoId);
            if (item != null)
            {
                carrito.Remove(item);
                HttpContext.Session.SetObjectAsJson("Carrito", carrito);
            }

            var total = carrito.Sum(c => c.Precio);
            var cantidad = carrito.Count;

            return Ok(new { total, cantidad });
        }

        public class EliminarAsientoRequest
        {
            public int AsientoId { get; set; }
        }
    }
}