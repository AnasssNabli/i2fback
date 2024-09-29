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
        public string TvaN { get; set; }
        public string TvaN1 { get; set; }
        public string TvaVAR { get; set; }
        public string IrN { get; set; }
        public string IrN1 { get; set; }
        public string IrVAR { get; set; }
        public string IsN { get; set; }
        public string IsN1 { get; set; }
        public string IsVAR { get; set; }

        [ForeignKey("Utilisateur")]
        public int Id_User { get; set; }
        public Utilisateur? Utilisateur { get; set; }
    }
}
