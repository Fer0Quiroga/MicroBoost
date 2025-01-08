using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraLine
    {
        [Display(Name = "Línea")]
        public string nameLine { get; set; }
        [Display(Name = "")]
        public string description { get; set; }
        public List<MuestraRutasMapas> muestraRutasMapas{ get; set; }

        public MuestraLine()
        {
            
        }

        public MuestraLine(string nameLine, string description, List<MuestraRutasMapas> muestraRutasMapas)
        {
            this.nameLine = nameLine;
            this.description = description;
            this.muestraRutasMapas = muestraRutasMapas;
        }
    }
}