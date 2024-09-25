using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace I2FCONSEIL.Models
{
    public class Utilisateur : IdentityUser<int>
    {
        public string Nom { get; set; }
        public string? Nomsociete { get; set; }
        public string Prenom { get; set; }
        public string Cin { get; set; }

        public string Role { get; set; } 

        [Phone]
        public string? Telephone { get; set; }
    }
}
