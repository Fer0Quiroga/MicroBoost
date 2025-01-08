using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraRouteMap
    {
        public decimal[] StartPoint { get; set; }
        public decimal[] EndPoint { get; set; }
        public List<decimal[]> IntermediatePoints { get; set; }
        public List<MuestraRoute> muestraRoutes { get; set; }
        public List<string[]> IntermediateCheckpoints { get; set; }
        public int NumberRoute { get; set; }
        public MuestraRouteMap()
        {
            
        }

        public MuestraRouteMap(decimal[] startPoint, decimal[] endPoint, List<decimal[]> intermediatePoints, List<MuestraRoute> muestraRoutes)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            IntermediatePoints = intermediatePoints;
            this.muestraRoutes = muestraRoutes;
        }
    }
}