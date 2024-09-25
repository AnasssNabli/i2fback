using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace I2FCONSEIL.Models
{
    public class Fiscal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Mois { get; set; }
        public float TvaN { get; set; }
        public float TvaN1 { get; set; }
        public string TvaVAR { get; set; }
        public float IrN { get; set; }
        public float IrN1 { get; set; }
        public string IrVAR { get; set; }
        public float IsN { get; set; }
        public float IsN1 { get; set; }
        public string IsVAR { get; set; }

        [ForeignKey("Utilisateur")]
        public int Id_User { get; set; }
        public Utilisateur? Utilisateur { get; set; }
    }
}
