using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;

namespace ProyectoControlLineaBus.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Views()
        {
            TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");

            DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);


            return View();
        }

        public ActionResult Index()
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var cookieData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ticket.UserData);

                ViewBag.RoleSession = cookieData.RoleId;
                ViewBag.Employee = cookieData.EmployeeId;
                ViewBag.Linea = cookieData.Linea;
                switch((int)ViewBag.RoleSession)
                {
                    case 1:
                        return RedirectToAction("Index", "Chofer");
                    case 2:
                        return RedirectToAction("OptionsPrincipalControl", "TimeControl");
                    case 3:
                        return RedirectToAction("HomeLine", "Line");
                    case 4:
                        return RedirectToAction("Index", "Line");
                    default:
                        break;
                }
            }
            return View();
        }
        [HttpPost]
        public ActionResult Index(Place placeinfo)
        {
            return RedirectToAction("MapSearch", new { info = placeinfo.description });
        }

        public ActionResult MapSearch(string info)
        {
            try
            {
                if(info==null) return RedirectToAction("Index", "Home");
                List<Place> place = new List<Place>();
                string inform= info.ToLower().Trim();
                List<Line> lines = new List<Line>();
                List<Route> routes = new List<Route>();
                using (dbModels context = new dbModels()) lines = context.Line.Where(x => x.nameLine.ToLower().Contains(inform)).ToList(); if (lines == null) lines = new List<Line>();
                using (dbModels context = new dbModels()) place = context.Place.Where(x => x.description.ToLower().Contains(inform)).ToList();
                if (place != null)
                {
                    using (dbModels context = new dbModels())

                        foreach (var item in place)
                        {
                            List<Route> r = context.Route.Where(x => x.idPlace == item.idPlace).ToList();
                            foreach (var item2 in r)
                            {
                                Route r2 = context.Route.Where(x => x.idRoute == item2.idRoute).FirstOrDefault();
                                if (r2 != null)
                                    routes.Add(r2);
                            }
                        }
                    if (routes != null)
                        using (dbModels context = new dbModels())
                        {
                            foreach (var item in routes)
                            {
                                bool valid = true;
                                Line line = context.Line.Where(x => x.nameLine == item.idLine).FirstOrDefault();
                                foreach (var item2 in lines) { if (item2.nameLine == line.nameLine) valid = false; }
                                if (line != null && valid) lines.Add(line);
                            }
                        }
                }
                if (lines != null && lines.Count > 0) return View(lines);
                else { Session["MensajeError"] = "1"; return RedirectToAction("Index"); }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult ViewLine(string nameLine)
        {
            try
            {
                if (nameLine == null) return RedirectToAction("Index", "Home");
                string Linea = nameLine;
                Line line = new Line();
                List<Route> routes = new List<Route>();
                List<Checkpoint> checkpoints = new List<Checkpoint>();
                List<Route> rutasOrdenadas = new List<Route>();
                using (dbModels context = new dbModels()) routes = context.Route.Where(x => x.idLine == Linea).ToList();
                using (dbModels context = new dbModels()) checkpoints = context.Checkpoint.Where(x => x.idLine == Linea && x.status == 1).ToList();
                MuestraSearchMap muestra = new MuestraSearchMap();
                muestra.IntermediateCheckpoints = new List<string[]>();
                muestra.IntermediatePoints = new List<decimal[]>();
                muestra.NameCheckpoint = new List<string[]>();
                List<UltCheckpoint_Result> ultCheckpoint_Result = new List<UltCheckpoint_Result>();
                muestra.ultCheckpoints = new List<CheckpointInSearchMap>();
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
                    foreach (var item in checkpoints)
                    {
                        Checkpoint check = new Checkpoint();
                        if (item.status == 1)
                            using (dbModels context = new dbModels())
                            {
                                check = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                if (check != null) muestra.NameCheckpoint.Add(new string[] { item.idCheckpoint.ToString(), check.description });
                            }
                    }
                }
                using (dbModels context = new dbModels()) ultCheckpoint_Result = context.UltCheckpoint(Linea).ToList();
                if (ultCheckpoint_Result != null)
                {
                    foreach (var item in ultCheckpoint_Result)
                    {
                        DateTime hoy = DateTime.Now;
                        TimeSpan diferencia = (DateTime)item.dateRegister - hoy;
                        double minutosDiferencia = Math.Floor(diferencia.TotalMinutes);
                        if (minutosDiferencia < 0) minutosDiferencia = minutosDiferencia * (-1);
                        long date = (long)minutosDiferencia;
                        muestra.ultCheckpoints.Add(new CheckpointInSearchMap(item.idEmployee, (int)item.idCheckpoint, (DateTime)item.dateRegister, (int)item.status,item.name.ToString(), date));
                    }
                }
                if (muestra.EndPoint == null) muestra.EndPoint = muestra.StartPoint;
                muestra.price = decimal.Parse(line.price.ToString());
                muestra.nameLine = line.nameLine;
                muestra.description = line.description;
                return View(muestra);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult CloseSession()
        {
            FormsAuthentication.SignOut();

            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
            {
                Expires = DateTime.Now.AddDays(-1), 
                HttpOnly = true
            };
            Response.Cookies.Add(authCookie);

            return RedirectToAction("Index", "Home");
        }
        public ActionResult Error()
        {
            return View();
        }
        [ChildActionOnly]
        public ActionResult GetMenuData()
        {
            LayoutViewModel layoutModel = new LayoutViewModel();
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                if (ticket != null && !ticket.Expired)
                {
                    var cookieData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ticket.UserData);
                    layoutModel.RoleSession = cookieData.RoleId;
                    layoutModel.EmployeeId = cookieData.EmployeeId;
                    layoutModel.Linea = cookieData.Linea;
                    return PartialView("_MenuPartial", layoutModel);
                }
            }
            return PartialView("_LoggedOutMenuPartial");
        }
        public ActionResult QRActions(string placa)
        {
            if (placa == null) return RedirectToAction("Index", "Home");
            ViewBag.placa = placa;
            return View();
        }

        public ActionResult SubmitCalification(string id, int calificacion)
        {
            try
            {
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                Employee employee = new Employee();
                using (dbModels context = new dbModels())
                {
                    employee = context.Employee.Where(x => x.plate == id.Trim()).FirstOrDefault();
                    if (employee == null)
                        return RedirectToAction("Error", "Home");

                    var star = new Star
                    {
                        numStar = calificacion,
                        idEmployee = employee.idEmployee,
                        dateRegister = boliviaTime
                    };
                    context.Star.Add(star);
                    context.SaveChanges();
                }
                return RedirectToAction("QrActions", "Home", new { placa = employee.plate });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult SubmitReport(string id, string reason)
        {
            try
            {
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                Employee employee = new Employee();
                using (dbModels context = new dbModels())
                {
                    employee = context.Employee.Where(x => x.plate == id.Trim()).FirstOrDefault();
                    if (employee == null)
                        return RedirectToAction("Error", "Home");

                    var report = new Report
                    {
                        idEmployee = employee.idEmployee,
                        description = reason,
                        dateRegister = boliviaTime
                    };
                    context.Report.Add(report);
                    context.SaveChanges();
                }
                return RedirectToAction("QrActions", "Home", new { placa = employee.plate });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }

}