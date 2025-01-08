using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraSearchMap
    {
        [Display(Name = "Línea")]
        public string nameLine { get; set; }
        [Display(Name = "")]
        public string description { get; set; }
        [Display(Name = "Precio")]
        public decimal price { get; set; }
        public decimal[] StartPoint { get; set; }
        public decimal[] EndPoint { get; set; }
        public List<decimal[]> IntermediatePoints { get; set; }
        public List<string[]> IntermediateCheckpoints { get; set; }
        public List<string[]> NameCheckpoint{ get; set; }
        public List<CheckpointInSearchMap> ultCheckpoints{ get; set; }
        public MuestraSearchMap()
        {
            
        }

        public MuestraSearchMap(string nameLine, string description, decimal price, decimal[] startPoint, decimal[] endPoint, List<decimal[]> intermediatePoints, List<string[]> intermediateCheckpoints, List<string[]> nameCheckpoint, List<CheckpointInSearchMap> ultCheckpoints)
        {
            this.nameLine = nameLine;
            this.description = description;
            this.price = price;
            StartPoint = startPoint;
            EndPoint = endPoint;
            IntermediatePoints = intermediatePoints;
            IntermediateCheckpoints = intermediateCheckpoints;
            NameCheckpoint = nameCheckpoint;
            this.ultCheckpoints = ultCheckpoints;
        }
    }
}