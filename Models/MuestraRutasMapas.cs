using ProyectoControlLineaBus.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraRutasMapas
    {

        public List<RutasMostrar> ListRutasMostrar { get; set; }
        public List<ParadasMostrar> ListParadasMostrar { get; set; }
        public int NumeroRuta { get; set; }
        public List<BloqueosMostrar> ListBloqueosMostrar { get; set; }
        public MuestraRutasMapas()
        {
                
        }

        public MuestraRutasMapas(List<RutasMostrar> listRutasMostrar, List<ParadasMostrar> listParadasMostrar, int numeroRuta, List<BloqueosMostrar> listBloqueosMostrar)
        {
            ListRutasMostrar = listRutasMostrar;
            ListParadasMostrar = listParadasMostrar;
            NumeroRuta = numeroRuta;
            ListBloqueosMostrar = listBloqueosMostrar;
        }

        public double CalcularDistanciaHaversine(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double RadioTierra = 6371; // Radio de la Tierra en kilómetros
            double dLat = (double)(lat2 - lat1) * Math.PI / 180.0;
            double dLon = (double)(lon2 - lon1) * Math.PI / 180.0;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos((double)lat1 * Math.PI / 180.0) * Math.Cos((double)lat2 * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return RadioTierra * c;
        }
    }
}