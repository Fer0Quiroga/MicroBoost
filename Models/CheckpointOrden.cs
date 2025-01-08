using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class CheckpointOrden
    {
        public int idCheckpoint { get; set; }
        public int orden { get; set; }
        public CheckpointOrden()
        {
                
        }

        public CheckpointOrden(int idCheckpoint, int orden)
        {
            this.idCheckpoint = idCheckpoint;
            this.orden = orden;
        }
    }
}