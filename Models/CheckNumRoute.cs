using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class CheckNumRoute
    {
        public int idCheckpoint { get; set; }
        public int numPoint { get; set; }
        public CheckNumRoute()
        {
            
        }

        public CheckNumRoute(int idCheckpoint, int numPoint)
        {
            this.idCheckpoint = idCheckpoint;
            this.numPoint = numPoint;
        }
    }
}