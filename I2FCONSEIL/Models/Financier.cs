using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace I2FCONSEIL.Models
{
    public class Financier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Designation { get; set; }
        public float ExN { get; set; }
        public float ExN1 { get; set; }
        public string Var { get; set; }

        [ForeignKey("Utilisateur")]
        public int Id_User { get; set; }
        public Utilisateur? Utilisateur { get; set; }
    }
}
