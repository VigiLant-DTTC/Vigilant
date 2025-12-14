using VigiLant.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace VigiLant.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public string Email { get; set; }

        public string SenhaHash { get; set; }
        public Cargo cargo { get; set; } 
    }
}