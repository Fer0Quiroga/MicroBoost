using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class EmployeeChofer
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
        public string name{ get; set; }
        [Display(Name = "Apellido")]
        [MinLength(3)]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string lastname { get; set; }
        [Display(Name = "Placa del vehículo")]
        [MinLength(3)]
        [MaxLength(8)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "La placa del vehículo es obligatorio.")]
        public string plate { get; set; }
        [Display(Name = "Salario")]
        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "El salario es obligatorio.")]
        public decimal salary { get; set; }
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "El E-mail es obligatorio")]
        public string email { get; set; }
        public EmployeeChofer()
        {
            
        }

        public EmployeeChofer(string idEmployee, string name, string lastname, string plate, decimal salary)
        {
            this.idEmployee = idEmployee;
            this.name = name;
            this.lastname = lastname;
            this.plate = plate;
            this.salary = salary;
        }
        public EmployeeChofer(string idEmployee, string name, string lastname, string plate)
        {
            this.idEmployee = idEmployee;
            this.name = name;
            this.lastname = lastname;
            this.plate = plate;
        }

        public EmployeeChofer(string idEmployee, string name, string lastname, string plate, decimal salary, string email) : this(idEmployee, name, lastname, plate, salary)
        {
            this.email = email;
        }
    }
}