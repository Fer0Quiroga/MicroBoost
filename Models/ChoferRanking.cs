using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class ChoferRanking
    {
        public string idEmployee { get; set; }
        public int vueltas{ get; set; }
        public int status { get; set; }
        public ChoferRanking()
        {
            
        }

        public ChoferRanking(string idEmployee, int vueltas, int status)
        {
            this.idEmployee = idEmployee;
            this.vueltas = vueltas;
            this.status = status;
        }
    }
}