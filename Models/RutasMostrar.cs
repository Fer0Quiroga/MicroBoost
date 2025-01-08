using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Controllers
{
    public class RutasMostrar
    {
        public int idRoute { get; set; }
        public decimal latitud { get; set; }
        public decimal longitud { get; set;}
        public  string name { get; set; }
        public int? status { get; set; }
        public int numPoint { get; set; }
        public RutasMostrar()
        {
                
        }

        public RutasMostrar(int idRoute, decimal latitud, decimal longitud, string name, int? status, int numPoint)
        {
            this.idRoute = idRoute;
            this.latitud = latitud;
            this.longitud = longitud;
            this.name = name;
            this.status = status;
            this.numPoint = numPoint;
        }
    }
}