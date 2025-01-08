using ProyectoControlLineaBus.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoControlLineaBus.Controllers
{
    public class SanctionController : Controller
    {
        // GET: Sanction
        public ActionResult Index()
        {
            try
            {

                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                List <Sanction> sanction = new List <Sanction>();
                using (dbModels context = new dbModels())
                {
                    sanction = context.Sanction.Where(x=>x.idLine == Line && x.status == 1).ToList();
                }
                return View(sanction);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult Create()
        {
            try
            {

                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: Sanction/Create
        [HttpPost]
        public ActionResult Create(Sanciones sanciones)
        {
            try
            {

                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                Sanction sanction = new Sanction();
                using (dbModels context = new dbModels())
                {
                    sanction.title = sanciones.title;
                    sanction.description = sanciones.description;
                    sanction.idLine = Line;
                    sanction.status = 1;
                    context.Sanction.Add(sanction);
                    context.SaveChanges();
                }
                return RedirectToAction("Index", "Sanction");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

       

        public ActionResult Delete(int id)
        {
            try
            {

                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                Sanction sanction = new Sanction();
                using (dbModels context = new dbModels())
                {
                    sanction = context.Sanction.Where(x => x.idSanction == id).FirstOrDefault();
                    sanction.status = 0;
                    context.Entry(sanction).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("Index", "Sanction");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult VerSanciones(string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<SancionesEmpleadoMostrar> sancionesEmpleadoMostrars = new List<SancionesEmpleadoMostrar>();
                using (dbModels context = new dbModels())
                {
                    List<Sanction> sanctions = new List<Sanction>();
                    sanctions = context.Sanction.Where(x => x.idLine == Line && x.status == 1).ToList();
                    if (sanctions.Count<=0)
                    {
                        return RedirectToAction("Create", "Sanction");
                    }
                    ViewBag.SancionesDisponibles = sanctions;
                }
                List<SanctionEmployee> sanction = new List<SanctionEmployee>();
                using (dbModels context = new dbModels())
                {
                    sanction = context.SanctionEmployee.Where(x => x.idEmployee == id).ToList();
                    if (sanction.Count > 0)
                    {
                        foreach (var item in sanction)
                        {
                            var titulo = context.Sanction.Where(x => x.idSanction == item.idSanction && x.status == 1).FirstOrDefault();
                            if(titulo != null)
                            sancionesEmpleadoMostrars.Add(new SancionesEmpleadoMostrar(id, titulo.title,titulo.description, DateTime.Parse(item.dateRegister.ToString())));
                        }
                    }
                }
                ViewBag.ci = id;
                return View(sancionesEmpleadoMostrars);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult CreateEmployeeSanction(string id,int sanctione)
        {
            try
            {

                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                string Line = logi.GetLineaFromCookie(Request).ToString();
                SanctionEmployee sanction = new SanctionEmployee();
                using (dbModels context = new dbModels())
                {
                    sanction.idEmployee = id;
                    sanction.idSanction = sanctione;
                    sanction.dateRegister = boliviaTime;
                    context.SanctionEmployee.Add(sanction);
                    context.SaveChanges();
                }
                return RedirectToAction("VerSanciones", "Sanction", new {id = id});
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult VerSancionesEscogido(string id, string selectedYear)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<SancionesEmpleadoMostrar> sancionesEmpleadoMostrars = new List<SancionesEmpleadoMostrar>();
                List<SanctionEmployee> sanction = new List<SanctionEmployee>();
                using (dbModels context = new dbModels())
                {
                    var sanctione = context.SanctionEmployee.Where(x => x.idEmployee == id).ToList();
                    if (selectedYear != "Todos los años")
                    {
                        sanction = sanctione.Where(x => DateTime.Parse(x.dateRegister.ToString()).Year == int.Parse(selectedYear)).ToList();
                    }
                    else sanction = sanctione;
                    if (sanction.Count > 0)
                    {
                        foreach (var item in sanction)
                        {
                            var titulo = context.Sanction.Where(x => x.idSanction == item.idSanction && x.status == 1).FirstOrDefault();
                            if (titulo != null)
                                sancionesEmpleadoMostrars.Add(new SancionesEmpleadoMostrar(id, titulo.title,titulo.description, DateTime.Parse(item.dateRegister.ToString())));
                        }
                    }
                }
                ViewBag.ci = id;
                return View(sancionesEmpleadoMostrars);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult PrintedReportFallas(string id, string selectedYear)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                return new ActionAsPdf("VerSancionesEscogido", new { id = id, selectedYear = selectedYear }) { FileName = id + "-" + selectedYear + "-" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
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
