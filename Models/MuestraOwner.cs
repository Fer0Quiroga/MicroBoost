using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ProyectoControlLineaBus.Models
{
    public class MuestraOwner
    {
        public MuestraOwner()
        {

        }

        public MuestraOwner(string idLine, string idPerson, string nit, string doc)
        {
            this.idLine = idLine;
            this.idPerson = idPerson;
            this.nit = nit;
            this.doc = doc;
        }

        public MuestraOwner(string idLine, string idPerson, string nit, string doc, int idOwner) : this(idLine, idPerson, nit, doc)
        {
            this.idOwner = idOwner;
        }

        [Display(Name = "*")]
        public string idLine { get; set; }
        [Display(Name = "*")]
        public string idPerson { get; set; }
        [Display(Name = "NIT")]
        public string nit { get; set; }
        [Display(Name = "Documento")]
        public string doc { get; set; }
        public int idOwner { get; set; }
    }
}