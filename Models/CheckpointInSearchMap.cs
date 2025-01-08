using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class CheckpointInSearchMap
    {
        public string idEmployee { get; set; }
        public int idCheckpoint { get; set; }
        public DateTime dateRegister { get; set; }
        public int status { get; set; }
        public string name { get; set; }
        public long dateMin{ get; set; }
        public CheckpointInSearchMap()
        {
            
        }

        public CheckpointInSearchMap(string idEmployee, int idCheckpoint, DateTime dateRegister, int status, string name, long dateMin)
        {
            this.idEmployee = idEmployee;
            this.idCheckpoint = idCheckpoint;
            this.dateRegister = dateRegister;
            this.status = status;
            this.name = name;
            this.dateMin = dateMin;
        }
    }
}