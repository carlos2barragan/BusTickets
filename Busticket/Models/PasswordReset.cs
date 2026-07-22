using System;
using System.ComponentModel.DataAnnotations;

namespace Busticket.Models
{
    public class PasswordReset
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; }   

        [Required]
        public Guid Token { get; set; }

        public string? IdentityToken { get; set; }

        [Required]
        public DateTime FechaExpiracion { get; set; }

        public bool Usado { get; set; } = false;
    }
}
