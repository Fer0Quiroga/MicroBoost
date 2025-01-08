using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Windows.Documents;

namespace ProyectoControlLineaBus.Controllers
{
    public class CheckpointController : Controller
    {
        // GET: Checkpoint
        public ActionResult Index()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                Line line = new Line();
                List<Route> routes = new List<Route>();
                List<Checkpoint> checkpoints = new List<Checkpoint>();
                List<Route> rutasOrdenadas = new List<Route>();
                using (dbModels context = new dbModels()) routes = context.Route.Where(x => x.idLine == Linea).ToList();
                using (dbModels context = new dbModels()) checkpoints = context.Checkpoint.Where(x => x.idLine == Linea && x.status != 0).ToList();
                MuestraCheckpoint muestra = new MuestraCheckpoint();
                muestra.IntermediateCheckpoints = new List<string[]>();
                muestra.IntermediatePoints = new List<decimal[]>();
                muestra.checkpoints = new List<Checkpoint>();
                using (dbModels context = new dbModels()) line = context.Line.Where(x => x.nameLine == Linea).FirstOrDefault();
                if (routes != null)
                {
                    int count = 0;
                    rutasOrdenadas = routes.OrderByDescending(obj => (double)obj.latitud - (double)obj.logintud).ToList();
                    foreach (var item in rutasOrdenadas)
                        using (dbModels context = new dbModels())
                        {
                            Route route = context.Route.Where(x => x.idRoute == item.idRoute).FirstOrDefault();
                            if (count == 0) muestra.StartPoint = new decimal[] { (decimal)route.latitud, (decimal)route.logintud };
                            else if (count == rutasOrdenadas.Count - 1) muestra.EndPoint = new decimal[] { (decimal)route.latitud, (decimal)route.logintud };
                            else muestra.IntermediatePoints.Add(new decimal[] { (decimal)route.latitud, (decimal)route.logintud });
                            count++;
                        }
                }
                if (checkpoints != null)
                {
                    foreach (var item in checkpoints)
                    {
                        int count = 0;
                        using (dbModels context = new dbModels())
                        {
                            Checkpoint checkpoint = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                            if (count == 0 && muestra.StartPoint == null) muestra.StartPoint = new decimal[] { (decimal)checkpoint.latitud, (decimal)checkpoint.longitud };
                            muestra.IntermediateCheckpoints.Add(new string[] { checkpoint.latitud.ToString(), checkpoint.longitud.ToString(), checkpoint.description });
                            count++;
                        }
                    }
                }
                if (muestra.EndPoint == null) muestra.EndPoint = muestra.StartPoint;
                muestra.checkpoints = checkpoints;
                return View(muestra);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        // GET: Checkpoint/Create
        public ActionResult Create()
        {
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            return View();
        }

        // POST: Checkpoint/Create
        [HttpPost]
        public ActionResult Create(AddCheckpoint addCheckpoint)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                Checkpoint checkpoint = new Checkpoint();
                using (dbModels context = new dbModels())
                {
                    checkpoint.description = addCheckpoint.description;
                    if(addCheckpoint.final) checkpoint.status = 2;
                    else checkpoint.status = 1;
                    decimal latitud = decimal.Parse(addCheckpoint.latitud_longitud.Split(',')[0].ToString().Trim());
                    decimal longitud = decimal.Parse(addCheckpoint.latitud_longitud.Split(',')[1].ToString().Trim());
                    checkpoint.idLine = Linea;
                    checkpoint.latitud = latitud;
                    checkpoint.longitud = longitud;
                    checkpoint.numberRoute = 0;
                    context.Checkpoint.Add(checkpoint);
                    context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        // GET: Checkpoint/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Checkpoint checkpoint = new Checkpoint();
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.ToList().Where(x => x.idCheckpoint == id).FirstOrDefault();

                using (dbModels context = new dbModels())
                {
                    checkpoint.status = 0;
                    checkpoint.idEmployee = null;
                    context.Entry(checkpoint).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("Index","Route", new { numTypeRoute = checkpoint.numberRoute});
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult DeleteEmployee(int id, string employee)
        {
            try
            {
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Checkpoint checkpoint = new Checkpoint();
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.ToList().Where(x => x.idCheckpoint == id).FirstOrDefault();

                using (dbModels context = new dbModels())
                {
                    checkpoint.idEmployee = null;
                    context.Entry(checkpoint).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("ListChecksEmployee", "Checkpoint", new {id = employee});
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ListChecksEmployee(string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();

                List<MuestraListCheckpoint> muestra = new List<MuestraListCheckpoint>();
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                using (dbModels context = new dbModels()) {
                    checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == id && x.status != 0).ToList();
                    if (checkpoint == null) return RedirectToAction("Index", "Employee");
                    foreach (var item in checkpoint)
                    {
                        muestra.Add(new MuestraListCheckpoint(id, item.idCheckpoint, item.description+" Ruta: "+item.numberRoute));
                    }
                }
                ViewBag.ci = id;
                return View(muestra);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult AddCheckpoint(string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                Session["EmployeeAddCheckpoint"] = id;
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                List<Checkpoint> check = new List<Checkpoint>();
                using (dbModels context = new dbModels())
                {
                    check = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == id && x.status != 0).ToList();
                    checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status == 1).ToList();
                    var c = check.FirstOrDefault();
                    if (c.status == 2 || c.status == 3)
                    {
                        checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status == 2 || x.status == 3).ToList();
                    }
                    var noCoincidenLista1 = checkpoint.Where(p1 => check.All(p2 => p1.numberRoute != p2.numberRoute)).ToList();
                    if (checkpoint == null) return RedirectToAction("Index", "Employee");
                    var lista = noCoincidenLista1.Select(x => new {
                        idCheckpoint = x.idCheckpoint,
                        description = x.description + " Ruta: " + x.numberRoute
                    }).ToList();
                    ViewBag.idCheckpoint = new SelectList(lista, "idCheckpoint", "description");
                }
                ViewBag.ci = id;
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public ActionResult AddCheckpoint(EmployeeControl employeeControl)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string ControlEmployee = Session["EmployeeAddCheckpoint"].ToString();
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                using (dbModels context = new dbModels())
                    checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status != 0).ToList();
                if (checkpoint == null) return RedirectToAction("RegisterCheck", "Route");
                ViewBag.idCheckpoint = new SelectList(checkpoint, "idCheckpoint", "description");
                using (dbModels context = new dbModels())
                {
                    Checkpoint c = context.Checkpoint.Where(x => x.idCheckpoint == employeeControl.idCheckpoint).FirstOrDefault();
                    c.idEmployee = ControlEmployee;
                    context.Entry(c).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                Session["EmployeeAddCheckpoint"] = null;
                return RedirectToAction("ListChecksEmployee", "Checkpoint",new{id = ControlEmployee});
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
