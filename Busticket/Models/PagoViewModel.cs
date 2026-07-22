using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Busticket.Models
{
    public class PagoViewModel
    {
        // =========================
        // COMPRA (OBLIGATORIO)
        // =========================
        public int RutaId { get; set; }
        public required List<string> Asientos { get; set; } = new();
        public decimal Total { get; set; }

        // =========================
        // SOLO PARA MOSTRAR
        // =========================
        public required string Origen { get; set; }
        public required string Destino { get; set; }
        public required string Empresa { get; set; }
        public required string Duracion { get; set; }
        public required string Fecha { get; set; }
        public required string Hora { get; set; }

        // =========================
        // DATOS DEL PASAJERO
        // =========================
        public string DocumentoPasajero { get; set; } = "";
        public string TelefonoPasajero { get; set; } = "";

        // =========================
        // TARJETA (FAKE / UI)
        // ❌ NO VALIDAR
        // =========================
        public required string Nombre { get; set; }
        public required string NumeroTarjeta { get; set; }
        public required string Validez { get; set; }
        public required string CVC { get; set; }
        public required string Descuento { get; set; }
    }
}