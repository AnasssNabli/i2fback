﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace I2FCONSEIL.Models
{
    public class Social
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Mois { get; set; }
        public string MasseN { get; set; }
        public string MasseN1 { get; set; }
        public string MasseVAR { get; set; }
        public string CnssN { get; set; }
        public string CnssN1 { get; set; }
        public string CnssVAR { get; set; }

        [ForeignKey("Utilisateur")]
        public int Id_User { get; set; }
        public Utilisateur? Utilisateur { get; set; }
    }
}
