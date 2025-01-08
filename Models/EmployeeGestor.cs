using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class EmployeeGestor
    {
        [Display(Name = "CI")]
        [MinLength(3)]
        [MaxLength(16)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El CI es obligatorio.")]
        public string idEmployee { get; set; }
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
        [Display(Name = "Placa del vehículo")]
        [MinLength(3)]
        [MaxLength(8)]
        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "La teléfono es obligatorio.")]
        public string phone { get; set; }
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "El E-mail es obligatorio")]
        public string email { get; set; }
        public EmployeeGestor()
        {
                
        }

        public EmployeeGestor(string idEmployee, string name, string lastname, string phone, string email)
        {
            this.idEmployee = idEmployee;
            this.name = name;
            this.lastname = lastname;
            this.phone = phone;
            this.email = email;
        }
    }
}