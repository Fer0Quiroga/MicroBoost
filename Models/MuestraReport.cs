using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraReport
    {
        public int idCheckpoint { get; set; }
        public string name { get; set; }
        public string idEmployee{ get; set; }
        public string plate { get; set; }
        public DateTime dateRegister{ get; set; }
        public string descCheckpoint { get; set; }
        public MuestraReport()
        {
            
        }

        public MuestraReport(int idCheckpoint, string name, string idEmployee, string plate, DateTime dateRegister, string descCheckpoint)
        {
            this.idCheckpoint = idCheckpoint;
            this.name = name;
            this.idEmployee = idEmployee;
            this.plate = plate;
            this.dateRegister = dateRegister;
            this.descCheckpoint = descCheckpoint;
        }
    }
}