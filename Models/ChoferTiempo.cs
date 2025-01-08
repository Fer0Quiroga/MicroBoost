using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class ChoferTiempo
    {
        public string chofer{ get; set; }
        public string tiempo { get; set; }
        public ChoferTiempo()
        {
            
        }

        public ChoferTiempo(string chofer, string tiempo)
        {
            this.chofer = chofer;
            this.tiempo = tiempo;
        }
    }
}