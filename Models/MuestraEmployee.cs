using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraEmployee
    {
        [Display(Name = "CI")]
        public string idEmployee { get; set; }
        [Display(Name = "Nombre")]
        public string name { get; set; }
        [Display(Name = "Salario")]
        public decimal salary { get; set; }
        [Display(Name = "Placa de Vehículo")]
        public string plate { get; set; }
        public int idRole { get; set; }
        [Display(Name = "Punto de Control")]
        public string description { get; set; }
        public MuestraEmployee()
        {
            
        }

        public MuestraEmployee(string idEmployee, string name, decimal salary, string plate, int idRole, string description)
        {
            this.idEmployee = idEmployee;
            this.name = name;
            this.salary = salary;
            this.plate = plate;
            this.idRole = idRole;
            this.description = description;
        }
    }
}