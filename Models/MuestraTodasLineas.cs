using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraTodasLineas
    {
        public string nameLine { get; set; }
        public List<MuestraRutasMapas> MuestraLin{ get; set; }
        public List<string> choferesTiempo{ get; set; }
        public MuestraTodasLineas()
        {
            
        }

        public MuestraTodasLineas(string nameLine, List<MuestraRutasMapas> MuestraLin)
        {
            this.nameLine = nameLine;
            this.MuestraLin = MuestraLin;
        }

        public MuestraTodasLineas(string nameLine, List<MuestraRutasMapas> muestraLin, List<string> choferesTiempo) : this(nameLine, muestraLin)
        {
            this.choferesTiempo = choferesTiempo;
        }
    }
}