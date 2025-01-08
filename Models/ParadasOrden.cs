using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class ParadasOrden
    {
        public int id { get; set; }
        public int status{ get; set; }
        public decimal latitud{ get; set; }
        public decimal longitud{ get; set; }
        public ParadasOrden()
        {
            
        }

        public ParadasOrden(int id, int status, decimal latitud, decimal longitud)
        {
            this.id = id;
            this.status = status;
            this.latitud = latitud;
            this.longitud = longitud;
        }
    }
}