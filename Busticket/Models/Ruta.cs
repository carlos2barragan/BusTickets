using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Busticket.Models
{
    [Table("Ruta")]
    public class Ruta
    {
        [Key]
        public int RutaId { get; set; }

        // Ciudad Origen - El ID es el que realmente importa para la DB
        [Required]
        public int CiudadOrigenId { get; set; }

        [ForeignKey("CiudadOrigenId")]
        [ValidateNever]
        // ✅ Quitamos 'required' y usamos '?' para permitir que sea opcional en el código (C#)
        public Ciudad? CiudadOrigen { get; set; }

        // Ciudad Destino
        [Required]
        public int CiudadDestinoId { get; set; }

        [ForeignKey("CiudadDestinoId")]
        [ValidateNever]
        public Ciudad? CiudadDestino { get; set; }

        // Empresa
        [Required]
        public int EmpresaId { get; set; }

        [ForeignKey("EmpresaId")]
        [ValidateNever]
        public Empresa? Empresa { get; set; }

        [Required]
        public decimal Precio { get; set; }

        [Required]
        public int DuracionMin { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaSalida { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaLlegada { get; set; }

        public string? ImagenUrl { get; set; }

        public string? TipoBus { get; set; }   // "Normal" | "VIP" | "DosPisos"
        public int? NumAsientos { get; set; }

        // Lista de asientos
        public List<Asiento> Asientos { get; set; } = new List<Asiento>();
    }
}