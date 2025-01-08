using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class LineaRutaPasajero
    {
        public string nameLine { get; set; }
        public int inicio { get; set; }
        public int final { get; set; }
        public int numRuta { get; set; }
        public  int numType{ get; set; }
        public LineaRutaPasajero()
        {
            
        }

        public LineaRutaPasajero(string nameLine, int inicio, int final, int numRuta, int numType)
        {
            this.nameLine = nameLine;
            this.inicio = inicio;
            this.final = final;
            this.numRuta = numRuta;
            this.numType = numType;
        }
    }
}