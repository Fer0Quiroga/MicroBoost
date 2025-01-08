using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class ChoferObservaciones
    {

        public string observacion{ get; set; }
        public DateTime fechaObservacion{ get; set; }
        public ChoferObservaciones()
        {
                
        }

        public ChoferObservaciones( string observacion, DateTime fechaObservacion)
        {

            this.observacion = observacion;
            this.fechaObservacion = fechaObservacion;
        }
    }
}