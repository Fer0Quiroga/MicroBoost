using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraRoute
    {
        public int idRoute { get; set; }
        public string idLine { get; set; }
        public int idPlace { get; set; }
        [Display(Name = "Extensión")]
        public string extention{ get; set; }
        [Display(Name = "Nombre del lugar")]
        [MinLength(1)]
        [MaxLength(30)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El nombre del lugar es obligatorio")]
        public string descriptionPlace{ get; set; }
        [Display(Name = "Latitud y Longitud")]
        [MinLength(1)]
        [MaxLength(46)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "La Latitud y Longitud es obligatorio")]
        public string latitud_longitud  { get; set; }
        public int? numberRoute { get; set; }
        public MuestraRoute()
        {
            
        }

        public MuestraRoute(int idRoute, string idLine, int idPlace, string extention, string descriptionPlace, string latitud_longitud)
        {
            this.idRoute = idRoute;
            this.idLine = idLine;
            this.idPlace = idPlace;
            this.extention = extention;
            this.descriptionPlace = descriptionPlace;
            this.latitud_longitud = latitud_longitud;
        }
        public MuestraRoute(int idRoute, string idLine, int idPlace, string extention, string descriptionPlace)
        {
            this.idRoute = idRoute;
            this.idLine = idLine;
            this.idPlace = idPlace;
            this.extention = extention;
            this.descriptionPlace = descriptionPlace;
        }

        public MuestraRoute(int idRoute, string idLine, int idPlace, string extention, string descriptionPlace, string latitud_longitud, int? numberRoute) : this(idRoute, idLine, idPlace, extention, descriptionPlace, latitud_longitud)
        {
            this.numberRoute = numberRoute;
        }
    }
}