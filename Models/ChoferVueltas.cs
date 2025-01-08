using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class ChoferVueltas
    {
        public string idEmployee { get; set; }
        public int vueltas { get; set; }
        public DateTime fecha { get; set; }
        public string horario { get; set; }  // Nuevo campo para almacenar el horario del primero y último registro

        public ChoferVueltas() { }

        public ChoferVueltas(string idEmployee, int vueltas, DateTime fecha, string horario)
        {
            this.idEmployee = idEmployee;
            this.vueltas = vueltas;
            this.fecha = fecha;
            this.horario = horario;
        }
    }
}