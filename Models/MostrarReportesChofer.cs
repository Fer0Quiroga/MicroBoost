using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class MostrarReportesChofer
    {
        public string Descripcion { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Count { get; set; }
        public MostrarReportesChofer()
        {
            
        }

        public MostrarReportesChofer(string descripcion, int month, int year, int count)
        {
            Descripcion = descripcion;
            Month = month;
            Year = year;
            Count = count;
        }
    }
}