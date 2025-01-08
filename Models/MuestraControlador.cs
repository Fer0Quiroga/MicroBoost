using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraControlador
    {
        public string idEmployee { get; set; }
        public string plate { get; set; }
        public List<string> retrasos{ get; set; }
        public List<string> incidencias { get; set; }
        public DateTime registerDate { get; set; }
        public int numTicket { get; set; }
        public MuestraControlador()
        {
        }

        public MuestraControlador(string idEmployee, string plate, List<string> retrasos, List<string> incidencias, DateTime registerDate, int numTicket)
        {
            this.idEmployee = idEmployee;
            this.plate = plate;
            this.retrasos = retrasos;
            this.incidencias = incidencias;
            this.registerDate = registerDate;
            this.numTicket = numTicket;
        }
    }
}