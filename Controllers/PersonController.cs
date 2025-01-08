using Microsoft.Ajax.Utilities;
using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;

namespace ProyectoControlLineaBus.Controllers
{
    public class PersonController : Controller
    {
        // GET: Person
        public ActionResult Index()
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                List<Person> list = new List<Person>();
                using (dbModels context = new dbModels())
                {
                    return View(context.Person.ToList().Where(x => x.status == 1 && x.phone != null));
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Person/Edit/5
        public ActionResult Edit(string id)
        {
            try
            {
                if (id == null) return RedirectToAction("Index", "Home");
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                using (dbModels context = new dbModels())
                {
                    Person person = context.Person.Where(x => x.nit == id).FirstOrDefault();
                    Session["PersonEdit"] = person;
                    OwnerAuxiliar ow = new OwnerAuxiliar(person.nit,"",person.name,person.lastname,person.phone);
                    return View(ow);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Person/Edit/5
        [HttpPost]
        public ActionResult Edit(string id, OwnerAuxiliar person)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                Person p = (Person)Session["PersonEdit"];
                Person valid = new Person();
                using (dbModels context = new dbModels())
                {
                    valid = context.Person.Where(x => x.nit == person.nit && x.status == 1).FirstOrDefault();
                    if (valid == null || p.nit == person.nit)
                    {
                        p.name = person.name;
                        p.lastname = person.lastname;
                        p.phone = person.phone;
                        p.status = 1;
                        context.Entry(p).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                    else TempData["MensajeCrear"] = "Ya existe una persona con ese nit";
                }
                Session["PersonEdit"] = null;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Person/Delete/5
        public ActionResult Delete(string id)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                Employee employee = new Employee();
                Person person = new Person();
                List<Owner> ownerList = new List<Owner>();
                using (dbModels context = new dbModels()) ownerList = context.Owner.Where(x => x.idPerson == id).ToList();
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit == id).FirstOrDefault();
                using (dbModels context = new dbModels()) employee = context.Employee.Where(x => x.idEmployee == id).FirstOrDefault();
                if (employee == null) person.status = 0;
                person.phone = null;
                using (dbModels context = new dbModels())
                {
                    context.Entry(person).State = System.Data.Entity.EntityState.Modified; ;
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    foreach (var item in ownerList)
                    {
                        Owner ow = context.Owner.Where(x => x.idOwner == item.idOwner).FirstOrDefault();
                        context.Owner.Remove(ow);
                    }
                    context.SaveChanges();
                }
                return RedirectToAction("Index", "Person");
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
