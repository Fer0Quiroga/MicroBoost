using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ProyectoControlLineaBus.Models
{
    public class ReportDates
    {
        [Display (Name ="Fecha de inicio")]
        public DateTime start { get; set; }
        [Display(Name = "Fecha final")]
        public DateTime end { get; set; }
        public ReportDates()
        {
            
        }

        public ReportDates(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
        }
    }
}