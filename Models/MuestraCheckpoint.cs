using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraCheckpoint
    {
        public decimal[] StartPoint { get; set; }
        public decimal[] EndPoint { get; set; }
        public List<decimal[]> IntermediatePoints { get; set; }
        public List<string[]> IntermediateCheckpoints { get; set; }
        public List<Checkpoint> checkpoints { get; set; }
        public MuestraCheckpoint()
        {
            
        }

        public MuestraCheckpoint(decimal[] startPoint, decimal[] endPoint, List<decimal[]> intermediatePoints, List<string[]> intermediateCheckpoints, List<Checkpoint> checkpoints)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            IntermediatePoints = intermediatePoints;
            IntermediateCheckpoints = intermediateCheckpoints;
            this.checkpoints = checkpoints;
        }
    }
}