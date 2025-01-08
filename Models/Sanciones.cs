using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class Sanciones
    {
        [Display(Name = "TÍTULO")]

        [DataType(DataType.Text)]
        [MinLength(3)]
        [MaxLength(30)]
        [Required(ErrorMessage = "Este punto es obligatorio")]
        public string title{ get; set; }
        [Display(Name = "DESCRIPCIÓN")]
        [MinLength(3)]
        [MaxLength(255)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Este punto es obligatorio")]
        public string description { get; set; }
        public Sanciones()
        {
                
        }
        public Sanciones( string title, string description)
        {
            this.title = title;
            this.description = description;
        }
    }
}