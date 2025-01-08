using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MostrarFallasMecanicas
    {
        public string plate { get; set; }
        public string fallas { get; set; }
        public DateTime fechaFalla{ get; set; }
        public MostrarFallasMecanicas()
        {
                
        }

        public MostrarFallasMecanicas(string plate, string fallas, DateTime fechaFalla)
        {
            this.plate = plate;
            this.fallas = fallas;
            this.fechaFalla = fechaFalla;
        }
    }
}