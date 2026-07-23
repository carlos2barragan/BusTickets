using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Busticket.Models
{
    public class Boleto
    {
        [Key]
        public int BoletoId { get; set; }

        // 🔗 RELACIONES
        public int VentaId { get; set; }
        public required Venta Venta { get; set; }

        public required string UserId { get; set; }
        public required IdentityUser User { get; set; }

        public int RutaId { get; set; }
        public required Ruta Ruta { get; set; }

        public int AsientoId { get; set; }
        public required Asiento Asiento { get; set; }

        // 💰 PRECIO
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        // 🗓 FECHA DE COMPRA
        public DateTime FechaCompra { get; set; }

        // 🔑 CÓDIGO DE BOLETO
        public required string Codigo { get; set; }

        // 👤 DATOS DEL PASAJERO
        public string? DocumentoPasajero { get; set; }
        public string? TelefonoPasajero { get; set; }
    }
}