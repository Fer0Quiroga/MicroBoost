using ProyectoControlLineaBus.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoControlLineaBus.Controllers
{
    public class ReportController : Controller
    {
        public ActionResult ReportDay()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                List<MuestraReport> report = new List<MuestraReport>();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                Employee employee = new Employee();
                Person person = new Person();
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea).ToList();
                List<TimeControl> time = new List<TimeControl>();
                using (dbModels context = new dbModels())
                    foreach (var item in checkpoint)
                    {
                        time = context.TimeControl.ToList().Where(x => x.idCheckpoint == item.idCheckpoint && DateTime.Parse(x.dateRegister.ToString()).ToString("yyyy-MM-dd") == boliviaTime.ToString("yyyy-MM-dd")).ToList();
                        foreach (var item2 in time)
                            report.Add(new MuestraReport(item.idCheckpoint, "", item2.idEmployee, item2.plate, DateTime.Parse(item2.dateRegister.ToString()), item.description));
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        item.name = person.name + " " + person.lastname;
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        employee = context.Employee.Where(x => x.idEmployee.ToString() == item.idEmployee.ToString()).FirstOrDefault();
                        item.idEmployee = employee.idEmployee;
                    }
                bool verificado = false;
                ViewBag.Hoy = boliviaTime.ToString();
                foreach (var item in report)
                    if (item.name.Trim() != "") verificado = true;
                if (!verificado) report.Add(new MuestraReport(1, "No se registraron tiempos el día de hoy", "-", "-", boliviaTime, "-"));
                return View(report);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult PrintedReportDay(string linea)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                List<MuestraReport> report = new List<MuestraReport>();
                Employee employee = new Employee();
                Person person = new Person();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea).ToList();
                List<TimeControl> time = new List<TimeControl>();
                using (dbModels context = new dbModels())
                    foreach (var item in checkpoint)
                    {
                        time = context.TimeControl.ToList().Where(x => x.idCheckpoint == item.idCheckpoint && DateTime.Parse(x.dateRegister.ToString()).ToString("yyyy-MM-dd") == boliviaTime.ToString("yyyy-MM-dd")).ToList();
                        foreach (var item2 in time)
                            report.Add(new MuestraReport(item.idCheckpoint, "", item2.idEmployee, item2.plate, DateTime.Parse(item2.dateRegister.ToString()), item.description));
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        item.name = person.name + " " + person.lastname;
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        employee = context.Employee.Where(x => x.idEmployee.ToString() == item.idEmployee.ToString()).FirstOrDefault();
                        item.idEmployee = employee.idEmployee;
                    }
                bool verificado = false;
                ViewBag.Hoy = boliviaTime.ToString();
                foreach (var item in report)
                    if (item.name.Trim() != "") verificado = true;
                if (!verificado) report.Add(new MuestraReport(1, "No se registraron tiempos el día de hoy", "-", "-", boliviaTime, "-"));
                return View(report);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportRequestDates()
        {
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            Session["start"] = null; Session["end"] = null;
            return View();
        }
        [HttpPost]
        public ActionResult ReportRequestDates(ReportDates report)
        {
            try
            {
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Session["start"] = report.start; Session["end"] = report.end;
                if (report.end < report.start)
                {
                    ViewBag.note = "Las fechas son incoherentes";
                    return View();
                }
                return RedirectToAction("ReportDates");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult ReportDates()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                if (Session["start"] == null || Session["end"] == null) return RedirectToAction("Index", "Home");
                DateTime start = DateTime.Parse(Session["start"].ToString());
                DateTime end = DateTime.Parse(Session["end"].ToString());
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                List<MuestraReport> report = new List<MuestraReport>();
                Employee employee = new Employee();
                Person person = new Person();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea).ToList();
                List<TimeControl> time = new List<TimeControl>();
                end = end.AddDays(1);
                using (dbModels context = new dbModels())
                    foreach (var item in checkpoint)
                    {
                        time = context.TimeControl.ToList().Where(x => x.idCheckpoint == item.idCheckpoint && x.dateRegister >= start && x.dateRegister <= end).ToList();
                        foreach (var item2 in time)
                            report.Add(new MuestraReport(item.idCheckpoint, "", item2.idEmployee, item2.plate, DateTime.Parse(item2.dateRegister.ToString()), item.description));
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        item.name = person.name + " " + person.lastname;
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        employee = context.Employee.Where(x => x.idEmployee == item.idEmployee).FirstOrDefault();
                        item.idEmployee = employee.idEmployee;
                    }
                bool verificado = false;
                ViewBag.Start = start.ToString("dd/MM/yyyy");
                ViewBag.End = end.ToString("dd/MM/yyyy");
                ViewBag.Hoy = boliviaTime.ToString();
                foreach (var item in report)
                    if (item.name.Trim() != "") verificado = true;
                if (!verificado) report.Add(new MuestraReport(1, "No se registraron tiempos entre esas fechas", "-", "-", boliviaTime, "-"));
                return View(report);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult PrintedReportDates(string linea, DateTime start , DateTime end)
        {
            try
            {
                Login logi = new Login();
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                List<MuestraReport> report = new List<MuestraReport>();
                Employee employee = new Employee();
                Person person = new Person();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea).ToList();
                List<TimeControl> time = new List<TimeControl>();
                end = end.AddDays(1);
                using (dbModels context = new dbModels())
                    foreach (var item in checkpoint)
                    {
                        time = context.TimeControl.ToList().Where(x => x.idCheckpoint == item.idCheckpoint && x.dateRegister >= start && x.dateRegister <= end).ToList();
                        foreach (var item2 in time)
                            report.Add(new MuestraReport(item.idCheckpoint, "", item2.idEmployee, item2.plate, DateTime.Parse(item2.dateRegister.ToString()), item.description));
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        item.name = person.name + " " + person.lastname;
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        employee = context.Employee.Where(x => x.idEmployee == item.idEmployee).FirstOrDefault();
                        item.idEmployee = employee.idEmployee;
                    }
                bool verificado = false;
                ViewBag.Start = start.ToString("dd/MM/yyyy");
                ViewBag.End = end.ToString("dd/MM/yyyy");
                ViewBag.Hoy = boliviaTime.ToString();
                foreach (var item in report)
                    if (item.name.Trim() != "") verificado = true;
                if (!verificado) report.Add(new MuestraReport(1, "No se registraron tiempos entre esas fechas", "-", "-", boliviaTime, "-"));
                return View(report);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportDayEmployee(string id)
        {
            try
            {
                if (id == null) return RedirectToAction("Index", "Home");
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                ViewBag.NIT = id;
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                List<MuestraReport> report = new List<MuestraReport>();
                Employee employee = new Employee();
                Person person = new Person();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea).ToList();
                List<TimeControl> time = new List<TimeControl>();
                using (dbModels context = new dbModels())
                    foreach (var item in checkpoint)
                    {
                        time = context.TimeControl.ToList().Where(x => x.idCheckpoint == item.idCheckpoint && x.idEmployee == id && DateTime.Parse(x.dateRegister.ToString()).ToString("yyyy-MM-dd") == boliviaTime.ToString("yyyy-MM-dd")).ToList();
                        foreach (var item2 in time)
                            report.Add(new MuestraReport(item.idCheckpoint, "", item2.idEmployee, item2.plate, DateTime.Parse(item2.dateRegister.ToString()), item.description));
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        item.name = person.name + " " + person.lastname;
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        employee = context.Employee.Where(x => x.idEmployee.ToString() == item.idEmployee.ToString()).FirstOrDefault();
                        item.idEmployee = employee.idEmployee;
                    }
                bool verificado = false;
                ViewBag.Hoy = boliviaTime.ToString();
                Session["NITEmployee"] = id;
                foreach (var item in report)
                    if (item.name.Trim() != "") verificado = true;
                if (!verificado) report.Add(new MuestraReport(1, "No se registraron tiempos el día de hoy", "-", "-", boliviaTime, "-"));
                return View(report);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult PrintedReportDayEmployee(string id, string linea)
        {
            try
            {
                Login logi = new Login();
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                ViewBag.NIT = id;
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                List<MuestraReport> report = new List<MuestraReport>();
                Employee employee = new Employee();
                Person person = new Person();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea).ToList();
                List<TimeControl> time = new List<TimeControl>();
                using (dbModels context = new dbModels())
                    foreach (var item in checkpoint)
                    {
                        time = context.TimeControl.ToList().Where(x => x.idCheckpoint == item.idCheckpoint && x.idEmployee == id && DateTime.Parse(x.dateRegister.ToString()).ToString("yyyy-MM-dd") == boliviaTime.ToString("yyyy-MM-dd")).ToList();
                        foreach (var item2 in time)
                            report.Add(new MuestraReport(item.idCheckpoint, "", item2.idEmployee, item2.plate, DateTime.Parse(item2.dateRegister.ToString()), item.description));
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        item.name = person.name + " " + person.lastname;
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        employee = context.Employee.Where(x => x.idEmployee.ToString() == item.idEmployee.ToString()).FirstOrDefault();
                        item.idEmployee = employee.idEmployee;
                    }
                bool verificado = false;
                ViewBag.Hoy = boliviaTime.ToString();
                foreach (var item in report)
                    if (item.name.Trim() != "") verificado = true;
                if (!verificado) report.Add(new MuestraReport(1, "No se registraron tiempos el día de hoy", "-", "-", boliviaTime, "-"));
                return View(report);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportRequestDatesEmployee()
        {
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            if (Session["NITEmployee"] == null) return RedirectToAction("Index", "Home");
            string id = Session["NITEmployee"].ToString();
            ViewBag.NIT = id;
            Session["idReport"] = id;
            Session["start"] = null; Session["end"] = null;
            return View();
        }
        [HttpPost]
        public ActionResult ReportRequestDatesEmployee(ReportDates report)
        {
            try
            {
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Session["start"] = report.start; Session["end"] = report.end;
                if (report.end < report.start)
                {
                    ViewBag.note = "Las fechas son incoherentes";
                    return View();
                }
                return RedirectToAction("ReportDatesEmployee");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult ReportDatesEmployee()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                if (Session["start"] == null || Session["end"] == null || Session["idReport"] == null) return RedirectToAction("Index", "Home");
                string id = Session["idReport"].ToString();
                DateTime start = DateTime.Parse(Session["start"].ToString());
                DateTime end = DateTime.Parse(Session["end"].ToString());
                ViewBag.NIT = id;
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                List<MuestraReport> report = new List<MuestraReport>();
                Employee employee = new Employee();
                Person person = new Person();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea).ToList();
                List<TimeControl> time = new List<TimeControl>();
                end = end.AddDays(1);
                using (dbModels context = new dbModels())
                    foreach (var item in checkpoint)
                    {
                        time = context.TimeControl.ToList().Where(x => x.idCheckpoint == item.idCheckpoint && x.idEmployee == id && x.dateRegister >= start && x.dateRegister <= end).ToList();
                        foreach (var item2 in time)
                            report.Add(new MuestraReport(item.idCheckpoint, "", item2.idEmployee, item2.plate, DateTime.Parse(item2.dateRegister.ToString()), item.description));
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        item.name = person.name + " " + person.lastname;
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        employee = context.Employee.Where(x => x.idEmployee == item.idEmployee).FirstOrDefault();
                        item.idEmployee = employee.idEmployee;
                    }
                bool verificado = false;
                ViewBag.Start = start.ToString("dd/MM/yyyy");
                ViewBag.End = end.ToString("dd/MM/yyyy");
                ViewBag.Hoy = boliviaTime.ToString();
                foreach (var item in report)
                    if (item.name.Trim() != "") verificado = true;
                if (!verificado) report.Add(new MuestraReport(1, "No se registraron tiempos entre esas fechas", "-", "-", boliviaTime, "-"));
                return View(report);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult PrintedReportDatesEmployee(string id, DateTime start, DateTime end, string linea)
        {
            try
            {
                Login logi = new Login();
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                ViewBag.NIT = id;
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                List<MuestraReport> report = new List<MuestraReport>();
                Employee employee = new Employee();
                Person person = new Person();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea).ToList();
                List<TimeControl> time = new List<TimeControl>();
                end = end.AddDays(1);
                using (dbModels context = new dbModels())
                    foreach (var item in checkpoint)
                    {
                        time = context.TimeControl.ToList().Where(x => x.idCheckpoint == item.idCheckpoint && x.idEmployee == id && x.dateRegister >= start && x.dateRegister <= end).ToList();
                        foreach (var item2 in time)
                            report.Add(new MuestraReport(item.idCheckpoint, "", item2.idEmployee, item2.plate, DateTime.Parse(item2.dateRegister.ToString()), item.description));
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        item.name = person.name + " " + person.lastname;
                    }
                using (dbModels context = new dbModels())
                    foreach (var item in report)
                    {
                        employee = context.Employee.Where(x => x.idEmployee == item.idEmployee).FirstOrDefault();
                        item.idEmployee = employee.idEmployee;
                    }
                bool verificado = false;
                ViewBag.Start = start.ToString("dd/MM/yyyy");
                ViewBag.End = end.ToString("dd/MM/yyyy");
                ViewBag.Hoy = boliviaTime.ToString();
                foreach (var item in report)
                    if (item.name.Trim() != "") verificado = true;
                if (!verificado) report.Add(new MuestraReport(1, "No se registraron tiempos entre esas fechas", "-", "-", boliviaTime, "-"));
                return View(report);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult PrintReportDay()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            string Linea = logi.GetLineaFromCookie(Request).ToString();
            TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
            DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
            return new ActionAsPdf("PrintedReportDay", new {linea = Linea.ToString()}) { FileName = "ReporteDia" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
        }
        public ActionResult PrintReportDayEmployee()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            string Linea = logi.GetLineaFromCookie(Request).ToString();
            if (Session["NITEmployee"] == null) return RedirectToAction("Index", "Home");
            TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
            DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
            return new ActionAsPdf("PrintedReportDayEmployee", new {id = Session["NITEmployee"].ToString(), linea = Linea.ToString() }) { FileName = "ReporteDiaEmpleado-" + Session["NITEmployee"].ToString() + "-" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
        }
        public ActionResult PrintReportDates()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            string Linea = logi.GetLineaFromCookie(Request).ToString();
            if (Session["start"] == null || Session["end"] == null) return RedirectToAction("Index", "Home");
            TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
            DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
            return new ActionAsPdf("PrintedReportDates",new {linea = Linea, start = DateTime.Parse(Session["start"].ToString()), end = DateTime.Parse(Session["end"].ToString()) }) { FileName = "ReporteDiaEmpleado-" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
        }
        public ActionResult PrintReportDatesEmployee()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            string Linea = logi.GetLineaFromCookie(Request).ToString();
            if (Session["start"] == null || Session["end"] == null) return RedirectToAction("Index", "Home");
            TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
            DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
            return new ActionAsPdf("PrintedReportDatesEmployee", new {id = Session["NITEmployee"].ToString(), start = DateTime.Parse(Session["start"].ToString()), end = DateTime.Parse(Session["end"].ToString()), linea = Linea }) { FileName = "ReporteDiaEmpleado-" + Session["NITEmployee"].ToString() + "-" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
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
