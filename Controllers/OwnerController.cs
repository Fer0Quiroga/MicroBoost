using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoControlLineaBus.Controllers
{
    public class OwnerController : Controller
    {
        // GET: Owner
        public ActionResult Index(string id)
        {
            try
            {
                if (id == null && Session["OwnerLine"] == null) return RedirectToAction("Index", "Home");
                if(Session["OwnerLine"] == null)
                Session["OwnerLine"] = id;
                id = Session["OwnerLine"].ToString();
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                List<Owner> owner = new List<Owner>();
                List<MuestraOwner> muestraOwners = new List<MuestraOwner>();
                if (Session["idLineCreateOwner"] == null)
                    Session["idLineCreateOwner"] = id;
                string s = Session["idLineCreateOwner"].ToString();
                using (dbModels context = new dbModels())
                {
                    owner = context.Owner.Where(x => x.idLine == s).ToList();
                }
                if (owner.Count > 0)
                    using (dbModels context = new dbModels())
                    {
                        foreach (var item in owner)
                        {
                            Person person = context.Person.Where(x => x.nit == item.idPerson).FirstOrDefault();
                            muestraOwners.Add(new MuestraOwner(item.idLine.ToString(), item.idPerson, person.nit, item.doc, item.idOwner));
                        }

                    }
                return View(muestraOwners);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult Details(string id)
        {
            try
            {
                if (id == null) return RedirectToAction("Index", "Home");
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                using (dbModels context = new dbModels())
                {
                    return View(context.Person.Where(x => x.nit == id).FirstOrDefault());
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Owner/Create
        public ActionResult Create()
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                OwnerAuxiliar ownerAuxiliar = new OwnerAuxiliar();
                if (Session["PersonOwner"] != null) ownerAuxiliar.nit = Session["PersonOwner"].ToString();
                Session["PersonOwner"] = null;
                return View(ownerAuxiliar);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Owner/Create
        [HttpPost]
        public ActionResult Create(OwnerAuxiliar muestraOwner)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                string idLine = Session["idLineCreateOwner"].ToString();
                Owner ow = new Owner();
                Person valid = new Person();
                Employee employee = new Employee();
                using (dbModels context = new dbModels()) employee = context.Employee.Where(x => x.idEmployee == muestraOwner.nit.ToUpper().Trim()).FirstOrDefault();
                using (dbModels context = new dbModels())
                {
                    valid = context.Person.Where(x => x.nit == muestraOwner.nit.ToUpper().Trim() && x.status == 1).FirstOrDefault();
                    if (valid == null)
                        return RedirectToAction("AddPerson", new {id = muestraOwner.nit});
                    else if(valid.phone == null)
                    {
                        valid.phone = "0000";
                        context.Entry(valid).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                using (dbModels context = new dbModels())
                {
                    ow = context.Owner.Where(x => x.idLine == idLine && x.idPerson == muestraOwner.nit.ToUpper().Trim()).FirstOrDefault();
                    if (ow != null) { TempData["MensajeCreateOwner"] = "Ya existe este dueño"; return View(); }
                    Owner owner = new Owner();
                    owner.idLine = idLine;
                    owner.idPerson = muestraOwner.nit.ToUpper().Trim();
                    owner.doc = muestraOwner.doc;
                    context.Owner.Add(owner);
                    context.SaveChanges();
                    return RedirectToAction("Index", new { id = Session["idLineCreateOwner"] });
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }


        // GET: Owner/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                Owner owner = new Owner();
                using (dbModels context = new dbModels())
                {
                    owner = context.Owner.Where(x => x.idOwner == id).FirstOrDefault();
                    context.Owner.Remove(owner);
                    context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult AddPerson(string id)
        {
            try
            {
                if (id == null) return RedirectToAction("Index", "Home");
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                Session["PersonOwner"] = id.ToUpper().Trim();
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
            
        }
        [HttpPost]
        public ActionResult AddPerson(string id,OwnerAuxiliar ownerAuxiliar)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                string nit = Session["PersonOwner"].ToString();
                Person personC = new Person();
                using (dbModels context = new dbModels()) personC = context.Person.Where(x => x.nit == nit).FirstOrDefault();
                DateTime hoy = DateTime.Now;
                Person person = new Person();
                person.phone = ownerAuxiliar.phone;
                person.nit = nit;
                person.name = ownerAuxiliar.name;
                person.lastname = ownerAuxiliar.lastname;
                person.dateRegister = hoy;
                person.idRole = 0;
                person.status = 1;
                using (dbModels context = new dbModels())
                {
                    if (personC == null) context.Person.Add(person);
                    else if (personC.status == 0) context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                Session["PersonOwner"] = nit;
                return RedirectToAction("Create");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult AutenticarPasosRol(int role)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            Login logi = new Login();
            int? roleSession = logi.GetRoleSessionFromCookie(Request);
            if (roleSession == null || roleSession != role) return RedirectToAction("Index", "Home");
            return null;
        }
    }
}
