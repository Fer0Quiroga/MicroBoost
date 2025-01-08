using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraListCheckpoint
    {
        public string idEmployee { get; set; }
        public int idCheckpoint{ get; set; }
        public string description { get; set; }
        public MuestraListCheckpoint()
        {
            
        }

        public MuestraListCheckpoint(string idEmployee, int idCheckpoint, string description)
        {
            this.idEmployee = idEmployee;
            this.idCheckpoint = idCheckpoint;
            this.description = description;
        }
    }
}