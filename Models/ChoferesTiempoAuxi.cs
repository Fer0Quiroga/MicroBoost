using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class ChoferesTiempoAuxi
    {
        public string description { get; set; }
        public int dateRegist { get; set; }
        public ChoferesTiempoAuxi()
        {
            
        }

        public ChoferesTiempoAuxi(string description, int dateRegist)
        {
            this.description = description;
            this.dateRegist = dateRegist;
        }
    }
}