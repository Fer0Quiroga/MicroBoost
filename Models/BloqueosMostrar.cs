using ProyectoControlLineaBus.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class BloqueosMostrar
    {

        public int idDeviation { get; set; }
        public string name { get; set; }
        public List<RutasMostrar> ListRutasMostrar { get; set; }
        public double latitud { get; set; }
        public double longitud { get; set; }
        public BloqueosMostrar()
        {
                
        }

        public BloqueosMostrar(int idDeviation, string name, List<RutasMostrar> listRutasMostrar)
        {
            this.idDeviation = idDeviation;
            this.name = name;
            ListRutasMostrar = listRutasMostrar;
        }

        public BloqueosMostrar(int idDeviation, string name, List<RutasMostrar> listRutasMostrar, double latitud, double longitud) : this(idDeviation, name, listRutasMostrar)
        {
            this.latitud = latitud;
            this.longitud = longitud;
        }
    }
}