using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class SancionesEmpleadoMostrar
    {
        public string idEmpleado { get; set; }
        public string titulo{ get; set; }
        public string description { get; set; }
        public DateTime dateRegister{ get; set; }
        public SancionesEmpleadoMostrar()
        {
            
        }

        public SancionesEmpleadoMostrar(string idEmpleado, string titulo, string description, DateTime dateRegister)
        {
            this.idEmpleado = idEmpleado;
            this.titulo = titulo;
            this.description = description;
            this.dateRegister = dateRegister;
        }
    }
}