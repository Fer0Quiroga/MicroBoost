using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraRutasPasajeros
    {
        public string nameLine{ get; set; }
        public int inicio { get; set; }
        public int final { get; set; }
        public List<MuestraRutasMapas> MuestraLin { get; set; }
        public List<ChoferTiempo> choferTiempos{ get; set; }
        public MuestraRutasPasajeros()
        {
            
        }

        public MuestraRutasPasajeros(string nameLine, int inicio, int final, List<MuestraRutasMapas> muestraLin, List<ChoferTiempo> choferTiempos)
        {
            this.nameLine = nameLine;
            this.inicio = inicio;
            this.final = final;
            MuestraLin = muestraLin;
            this.choferTiempos = choferTiempos;
        }
    }
}