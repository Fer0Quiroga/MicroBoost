using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ProyectoControlLineaBus.Models
{
    public class LineControl
    {
        [Display (Name ="Extensión")]
        public string extention { get; set; }
        [Display(Name = "Línea")]
        [MinLength(1)]
        [MaxLength(9)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El nombre de la Línea es obligatorio")]
        public string nameLine { get; set; }

        public Nullable<decimal> price { get; set; }

        public string description { get; set; }
        [Display(Name = "Mail")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "El Mail es obligatorio")]
        public string email { get; set; }
        public LineControl()
        {
            
        }

        public LineControl(string extention, string nameLine, decimal? price, string description, int? status, string email)
        {
            this.extention = extention;
            this.nameLine = nameLine;
            this.price = price;
            this.description = description;
            this.email = email;
        }

    }
}