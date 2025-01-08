using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ProyectoControlLineaBus.Models
{
    public class OwnerAuxiliar
    {
        public OwnerAuxiliar()
        {
                
        }

        public OwnerAuxiliar(string nit, string doc)
        {
            this.nit = nit;
            this.doc = doc;
        }

        [Display(Name = "NIT")]
        [MinLength(3)]
        [MaxLength(16)]
        [Required(ErrorMessage = "El NIT es obligatorio")]
        public string nit { get; set; }
        [Display(Name = "Documento")]
        [DataType(DataType.Url)]
        [MaxLength(200)]
        [Required(ErrorMessage = "El Documento es obligatorio")]
        public string doc { get; set; }
        [Display(Name = "Nombre")]
        [MinLength(3)]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string name { get; set; }
        [Display(Name = "Apellido")]
        [MinLength(3)]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string lastname { get; set; }
        [Display(Name = "Teléfono")]
        [MinLength(3)]
        [MaxLength(50)]
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string phone { get; set; }

        public OwnerAuxiliar(string nit, string doc, string name, string lastname, string phone) : this(nit, doc)
        {
            this.name = name;
            this.lastname = lastname;
            this.phone = phone;
        }
    }
}