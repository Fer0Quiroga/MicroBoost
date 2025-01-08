using ProyectoControlLineaBus.Models;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Web;
using System.Web.Mvc;

namespace ProyectoControlLineaBus.Controllers
{
    public class NewReportController : Controller
    {

        public ActionResult ReportFallasMecanicas(string plate)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<MostrarFallasMecanicas> fallas = new List<MostrarFallasMecanicas>();
                using (dbModels context = new dbModels())
                {
                    var failures = context.MechanicalFailures.Where(x => x.plate == plate).ToList();
                    if (failures.Count > 0)
                    {
                        foreach (var item in failures)
                        {
                            fallas.Add(new MostrarFallasMecanicas(item.plate, item.description, DateTime.Parse(item.dateRegister.ToString())));
                        }
                    }
                }

                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, boliviaTimeZone);
                ViewBag.plate = plate;
                ViewBag.CurrentYear = boliviaTime.Year;
                return View(fallas);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportFallasMecanicasEscogidos(int year, string plate)
        {
            try
            {
                List<MostrarFallasMecanicas> fallas = new List<MostrarFallasMecanicas>();

                using (dbModels context = new dbModels())
                {
                    var failuress = context.MechanicalFailures
                                          .Where(x => x.plate == plate)
                                          .ToList();
                    var failures = failuress.Where(x => x.plate == plate && DateTime.Parse(x.dateRegister.ToString()).Year == year).ToList();
                    foreach (var item in failures)
                    {
                        fallas.Add(new MostrarFallasMecanicas(item.plate, item.description, DateTime.Parse(item.dateRegister.ToString())));
                    }
                    ViewBag.plate = plate;
                    ViewBag.CurrentYear = year;
                }

                return View(fallas);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }

        public ActionResult PrintedReportFallas(int year,string plate)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                return new ActionAsPdf("ReportFallasMecanicasEscogidos", new {year = year, plate = plate}) { FileName = plate + "-"+year+"-" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportChofer(string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                List<MostrarReportesChofer> reportesChofers = new List<MostrarReportesChofer>();
                using (dbModels context = new dbModels())
                {
                    var report = context.Report.Where(x => x.idEmployee == id).ToList();
                    var person = context.Person.Where(x => x.nit == id).FirstOrDefault();

                    if (report.Count > 0 && person != null)
                    {
                        reportesChofers = report
                            .GroupBy(r => new { r.description, Month = DateTime.Parse(r.dateRegister.ToString()).Month, Year = DateTime.Parse(r.dateRegister.ToString()).Year })
                            .Select(g => new MostrarReportesChofer(
                                g.Key.description,
                                g.Key.Month,
                                g.Key.Year,
                                g.Count()
                            )).ToList();
                    }
                    ViewBag.Name = person != null ? person.name + " " + person.lastname : "Desconocido";
                }

                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, boliviaTimeZone);
                ViewBag.Id = id;
                ViewBag.CurrentYear = boliviaTime.Year;
                ViewBag.CurrentMonth = boliviaTime.Month;

                return View(reportesChofers);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }


        public ActionResult ReportChoferEscogido(int year, int month, string id)
        {

            try
            {
                List<MostrarReportesChofer> reportesFiltrados = new List<MostrarReportesChofer>();

                using (dbModels context = new dbModels())
                {
                    var report = context.Report.Where(x => x.idEmployee == id).ToList();
                    var person = context.Person.Where(x => x.nit == id).FirstOrDefault();
                    var reportFiltrado = report
                        .Where(x => x.dateRegister.HasValue && x.dateRegister.Value.Year == year && x.dateRegister.Value.Month == month)
                        .GroupBy(r => new { r.description, Month = r.dateRegister.Value.Month, Year = r.dateRegister.Value.Year })
                        .Select(g => new MostrarReportesChofer(
                            g.Key.description,
                            g.Key.Month,
                            g.Key.Year,
                            g.Count()
                        )).ToList();

                    reportesFiltrados.AddRange(reportFiltrado);
                    ViewBag.name = person.name + " " + person.lastname;
                }

                ViewBag.Year = year;
                ViewBag.Month = month;

                return View(reportesFiltrados);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }
        public ActionResult PrintedReportChofer(int year, int month, string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                return new ActionAsPdf("ReportChoferEscogido", new { year = year, month = month,id = id }) { FileName = id + "-"+month+"/" + year + "-" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
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
        public ActionResult ReportChoferView()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(1);
                if (authResult != null) return authResult;

                string id = logi.GetEmployeeIdFromCookie(Request);
                List<MostrarReportesChofer> reportesChofers = new List<MostrarReportesChofer>();
                using (dbModels context = new dbModels())
                {
                    var report = context.Report.Where(x => x.idEmployee == id).ToList();
                    var person = context.Person.Where(x => x.nit == id).FirstOrDefault();

                    if (report.Count > 0 && person != null)
                    {
                        reportesChofers = report
                            .GroupBy(r => new { r.description, Month = DateTime.Parse(r.dateRegister.ToString()).Month, Year = DateTime.Parse(r.dateRegister.ToString()).Year })
                            .Select(g => new MostrarReportesChofer(
                                g.Key.description,
                                g.Key.Month,
                                g.Key.Year,
                                g.Count()
                            )).ToList();
                    }
                    ViewBag.Name = person != null ? person.name + " " + person.lastname : "Desconocido";
                }

                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, boliviaTimeZone);
                ViewBag.Id = id;
                ViewBag.CurrentYear = boliviaTime.Year;
                ViewBag.CurrentMonth = boliviaTime.Month;

                return View(reportesChofers);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportRankingChoferes()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<Employee> employees = new List<Employee>();
                List<ChoferRanking> listRank = new List<ChoferRanking>();
                List<ChoferRanking> listRankMesAnterior = new List<ChoferRanking>();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                var primerDiaBoliviaTime = new DateTime(boliviaTime.Year, boliviaTime.Month, 1);
                var primerDiaBoliviaTimeMesAnterior = primerDiaBoliviaTime.AddMonths(-1);
                using (dbModels context = new dbModels())
                {
                    employees = context.Employee.Where(e => e.idLine == Linea && e.status == 1 && e.plate != null).ToList();
                    if (employees.Count > 0)
                    {
                        foreach (var employee in employees)
                        {
                            var check1 = context.Checkpoint.Where(x => x.idLine == Linea && x.status == 2).FirstOrDefault();
                            var check2 = context.Checkpoint.Where(x => x.idLine == Linea && x.status == 3).FirstOrDefault();
                            //var filterControls = context.TimeControl
                            //.Where(x => x.idEmployee == employee.idEmployee &&
                            //           ((x.idCheckpoint == check1.idCheckpoint && (x.enable == 1 || x.enable == 3)) ||
                            //            (x.idCheckpoint == check2.idCheckpoint && (x.enable == 2 || x.enable == 4))) &&
                            //           x.dateRegister >= primerDiaBoliviaTimeMesAnterior &&
                            //           x.dateRegister < primerDiaBoliviaTime.AddMonths(1))
                            //.ToList();

                            //if (filterControls.Count > 0)
                            //{
                            //  
                            //}
                            var person = context.Person.Where(x => x.nit == employee.idEmployee).FirstOrDefault();
                            var timeControls = context.TimeControl.Where(x => x.idEmployee == employee.idEmployee && ((x.idCheckpoint == check1.idCheckpoint && (x.enable == 1 || x.enable == 3)) || (x.idCheckpoint == check2.idCheckpoint && (x.enable == 2 || x.enable == 4)))).ToList();
                            int vueltasMes = timeControls.Where(x => x.dateRegister >= primerDiaBoliviaTime && x.dateRegister < primerDiaBoliviaTime.AddMonths(1)).Count();
                            int vueltasMesAmterior = timeControls.Where(x => x.dateRegister >= primerDiaBoliviaTimeMesAnterior && x.dateRegister < primerDiaBoliviaTime).Count();
                            listRank.Add(new ChoferRanking(person.name + " " + person.lastname, vueltasMes, 1));
                            listRankMesAnterior.Add(new ChoferRanking(person.name + " " + person.lastname, vueltasMesAmterior, 2));
                        }
                        listRank = listRank.OrderByDescending(x => x.vueltas).Take(10).ToList();
                        listRankMesAnterior = listRankMesAnterior.OrderByDescending(x => x.vueltas).Take(10).ToList();
                        listRank = listRank.Concat(listRankMesAnterior).ToList();
                    }
                }
                return View(listRank);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }
        public ActionResult ReportIncidenciasChofer(string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                Employee employee = new Employee();
                List<ChoferObservaciones> listObservaciones = new List<ChoferObservaciones>();
                MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                var primerDiaBoliviaTime = new DateTime(boliviaTime.Year, boliviaTime.Month, 1);
                var primerDiaBoliviaTimeMesAnterior = primerDiaBoliviaTime.AddMonths(-1);
                using (dbModels context = new dbModels())
                {
                    employee = context.Employee.Where(e => e.idLine == Linea && e.status == 1 && e.plate != null && e.idEmployee == id).FirstOrDefault();
                    var person = context.Person.Where(x => x.nit == id).FirstOrDefault();
                    var timeControlsFilter = context.TimeControl.Where(x => x.idEmployee == employee.idEmployee).GroupBy(x => x.numTicket).Select(g => g.FirstOrDefault()).ToList();
                    var groupedData = timeControlsFilter
                        .GroupBy(x => x.numTicket)
                        .Select(g => g.First())
                        .ToList();

                    foreach (var t in groupedData)
                    {
                        DateTime final = boliviaTime;
                        var timeControls = context.TimeControl.Where(x => x.numTicket ==   t.numTicket && x.idEmployee == t.idEmployee).ToList();
                        var timess = timeControls.FirstOrDefault();
                        var checksver = context.Checkpoint.Where(x => x.idCheckpoint == timess.idCheckpoint).FirstOrDefault();
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Linea && x.status != 0 && x.numberRoute == checksver.numberRoute).ToList();
                        if (timeControls.Any())
                        {
                            int statusPrimera = 2;
                            if (t.enable == 2 || t.enable == 4) statusPrimera = 3;
                            Checkpoint primerCheckpoint = checkpoints.FirstOrDefault(r => r.status == statusPrimera);
                            List<Checkpoint> checkpointsorder = new List<Checkpoint>();
                            if (primerCheckpoint != null)
                            {
                                checkpointsorder = checkpoints
                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                        (decimal)primerCheckpoint.latitud,
                                        (decimal)primerCheckpoint.longitud,
                                        route.latitud,
                                        route.longitud))
                                    .ToList();
                            }
                            timeControls = timeControls.OrderBy(x => x.dateRegister).ToList();
                            foreach (var item in timeControls)
                            {
                                if (item.dateRegister == null)
                                {
                                    var check = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                    listObservaciones.Add(new ChoferObservaciones("El conductor no llego a la " + check.description+ " RUTA "+check.numberRoute,DateTime.Parse(t.dateRegister.ToString())));
                                }
                                if (item.enable > 2 && item.dateRegister != null)
                                {
                                    var check = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                    var timeControlChofer = context.TimeControl.Where(x => x.idCheckpoint == check.idCheckpoint).ToList();
                                    var ticketCercano = timeControlChofer.Where(x => x.numTicket > t.numTicket).OrderBy(x => x.numTicket).FirstOrDefault();
                                    var timeControlChoferLePaso = context.TimeControl.Where(x => x.numTicket == ticketCercano.numTicket && x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                    var chofer = context.Person.Where(x => x.nit == timeControlChoferLePaso.idEmployee).FirstOrDefault();
                                    listObservaciones.Add(new ChoferObservaciones("Al conductor le pasó " + chofer.name + " " + chofer.lastname + " en " + check.description+ " RUTA "+check.numberRoute, DateTime.Parse(t.dateRegister.ToString())));
                                }
                            }
                            List<TimeControl> orderTimeControl = new List<TimeControl>();
                            foreach (var item in checkpointsorder)
                            {
                                TimeControl timeControlOrder = timeControls.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                orderTimeControl.Add(timeControlOrder);
                            }
                            if (t.enable == 1 || t.enable == 3)
                            {
                                for (int i = 0; i < orderTimeControl.Count - 1; i++)
                                {
                                    if (orderTimeControl[i + 1].dateRegister.HasValue && orderTimeControl[i].dateRegister.HasValue)
                                    {
                                        var checkpointPrimer = checkpoints.Where(x => x.status == 2 && x.idLine == Linea).FirstOrDefault();
                                        var timeControlInicio = timeControls.Where(x => x.idCheckpoint == checkpointPrimer.idCheckpoint && t.numTicket == x.numTicket).FirstOrDefault();
                                        Checkpoint checkpoint1 = new Checkpoint();
                                        double minutosParaParada = 0;
                                        double medicionTime = (orderTimeControl[i + 1].dateRegister.Value - orderTimeControl[i].dateRegister.Value).TotalMinutes;
                                        checkpoint1 = checkpointsorder.Where(x => x.idCheckpoint == orderTimeControl[i + 1].idCheckpoint).FirstOrDefault();
                                        TimeSpan timeSpan = new TimeSpan();
                                        if (DateTime.Parse(timeControlInicio.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControlInicio.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                        {
                                            string[] partes = checkpoint1.minArrivalMarket.Trim().Split(':');
                                            minutosParaParada = (double.Parse(partes[0]) * 60) + double.Parse(partes[1]);
                                        }
                                        else
                                        {
                                            string[] partes = checkpoint1.minArrival.Trim().Split(':');
                                            minutosParaParada = (double.Parse(partes[0]) * 60) + double.Parse(partes[1]);
                                        }
                                        double diferenciaMinutos = medicionTime - minutosParaParada;
                                        int diferencia = (int)Math.Floor(diferenciaMinutos);
                                        var check = checkpoints.Where(x => x.idCheckpoint == orderTimeControl[i + 1].idCheckpoint).FirstOrDefault();
                                        if (minutosParaParada < medicionTime)
                                        {
                                            listObservaciones.Add(new ChoferObservaciones("El conductor se retrazó en " + check.description + " con " + diferencia + "min. RUTA "+check.numberRoute,DateTime.Parse(t.dateRegister.ToString())));
                                        }
                                    }
                                }
                            }
                            else if (t.enable == 2 || t.enable == 4)
                            {
                                for (int i = 0; i < checkpoints.Count - 1; i++)
                                {
                                    if (orderTimeControl[i].dateRegister.HasValue && orderTimeControl[i + 1].dateRegister.HasValue)
                                    {
                                        var checkpointPrimer = checkpoints.Where(x => x.status == 3 && x.idLine == Linea).FirstOrDefault();
                                        var timeControlInicio = timeControls.Where(x => x.idCheckpoint == checkpointPrimer.idCheckpoint && t.numTicket == x.numTicket).FirstOrDefault();
                                        Checkpoint checkpoint1 = new Checkpoint();
                                        double minutosParaParada = 0;
                                        double medicionTime = (orderTimeControl[i + 1].dateRegister.Value - orderTimeControl[i].dateRegister.Value).TotalMinutes;
                                        checkpoint1 = checkpoints.Where(x => x.idCheckpoint == orderTimeControl[i].idCheckpoint).FirstOrDefault();
                                        TimeSpan timeSpan = new TimeSpan();
                                        if (DateTime.Parse(timeControlInicio.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControlInicio.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                            timeSpan = TimeSpan.Parse(checkpoint1.minArrivalMarket);
                                        else timeSpan = TimeSpan.Parse(checkpoint1.minArrival);
                                        minutosParaParada = timeSpan.TotalMinutes;
                                        double diferenciaMinutos = medicionTime - minutosParaParada;
                                        int diferencia = (int)Math.Floor(diferenciaMinutos);
                                        var check = checkpoints.Where(x => x.idCheckpoint == orderTimeControl[i + 1].idCheckpoint).FirstOrDefault();
                                        if (minutosParaParada < medicionTime)
                                        {
                                            listObservaciones.Add(new ChoferObservaciones("El conductor se retrazó en " + check.description + " con " + diferencia + "min. RUTA "+check.numberRoute,DateTime.Parse(t.dateRegister.ToString())));
                                        }
                                    }
                                }
                            }
                            if (t.numTicket != null) t.numTicket = 0;
                            if (t.dateRegister != null) final = DateTime.Parse(t.dateRegister.ToString());
                        }
                    }
                }
                ViewBag.name = id;
                return View(listObservaciones);

            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportIncidenciasChoferImprimir(string id, int mes, int anio)
        {
            try
            {
                DateTime primerDiaDelMes = new DateTime(anio, mes, 1);
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                Employee employee = new Employee();
                List<ChoferObservaciones> listObservaciones = new List<ChoferObservaciones>();
                MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                using (dbModels context = new dbModels())
                {
                    employee = context.Employee.Where(e => e.idLine == Linea && e.status == 1 && e.plate != null && e.idEmployee == id).FirstOrDefault();
                    var person = context.Person.Where(x => x.nit == id).FirstOrDefault();
                    var timeControlsFilter = context.TimeControl.Where(x => x.idEmployee == employee.idEmployee).GroupBy(x => x.numTicket).Select(g => g.FirstOrDefault()).ToList();
                    var groupedData = timeControlsFilter
                       .GroupBy(x => x.numTicket)
                       .Select(g => g.First())
                       .ToList();

                    foreach (var t in groupedData)
                    {
                        DateTime final = boliviaTime;
                        var timeControls = context.TimeControl.Where(x => x.numTicket == t.numTicket && x.idEmployee == t.idEmployee).ToList();
                        var timess = timeControls.FirstOrDefault();
                        var checksver = context.Checkpoint.Where(x => x.idCheckpoint == timess.idCheckpoint).FirstOrDefault();
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Linea && x.status != 0 && x.numberRoute == checksver.numberRoute).ToList();
                        if (timeControls.Any())
                        {
                            int statusPrimera = 2;
                            if (t.enable == 2 || t.enable == 4) statusPrimera = 3;
                            Checkpoint primerCheckpoint = checkpoints.FirstOrDefault(r => r.status == statusPrimera);
                            List<Checkpoint> checkpointsorder = new List<Checkpoint>();
                            if (primerCheckpoint != null)
                            {
                                checkpointsorder = checkpoints
                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                        (decimal)primerCheckpoint.latitud,
                                        (decimal)primerCheckpoint.longitud,
                                        route.latitud,
                                        route.longitud))
                                    .ToList();
                            }
                            timeControls = timeControls.OrderBy(x => x.dateRegister).ToList();
                            foreach (var item in timeControls)
                            {
                                if (item.dateRegister == null)
                                {
                                    var check = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                    listObservaciones.Add(new ChoferObservaciones("El conductor no llego a la " + check.description+ " RUTA "+check.numberRoute, DateTime.Parse(t.dateRegister.ToString())));
                                }
                                if (item.enable > 2 && item.dateRegister != null)
                                {
                                    var check = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                    var timeControlChofer = context.TimeControl.Where(x => x.idCheckpoint == check.idCheckpoint).ToList();
                                    var ticketCercano = timeControlChofer.Where(x => x.numTicket > t.numTicket).OrderBy(x => x.numTicket).FirstOrDefault();
                                    var timeControlChoferLePaso = context.TimeControl.Where(x => x.numTicket == ticketCercano.numTicket && x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                    var chofer = context.Person.Where(x => x.nit == timeControlChoferLePaso.idEmployee).FirstOrDefault();
                                    listObservaciones.Add(new ChoferObservaciones("Al conductor le pasó " + chofer.name + " " + chofer.lastname + " en " + check.description + " RUTA " + check.numberRoute, DateTime.Parse(t.dateRegister.ToString())));
                                }
                            }
                            List<TimeControl> orderTimeControl = new List<TimeControl>();
                            foreach (var item in checkpointsorder)
                            {
                                TimeControl timeControlOrder = timeControls.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                orderTimeControl.Add(timeControlOrder);
                            }
                            if (t.enable == 1 || t.enable == 3)
                            {
                                for (int i = 0; i < orderTimeControl.Count - 1; i++)
                                {
                                    if (orderTimeControl[i + 1].dateRegister.HasValue && orderTimeControl[i].dateRegister.HasValue)
                                    {
                                        var checkpointPrimer = checkpoints.Where(x => x.status == 2 && x.idLine == Linea).FirstOrDefault();
                                        var timeControlInicio = timeControls.Where(x => x.idCheckpoint == checkpointPrimer.idCheckpoint && t.numTicket == x.numTicket).FirstOrDefault();
                                        Checkpoint checkpoint1 = new Checkpoint();
                                        double minutosParaParada = 0;
                                        double medicionTime = (orderTimeControl[i + 1].dateRegister.Value - orderTimeControl[i].dateRegister.Value).TotalMinutes;
                                        checkpoint1 = checkpointsorder.Where(x => x.idCheckpoint == orderTimeControl[i + 1].idCheckpoint).FirstOrDefault();
                                        TimeSpan timeSpan = new TimeSpan();
                                        if (DateTime.Parse(timeControlInicio.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControlInicio.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                        {
                                            string[] partes = checkpoint1.minArrivalMarket.Trim().Split(':');
                                            minutosParaParada = (double.Parse(partes[0]) * 60) + double.Parse(partes[1]);
                                        }
                                        else
                                        {
                                            string[] partes = checkpoint1.minArrival.Trim().Split(':');
                                            minutosParaParada = (double.Parse(partes[0]) * 60) + double.Parse(partes[1]);
                                        }
                                        double diferenciaMinutos = medicionTime - minutosParaParada;
                                        int diferencia = (int)Math.Floor(diferenciaMinutos);
                                        var check = checkpoints.Where(x => x.idCheckpoint == orderTimeControl[i + 1].idCheckpoint).FirstOrDefault();
                                        if (minutosParaParada < medicionTime)
                                        {
                                            listObservaciones.Add(new ChoferObservaciones("El conductor se retrazó en " + check.description + " con " + diferencia + "min." + " RUTA "+check.numberRoute, DateTime.Parse(t.dateRegister.ToString())));
                                        }
                                    }
                                }
                            }
                            else if (t.enable == 2 || t.enable == 4)
                            {
                                for (int i = 0; i < checkpoints.Count - 1; i++)
                                {
                                    if (orderTimeControl[i].dateRegister.HasValue && orderTimeControl[i + 1].dateRegister.HasValue)
                                    {
                                        var checkpointPrimer = checkpoints.Where(x => x.status == 3 && x.idLine == Linea).FirstOrDefault();
                                        var timeControlInicio = timeControls.Where(x => x.idCheckpoint == checkpointPrimer.idCheckpoint && t.numTicket == x.numTicket).FirstOrDefault();
                                        Checkpoint checkpoint1 = new Checkpoint();
                                        double minutosParaParada = 0;
                                        double medicionTime = (orderTimeControl[i + 1].dateRegister.Value - orderTimeControl[i].dateRegister.Value).TotalMinutes;
                                        checkpoint1 = checkpoints.Where(x => x.idCheckpoint == orderTimeControl[i].idCheckpoint).FirstOrDefault();
                                        TimeSpan timeSpan = new TimeSpan();
                                        if (DateTime.Parse(timeControlInicio.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControlInicio.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                            timeSpan = TimeSpan.Parse(checkpoint1.minArrivalMarket);
                                        else timeSpan = TimeSpan.Parse(checkpoint1.minArrival);
                                        minutosParaParada = timeSpan.TotalMinutes;
                                        double diferenciaMinutos = medicionTime - minutosParaParada;
                                        int diferencia = (int)Math.Floor(diferenciaMinutos);
                                        var check = checkpoints.Where(x => x.idCheckpoint == orderTimeControl[i + 1].idCheckpoint).FirstOrDefault();
                                        if (minutosParaParada < medicionTime)
                                        {
                                            listObservaciones.Add(new ChoferObservaciones("El conductor se retrazó en " + check.description + " con " + diferencia + "min." + " RUTA " + check.numberRoute, DateTime.Parse(t.dateRegister.ToString())));
                                        }
                                    }
                                }
                            }
                            if (t.numTicket != null) t.numTicket = 0;
                            if (t.dateRegister != null) final = DateTime.Parse(t.dateRegister.ToString());
                        }
                    }
                }
                listObservaciones = listObservaciones.Where(x => x.fechaObservacion.Month == primerDiaDelMes.Month && x.fechaObservacion.Year == primerDiaDelMes.Year).ToList();
                return View(listObservaciones);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult PrintedReportObservaciones(string id, int mes, int anio)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                return new ActionAsPdf("ReportIncidenciasChoferImprimir", new { id = id, mes = mes, anio = anio}) { FileName = id + "-" + mes + "/" + anio + "-" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportVueltasChoferDiario(string idEmployee)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<ChoferVueltas> listRank = new List<ChoferVueltas>();

                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);

                using (dbModels context = new dbModels())
                {
                    var employee = context.Employee.Where(e => e.idLine == Linea && e.status == 1 && e.plate != null && e.idEmployee == idEmployee).FirstOrDefault();
                    var check1 = context.Checkpoint.Where(x => x.idLine == Linea && x.status == 2).FirstOrDefault();
                    var check2 = context.Checkpoint.Where(x => x.idLine == Linea && x.status == 3).FirstOrDefault();
                    var person = context.Person.Where(x => x.nit == employee.idEmployee).FirstOrDefault();
                    ViewBag.Name = person.name + " " + person.lastname;
                    var timeControls = context.TimeControl
                        .Where(x => x.idEmployee == employee.idEmployee &&
                                   ((x.idCheckpoint == check1.idCheckpoint && (x.enable == 1 || x.enable == 3)) ||
                                    (x.idCheckpoint == check2.idCheckpoint && (x.enable == 2 || x.enable == 4))) &&
                                   x.dateRegister.HasValue) 
                        .ToList();
                    var result = timeControls
                        .GroupBy(x => x.dateRegister.Value.Date)  
                        .Select(g => new
                        {
                            Fecha = g.Key,  
                            Vueltas = g.Count(),  
                            PrimerHorario = g.Min(x => x.dateRegister),  
                            UltimoHorario = g.Max(x => x.dateRegister), 
                            UltimoNumTicket = g.OrderByDescending(x => x.dateRegister).FirstOrDefault()?.numTicket  
                        })
                        .ToList();

                    foreach (var item in result)
                    {
                        string horario = $"{item.PrimerHorario?.ToString("HH:mm")} -";
                        var ultimoHorario = context.TimeControl.Where(x => x.numTicket == item.UltimoNumTicket).OrderByDescending(x => x.dateRegister).FirstOrDefault();
                        horario = horario + " " + ultimoHorario.dateRegister?.ToString("HH:mm");
                        listRank.Add(new ChoferVueltas(person.name + " " + person.lastname,item.Vueltas, item.Fecha, horario));
                    }
                }

                ViewBag.CurrentMonth = boliviaTime.Month;
                ViewBag.CurrentYear = boliviaTime.Year;
                ViewBag.Id = idEmployee;
                return View(listRank);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportVueltasChoferDiarioEscogido(int year, int month, string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<ChoferVueltas> listRank = new List<ChoferVueltas>();

                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);

                using (dbModels context = new dbModels())
                {
                    var employee = context.Employee.Where(e => e.idLine == Linea && e.status == 1 && e.plate != null && e.idEmployee == id).FirstOrDefault();
                    var check1 = context.Checkpoint.Where(x => x.idLine == Linea && x.status == 2).FirstOrDefault();
                    var check2 = context.Checkpoint.Where(x => x.idLine == Linea && x.status == 3).FirstOrDefault();
                    var person = context.Person.Where(x => x.nit == employee.idEmployee).FirstOrDefault();
                    ViewBag.Name = person.name + " " + person.lastname;
                    var timeControlss = context.TimeControl
                        .Where(x => x.idEmployee == employee.idEmployee &&
                                   ((x.idCheckpoint == check1.idCheckpoint && (x.enable == 1 || x.enable == 3)) ||
                                    (x.idCheckpoint == check2.idCheckpoint && (x.enable == 2 || x.enable == 4))) &&
                                   x.dateRegister.HasValue)
                        .ToList();
                    var timeControls = timeControlss.Where(x => DateTime.Parse(x.dateRegister.ToString()).Month == month && DateTime.Parse(x.dateRegister.ToString()).Year == year).ToList();
                    var result = timeControls
                        .GroupBy(x => x.dateRegister.Value.Date)
                        .Select(g => new
                        {
                            Fecha = g.Key,
                            Vueltas = g.Count(),
                            PrimerHorario = g.Min(x => x.dateRegister),
                            UltimoHorario = g.Max(x => x.dateRegister),
                            UltimoNumTicket = g.OrderByDescending(x => x.dateRegister).FirstOrDefault()?.numTicket
                        })
                        .ToList();

                    foreach (var item in result)
                    {
                        string horario = $"{item.PrimerHorario?.ToString("HH:mm")} -";
                        var ultimoHorario = context.TimeControl.Where(x => x.numTicket == item.UltimoNumTicket).OrderByDescending(x => x.dateRegister).FirstOrDefault();
                        horario = horario + " " + ultimoHorario.dateRegister?.ToString("HH:mm");
                        listRank.Add(new ChoferVueltas(person.name + " " + person.lastname, item.Vueltas, item.Fecha, horario));
                    }
                }

                ViewBag.CurrentMonth = boliviaTime.Month;
                ViewBag.CurrentYear = boliviaTime.Year;
                return View(listRank);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult PrintedReportVueltasChofer(int year, int month, string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                return new ActionAsPdf("ReportVueltasChoferDiarioEscogido", new { year = year, month = month, id = id }) { FileName = id + "-" + month + "/" + year + "-" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
