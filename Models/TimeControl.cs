//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProyectoControlLineaBus.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TimeControl
    {
        public int idTimeControl { get; set; }
        public string idEmployee { get; set; }
        public int idCheckpoint { get; set; }
        public Nullable<System.DateTime> dateRegister { get; set; }
        public string plate { get; set; }
        public Nullable<int> enable { get; set; }
        public Nullable<int> numTicket { get; set; }
    
        public virtual Checkpoint Checkpoint { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
