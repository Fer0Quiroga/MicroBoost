using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Controllers
{
    public class ParadasMostrar
    {
        public int idCheckpoint { get; set; }
        public string description { get; set; }
        public int status { get; set; }
        public decimal latitud { get; set; }
        public decimal longitud { get; set; }
        public ParadasMostrar()
        {
                
        }

        public ParadasMostrar(int idCheckpoint, string description, int status, decimal latitud, decimal longitud)
        {
            this.idCheckpoint = idCheckpoint;
            this.description = description;
            this.status = status;
            this.latitud = latitud;
            this.longitud = longitud;
        }
    }
}