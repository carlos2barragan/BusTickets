using Busticket.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Busticket.Controllers
{
    public class ItinerariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ItinerariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var itinerarios = await _context.Itinerario
                .Include(i => i.Ruta)
                    .ThenInclude(r => r.CiudadOrigen)
                .Include(i => i.Ruta)
                    .ThenInclude(r => r.CiudadDestino)
                .Include(i => i.Bus)
                .Include(i => i.Conductor)
                .ToListAsync();

            return View(itinerarios);
        }
    }
}
