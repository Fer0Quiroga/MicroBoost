using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ProyectoControlLineaBus.Models
{
    public class AddCheckpoint
    {
        [Display(Name = "Lugar del punto de Control")]
        [MinLength(3)]
        [MaxLength(62)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "Este punto es obligatorio")]
        public string description { get; set; }
        [Display(Name = "Último punto de Control(Marca si es el último punto de control)")]
        public bool final { get; set; }
        [Display(Name = "Latitud y longitud")]
        [MinLength(3)]
        [MaxLength(46)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "La latitud y longitud son obligatorias")]
        public string latitud_longitud { get; set; }
        public AddCheckpoint()
        {
            
        }

        public AddCheckpoint(string description, bool final)
        {
            this.description = description;
            this.final = final;
        }

        public AddCheckpoint(string description, bool final, string latitud_longitud) : this(description, final)
        {
            this.latitud_longitud = latitud_longitud;
        }
    }
}