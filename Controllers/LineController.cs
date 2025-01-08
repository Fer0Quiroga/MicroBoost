using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ProyectoControlLineaBus.Clases;
using System.Runtime.Remoting;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Web.Security;

namespace ProyectoControlLineaBus.Controllers
{
    public class LineController : Controller
    {
        // GET: Line
        public ActionResult Index()
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                Session["OwnerLine"] = null;
                List<Line> lines = new List<Line>();
                using (dbModels context = new dbModels())
                {
                    lines = context.Line.ToList();
                    foreach (var item in lines)
                    {
                        var person = context.Person.Where(x => x.nit == item.idAdmin).FirstOrDefault();
                        if (person != null)
                            item.description = person.name + " " + person.lastname;
                        else item.description = null;
                    }
                    Session["idLineCreateOwner"] = null;
                    return View(lines);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Line/Create
        public ActionResult Create()
        {
            var authResult = AutenticarPasosRol(4);
            if (authResult != null) return authResult;
            return View();
        }

        // POST: Line/Create
        [HttpPost]
        public ActionResult Create(LineControl lineControl)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                Line line = new Line();
                string name = lineControl.extention + " " + lineControl.nameLine.Trim();
                using (dbModels context = new dbModels()) line = context.Line.Where(x => x.nameLine.ToUpper().Trim() == name.ToUpper().Trim()).FirstOrDefault();
                if (line != null)
                {
                    TempData["Lineaa"] = "La línea ya esta registrada";
                    return View();
                }
                line = new Line();
                line.nameLine = name;
                line.price = 0;
                using (dbModels context = new dbModels())
                {
                    line.description = "00:00 - 00:00";
                    context.Line.Add(line);
                    context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }
        public ActionResult Edit()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                Line line;
                User user;
                string Linea = logi.GetLineaFromCookie(Request).ToString();

                using (dbModels context = new dbModels())
                {
                    line = context.Line.Where(x => x.nameLine == Linea).FirstOrDefault();
                }

                if (line == null)
                {
                    return RedirectToAction("Error", "Home");
                }

                LineControl lineControl = new LineControl
                {
                    description = line.description,
                    price = line.price
                };
                List<string> horas = new List<string>();
                List<string> minutos = new List<string> { "00", "15", "30", "45" };
                for (int hour = 0; hour < 24; hour++)
                {
                    horas.Add(hour.ToString("D2"));
                }

                string[] descriptionParts = lineControl.description.Trim().Split('-');
                string[] hourMinuteStart = descriptionParts[0].Trim().Split(':');
                string[] hourMinuteEnd = descriptionParts[1].Trim().Split(':');
                ViewBag.Hours = new SelectList(horas, hourMinuteStart[0]);
                ViewBag.Minutes = new SelectList(minutos, hourMinuteStart[1]);
                ViewBag.Hours2 = new SelectList(horas, hourMinuteEnd[0]);
                ViewBag.Minutes2 = new SelectList(minutos, hourMinuteEnd[1]);

                return View(lineControl);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult Edit(LineControl lineControl, string Hora1, string Minuto1, string Hora2, string Minuto2)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string employee = logi.GetEmployeeIdFromCookie(Request).ToString();
                List<string> horas = new List<string>();
                List<string> minutos = new List<string> { "00", "15", "30", "45" };
                for (int hour = 0; hour < 24; hour++)
                {
                    horas.Add(hour.ToString("D2"));
                }
                ViewBag.Hours = new SelectList(horas, Hora1);
                ViewBag.Minutes = new SelectList(minutos, Minuto1);
                ViewBag.Hours2 = new SelectList(horas, Hora2);
                ViewBag.Minutes2 = new SelectList(minutos, Minuto2);
                User user; Line linea;
                using (dbModels context = new dbModels()) linea = context.Line.Where(x => x.nameLine == Linea).FirstOrDefault();

                using (dbModels context = new dbModels())
                {
                    linea.price = 0;
                    linea.description = $"{Hora1}:{Minuto1} - {Hora2}:{Minuto2}";

                    context.Entry(linea).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("HomeLine");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }


        }


        // GET: Line/Delete/5
        public ActionResult Delete(string id)
        {
            try
            {
                Login login = new Login();
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;

                Line line = new Line();
                List<Employee> employees = new List<Employee>();
                List<Checkpoint> checkpoints = new List<Checkpoint>();
                List<Route> routes = new List<Route>();
                List<DeviationRote> deviationRotes = new List<DeviationRote>();
                List<SanctionEmployee> sanctionEmployees = new List<SanctionEmployee>();
                List<Sanction> sanctions = new List<Sanction>();
                using (dbModels context = new dbModels()) line = context.Line.Where(x => x.nameLine == id).FirstOrDefault();
                using (dbModels context = new dbModels()) employees = context.Employee.Where(x => x.idLine == id).ToList();
                using (dbModels context = new dbModels()) deviationRotes = context.DeviationRote.Where(x => x.idLine == id).ToList();
                using (dbModels context = new dbModels()) sanctions = context.Sanction.Where(x => x.idLine == id).ToList();
                using (dbModels context = new dbModels())
                {
                    foreach (var item in sanctions)
                    {
                        List<SanctionEmployee> sanctionEmployee = context.SanctionEmployee.Where(x => x.idSanction == item.idSanction).ToList();
                        foreach (var item2 in sanctionEmployee)
                        {
                            SanctionEmployee sanctionE = context.SanctionEmployee.Where(x => x.idSanctionEmployee == item2.idSanctionEmployee).FirstOrDefault();
                            context.SanctionEmployee.Remove(sanctionE);
                        }
                        Sanction sanction = context.Sanction.Where(x => x.idSanction == item.idSanction).FirstOrDefault();
                        context.Sanction.Remove(sanction);
                    }
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    foreach (var item in deviationRotes)
                    {
                        DeviationRote deviation = context.DeviationRote.Where(x => x.idDeviation == item.idDeviation).FirstOrDefault();
                        context.DeviationRote.Remove(deviation);
                    }
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    foreach (var item in employees)
                    {
                        List<Star> stars = new List<Star>();
                        stars = context.Star.Where(x => x.idEmployee == item.idEmployee).ToList();
                        foreach (var item2 in stars)
                        {
                            Star deviation = context.Star.Where(x => x.idStar == item2.idStar).FirstOrDefault();
                            context.Star.Remove(deviation);
                        }
                    }
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    foreach (var item in employees)
                    {
                        List<MechanicalFailures> stars = new List<MechanicalFailures>();
                        stars = context.MechanicalFailures.Where(x => x.idEmployee == item.idEmployee).ToList();
                        foreach (var item2 in stars)
                        {
                            MechanicalFailures deviation = context.MechanicalFailures.Where(x => x.idMechanicalFailutes == item2.idMechanicalFailutes).FirstOrDefault();
                            context.MechanicalFailures.Remove(deviation);
                        }
                    }
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    foreach (var item in employees)
                    {
                        List<Report> stars = new List<Report>();
                        stars = context.Report.Where(x => x.idEmployee == item.idEmployee).ToList();
                        foreach (var item2 in stars)
                        {
                            context.Report.Remove(item2);
                        }
                    }
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    foreach (var item in employees)
                    {
                        User user = context.User.Where(x => x.idEmployee == item.idEmployee).FirstOrDefault();
                        if (user != null)
                            context.User.Remove(user);
                    }
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels()) checkpoints = context.Checkpoint.Where(x => x.idLine == id).ToList();
                if (checkpoints.Count > 0)
                {
                    using (dbModels context = new dbModels())
                    {
                        foreach (var item in checkpoints)
                        {
                            List<TimeControl> time = context.TimeControl.Where(x => x.idCheckpoint == item.idCheckpoint).ToList();
                            if (time != null)
                                context.TimeControl.RemoveRange(time);
                        }
                        context.SaveChanges();
                    }
                    using (dbModels context = new dbModels())
                    {
                        foreach (var item in checkpoints)
                        {
                            Checkpoint checkpoint = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                            context.Checkpoint.Remove(checkpoint);
                        }
                        context.SaveChanges();
                    }
                }
                employees = new List<Employee>();
                using (dbModels context = new dbModels()) employees = context.Employee.Where(x => x.idLine == id).ToList();
                using (dbModels context = new dbModels())
                {
                    foreach (var item in employees)
                    {
                        Employee employee = context.Employee.Where(x => x.idEmployee == item.idEmployee).FirstOrDefault();
                        context.Employee.Remove(employee);
                    }
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels()) { List<Owner> owner = context.Owner.Where(x => x.idLine == line.nameLine).ToList(); if (owner.Count > 0) { context.Owner.RemoveRange(owner); context.SaveChanges(); } }
                using (dbModels context = new dbModels())
                {
                    foreach (var item in employees)
                    {
                        Person person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        context.Person.Remove(person);
                    }
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels()) routes = context.Route.Where(x => x.idLine == id).ToList();
                if (routes.Count > 0)
                    using (dbModels context = new dbModels())
                    {
                        foreach (var item in routes)
                        {
                            Route route = context.Route.Where(x => x.idRoute == item.idRoute).FirstOrDefault();
                            context.Route.Remove(route);

                        }
                        context.SaveChanges();
                    }
                using (dbModels context = new dbModels())
                {
                    line = context.Line.Where(x => x.nameLine == id).FirstOrDefault();
                    context.Line.Remove(line);
                    context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult MenuLines()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;

            string id = logi.GetEmployeeIdFromCookie(Request).ToString();
            using (dbModels context = new dbModels())
            {
                List<Line> lines = context.Line.Where(x => x.idAdmin == id).ToList();
                return View(lines);
            }
        }
        public ActionResult HomeLine()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                string id = logi.GetEmployeeIdFromCookie(Request).ToString();
                if (Line == "") 
                {
                    using (dbModels context = new dbModels())
                    {
                        var line = context.Line.Where(x => x.idAdmin == id).ToList();
                        if (line.Count == 1)
                        {
                            return RedirectToAction("actualizarCookie", "Line", new { linea = line[0].nameLine });
                        }
                    }
                    return RedirectToAction("MenuLines");
                }
                else
                {
                    using (dbModels context = new dbModels())
                    {
                        var line = context.Line.Where(x => x.idAdmin == id && x.nameLine == Line).FirstOrDefault();
                        if (line == null)
                            return RedirectToAction("CloseSession", "Home");
                    }
                    using (dbModels context = new dbModels())
                    {
                        var line = context.Line.Where(x => x.idAdmin == id).ToList();
                        ViewBag.count = line.Count;
                    }
                    var lineades = new Line();
                    List<Checkpoint> check1 = new List<Checkpoint>(); List<Checkpoint> check2 = new List<Checkpoint>();
                    using (dbModels context = new dbModels())
                    {
                        check1 = context.Checkpoint.Where(x => x.status == 2 && x.idLine == Line).GroupBy(x => x.numberRoute).Select(x => x.FirstOrDefault()).ToList();
                        check2 = context.Checkpoint.Where(x => x.status == 3 && x.idLine == Line).GroupBy(x => x.numberRoute).Select(x => x.FirstOrDefault()).ToList();
                        lineades = context.Line.Where(x => x.nameLine == Line).FirstOrDefault();
                    }

                    if (!check1.Any() ) return RedirectToAction("Parada1", "Route", new { numTypeRoute = 1 });
                    if (!check2.Any()) return RedirectToAction("Parada2", "Route", new { numTypeRoute = 1 });
                    var noCoincidencia = check1.Where(p2 => check2.All(p1 => p2.numberRoute != p1.numberRoute)).ToList();
                    ViewBag.name = Line;
                    ViewBag.description = lineades.description;
                    foreach (var item in noCoincidencia)
                    {
                        return RedirectToAction("Parada2", "Route", new { numTypeRoute = item.numberRoute });
                    }
                    List<MuestraRutasMapas> mapa = new List<MuestraRutasMapas>();
                    int count = 0;
                    List<int> numR = new List<int>();
                    using (dbModels context = new dbModels())
                        numR = context.Route
                        .Where(x => x.idLine == Line && x.numTypeRoute != null)
                        .GroupBy(x => x.numTypeRoute)
                        .Select(g => g.Key.Value)
                        .Distinct()
                        .ToList();
                    foreach (var numType in numR)
                    {
                        for (int i = 1; i <= 2; i++)
                        {
                            MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas
                            {
                                ListRutasMostrar = new List<RutasMostrar>(),
                                ListParadasMostrar = new List<ParadasMostrar>(),
                                ListBloqueosMostrar = new List<BloqueosMostrar>(),
                                NumeroRuta = i
                            };
                            List<BloqueosMostrar> nuevosBloqueos = new List<BloqueosMostrar>();
                            List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                            List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();

                            using (dbModels context = new dbModels())
                            {
                                var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status != 0 && x.numberRoute == numType).ToList();
                                var routes = context.Route
                                    .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numType)
                                    .ToList();
                                var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numType).ToList();
                                if (deviation.Count > 0)
                                {
                                    foreach (var item in deviation)
                                    {
                                        List<RutasMostrar> listBloqueos = new List<RutasMostrar>();
                                        var route1 = routes.Where(x => x.idRoute == item.idRouteStart && x.idLine == Line).FirstOrDefault();
                                        var route2 = routes.Where(x => x.idRoute == item.idRouteEnd && x.idLine == Line).FirstOrDefault();
                                        if (route1 != null && route2 != null)
                                        {
                                            var routesDeviation = routes.Where(x => x.numPoint >= route1.numPoint && x.numPoint <= route2.numPoint && x.idLine == Line).ToList();
                                            foreach (var item2 in routesDeviation)
                                            {
                                                listBloqueos.Add(new RutasMostrar(item2.idRoute, item2.latitud, item2.logintud, "", item2.status, (int)item2.numPoint));
                                            }
                                            var place1 = context.Place.Where(x => x.idPlace == route1.idPlace).FirstOrDefault();
                                            var place2 = context.Place.Where(x => x.idPlace == route2.idPlace).FirstOrDefault();
                                            nuevosBloqueos.Add(new BloqueosMostrar(item.idDeviation, "Bloqueo " + place1.description + " y " + place2.description, listBloqueos, (double)route1.latitud, (double)route2.logintud));
                                        }
                                    }
                                }
                                if (routes.Count > 0)
                                {
                                    count = 0;
                                    foreach (var item in routes)
                                    {
                                        count++;
                                        Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                                        if (place != null)
                                        {
                                            nuevasRutas.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description, item.status, (int)item.numPoint));
                                        }
                                    }

                                    if (nuevasRutas.Any())
                                    {
                                        RutasMostrar primeraRuta = nuevasRutas.FirstOrDefault(r => r.status == 2);
                                        if (primeraRuta != null)
                                        {
                                            nuevasRutas = nuevasRutas
                                                .OrderBy(route => route.numPoint)
                                                .ToList();
                                        }
                                    }
                                    count = 0;
                                    foreach (var item in nuevasRutas)
                                    {
                                        item.name = item.name + "-" + count;
                                        count++;
                                    }
                                }

                                if (checkpoints.Count > 0)
                                {
                                    count = 0;
                                    foreach (var item in checkpoints)
                                    {
                                        count++;
                                        nuevasParadas.Add(new ParadasMostrar(item.idCheckpoint, item.description, item.status, item.latitud, item.longitud));
                                    }

                                    if (nuevasParadas.Any())
                                    {
                                        ParadasMostrar primeraParada = nuevasParadas.FirstOrDefault(r => r.status == 2);
                                        if (primeraParada != null)
                                        {
                                            nuevasParadas = nuevasParadas
                                                .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                    (decimal)primeraParada.latitud,
                                                    (decimal)primeraParada.longitud,
                                                    route.latitud,
                                                    route.longitud))
                                                .ToList();
                                        }
                                    }
                                    count = 0;
                                    foreach (var item in nuevasParadas)
                                    {
                                        item.description = item.description + "-" + count;
                                        count++;
                                    }
                                }
                            }
                            muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                            muestraRutasMapas.ListParadasMostrar = nuevasParadas;
                            muestraRutasMapas.ListBloqueosMostrar = nuevosBloqueos;
                            mapa.Add(muestraRutasMapas);
                        }
                    }
                    return View(mapa);
                }

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
        public ActionResult ViewAllLines()
        {

            List<MuestraTodasLineas> allLines = new List<MuestraTodasLineas>();
            List<Line> lines = new List<Line>();
            using (dbModels context = new dbModels()) lines = context.Line.ToList();
            foreach (var item3 in lines)
            {
                string Line = item3.nameLine;

                int count = 0;
                List<int> numR = new List<int>();
                using (dbModels context = new dbModels())
                {
                    numR = context.Route
                        .Where(x => x.idLine == Line && x.numTypeRoute != null)
                        .GroupBy(x => x.numTypeRoute)
                        .Select(g => g.Key.Value)
                        .Distinct()
                        .ToList();
                    if (Line == "Micro LL")
                        ViewBag.NumRoutes = numR;
                }
                foreach (var numType in numR)
                {
                    List<MuestraRutasMapas> mapa = new List<MuestraRutasMapas>();
                    for (int i = 1; i <= 2; i++)
                    {
                        MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas
                        {
                            ListRutasMostrar = new List<RutasMostrar>(),
                            ListParadasMostrar = new List<ParadasMostrar>(),
                            ListBloqueosMostrar = new List<BloqueosMostrar>(),
                            NumeroRuta = i
                        };
                        List<BloqueosMostrar> nuevosBloqueos = new List<BloqueosMostrar>();
                        List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                        List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();
                        using (dbModels context = new dbModels())
                        {
                            var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status != 0 && x.numberRoute == numType).ToList();
                            var routes = context.Route
                                .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numType)
                                .ToList();
                            var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numType).ToList();
                            if (deviation.Count > 0)
                            {
                                foreach (var item in deviation)
                                {
                                    List<RutasMostrar> listBloqueos = new List<RutasMostrar>();
                                    var route1 = routes.Where(x => x.idRoute == item.idRouteStart && x.idLine == Line).FirstOrDefault();
                                    var route2 = routes.Where(x => x.idRoute == item.idRouteEnd && x.idLine == Line).FirstOrDefault();
                                    if (route1 != null && route2 != null)
                                    {
                                        var routesDeviation = routes.Where(x => x.numPoint >= route1.numPoint && x.numPoint <= route2.numPoint && x.idLine == Line && x.numTypeRoute == numType).ToList();
                                        foreach (var item2 in routesDeviation)
                                        {
                                            listBloqueos.Add(new RutasMostrar(item2.idRoute, item2.latitud, item2.logintud, "", item2.status, (int)item2.numPoint));
                                        }
                                        var place1 = context.Place.Where(x => x.idPlace == route1.idPlace).FirstOrDefault();
                                        var place2 = context.Place.Where(x => x.idPlace == route2.idPlace).FirstOrDefault();
                                        nuevosBloqueos.Add(new BloqueosMostrar(item.idDeviation, "Bloqueo " + place1.description + " y " + place2.description, listBloqueos));
                                    }

                                }
                            }
                            if (routes.Count > 0)
                            {
                                count = 0;
                                foreach (var item in routes)
                                {
                                    count++;
                                    Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                                    if (place != null)
                                    {
                                        nuevasRutas.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description, item.status, (int)item.numPoint));
                                    }
                                }

                                if (nuevasRutas.Any())
                                {
                                    RutasMostrar primeraRuta = nuevasRutas.FirstOrDefault(r => r.status == 2);
                                    if (primeraRuta != null)
                                    {
                                        nuevasRutas = nuevasRutas
                                            .OrderBy(route => route.numPoint)
                                            .ToList();
                                    }
                                }
                                count = 0;
                                foreach (var item in nuevasRutas)
                                {
                                    item.name = item.name + "-" + count;
                                    count++;
                                }
                            }

                            if (checkpoints.Count > 0)
                            {
                                count = 0;
                                foreach (var item in checkpoints)
                                {
                                    count++;
                                    nuevasParadas.Add(new ParadasMostrar(item.idCheckpoint, item.description, item.status, item.latitud, item.longitud));
                                }

                                if (nuevasParadas.Any())
                                {
                                    ParadasMostrar primeraParada = nuevasParadas.FirstOrDefault(r => r.status == 2);
                                    if (primeraParada != null)
                                    {
                                        nuevasParadas = nuevasParadas
                                            .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                (decimal)primeraParada.latitud,
                                                (decimal)primeraParada.longitud,
                                                route.latitud,
                                                route.longitud))
                                            .ToList();
                                    }
                                }
                                count = 0;
                                foreach (var item in nuevasParadas)
                                {
                                    item.description = item.description + "-" + count;
                                    count++;
                                }
                            }
                        }
                        muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                        muestraRutasMapas.ListParadasMostrar = nuevasParadas;
                        muestraRutasMapas.ListBloqueosMostrar = nuevosBloqueos;
                        mapa.Add(muestraRutasMapas);
                    }
                    allLines.Add(new MuestraTodasLineas(Line + "Ruta " + numType, mapa));
                }

            }

            return View(allLines);

        }
        public ActionResult ViewLinesPerson(decimal latitudInicial, decimal longitudInicial, decimal latitudFinal, decimal longitudFinal)
        {
            try
            {
                MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas();
                if (muestraRutasMapas.CalcularDistanciaHaversine(latitudInicial, longitudInicial, latitudFinal, longitudFinal) < 0.2)
                {
                    return RedirectToAction("Index", "Home");
                }
                bool verificacion = false;
                double kmMinimos = 1.0;
                ViewBag.latitudInicial = latitudInicial;
                ViewBag.longitudInicial = longitudInicial;
                ViewBag.latitudFinal = latitudFinal;
                ViewBag.longitudFinal = longitudFinal;
                List<LineaRutaPasajero> lines = new List<LineaRutaPasajero>();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                List<MuestraTodasLineas> allLines = new List<MuestraTodasLineas>();
                while (verificacion == false)
                {
                    lines = new List<LineaRutaPasajero>();
                    using (dbModels context = new dbModels())
                    {
                        var allRoutes = context.Route.ToList();
                        var linesInicio = allRoutes
                                .Where(x => muestraRutasMapas.CalcularDistanciaHaversine(latitudInicial, longitudInicial, x.latitud, x.logintud) < kmMinimos)
                                .GroupBy(x => new { x.idLine, x.numberRoute, x.numTypeRoute })
                                .Select(g => new
                                {
                                    idLine = g.Key.idLine,
                                    numberRoute = g.Key.numberRoute,
                                    numPoint = g.OrderBy(x => muestraRutasMapas.CalcularDistanciaHaversine(latitudInicial, longitudInicial, x.latitud, x.logintud)).FirstOrDefault().numPoint,
                                    numType = g.Key.numTypeRoute
                                })
                                .ToList();
                        var linesFinal = allRoutes
                            .Where(x => muestraRutasMapas.CalcularDistanciaHaversine(latitudFinal, longitudFinal, x.latitud, x.logintud) < kmMinimos)
                            .GroupBy(x => new { x.idLine, x.numberRoute, x.numTypeRoute })
                            .Select(g => new
                            {
                                idLine = g.Key.idLine,
                                numberRoute = g.Key.numberRoute,
                                numPoint = g.OrderBy(x => muestraRutasMapas.CalcularDistanciaHaversine(latitudFinal, longitudFinal, x.latitud, x.logintud)).FirstOrDefault().numPoint,
                                numType = g.Key.numTypeRoute
                            })
                            .ToList();

                        foreach (var item in linesInicio)
                        {
                            foreach (var item2 in linesFinal)
                            {
                                if (lines.Count <= 6)
                                    if (item.idLine == item2.idLine && item.numberRoute == item2.numberRoute && item.numPoint < item2.numPoint)
                                    {
                                        if (!lines.Any(x => x.nameLine == item.idLine && x.numRuta == (int)item.numberRoute && x.numType == (int)item.numType))
                                            lines.Add(new LineaRutaPasajero(item.idLine, (int)item.numPoint, (int)item2.numPoint, (int)item.numberRoute, (int)item.numType));
                                    }
                            }
                        }
                    }
                    kmMinimos = kmMinimos + 0.1;
                    if (lines.Count > 0) verificacion = true;
                }
                foreach (var item3 in lines)
                {
                    string Line = item3.nameLine;
                    List<MuestraRutasMapas> mapa = new List<MuestraRutasMapas>();
                    int count = 0;

                    muestraRutasMapas = new MuestraRutasMapas
                    {
                        ListRutasMostrar = new List<RutasMostrar>(),
                        ListParadasMostrar = new List<ParadasMostrar>(),
                        ListBloqueosMostrar = new List<BloqueosMostrar>(),
                        NumeroRuta = item3.numRuta
                    };
                    List<BloqueosMostrar> nuevosBloqueos = new List<BloqueosMostrar>();
                    List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                    List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();
                    List<string> choferesTiempo = new List<string>();
                    List<ChoferesTiempoAuxi> choferesTiempoAuxi = new List<ChoferesTiempoAuxi>();
                    using (dbModels context = new dbModels())
                    {
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status != 0 && x.numberRoute == item3.numType).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == item3.numRuta && x.numTypeRoute == item3.numType)
                            .ToList();
                        var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == item3.numRuta && x.numTypeRoute == item3.numType).ToList();
                        if (deviation.Count > 0)
                        {
                            foreach (var item in deviation)
                            {
                                List<RutasMostrar> listBloqueos = new List<RutasMostrar>();
                                var route1 = routes.Where(x => x.idRoute == item.idRouteStart && x.idLine == Line).FirstOrDefault();
                                var route2 = routes.Where(x => x.idRoute == item.idRouteEnd && x.idLine == Line).FirstOrDefault();
                                if (route1 != null && route2 != null)
                                {
                                    var routesDeviation = routes.Where(x => x.numPoint >= route1.numPoint && x.numPoint <= route2.numPoint && x.idLine == Line).ToList();
                                    foreach (var item2 in routesDeviation)
                                    {
                                        listBloqueos.Add(new RutasMostrar(item2.idRoute, item2.latitud, item2.logintud, "", item2.status, (int)item2.numPoint));
                                    }
                                    var place1 = context.Place.Where(x => x.idPlace == route1.idPlace).FirstOrDefault();
                                    var place2 = context.Place.Where(x => x.idPlace == route2.idPlace).FirstOrDefault();
                                    listBloqueos = listBloqueos
                                    .Where(item2 => item2.numPoint >= item3.inicio && item2.numPoint <= item3.final)
                                    .ToList();
                                    nuevosBloqueos.Add(new BloqueosMostrar(item.idDeviation, "Bloqueo " + place1.description + " y " + place2.description, listBloqueos));
                                }
                            }
                        }
                        if (routes.Count > 0)
                        {
                            count = 0;
                            foreach (var item in routes)
                            {
                                count++;
                                Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                                if (place != null)
                                {
                                    nuevasRutas.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description, item.status, (int)item.numPoint));
                                }
                            }

                            if (nuevasRutas.Any())
                            {
                                RutasMostrar primeraRuta = nuevasRutas.FirstOrDefault(r => r.status == 2);
                                if (primeraRuta != null)
                                {
                                    nuevasRutas = nuevasRutas
                                        .OrderBy(route => route.numPoint)
                                        .ToList();
                                }
                            }
                            count = 0;
                            nuevasRutas = nuevasRutas
                            .Where(item2 => item2.numPoint >= item3.inicio && item2.numPoint <= item3.final)
                            .ToList();
                            foreach (var item in nuevasRutas)
                            {
                                item.name = item.name + "-" + count;
                                count++;
                            }
                        }

                        if (checkpoints.Count > 0)
                        {
                            count = 0;
                            foreach (var item in checkpoints)
                            {
                                count++;
                                nuevasParadas.Add(new ParadasMostrar(item.idCheckpoint, item.description, item.status, item.latitud, item.longitud));
                            }

                            if (nuevasParadas.Any())
                            {
                                ParadasMostrar primeraParada = nuevasParadas.FirstOrDefault(r => r.status == 2);
                                if (primeraParada != null)
                                {
                                    nuevasParadas = nuevasParadas
                                        .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                            (decimal)primeraParada.latitud,
                                            (decimal)primeraParada.longitud,
                                            route.latitud,
                                            route.longitud))
                                        .ToList();
                                }
                            }
                            count = 0;
                            foreach (var item in nuevasParadas)
                            {
                                item.description = item.description + "-" + count;
                                count++;
                            }
                        }

                        List<Employee> em = context.Employee.Where(x => x.status != 0 && x.plate != null && (x.enabled != null || x.enabled == 1 || x.enabled == 3) && x.idLine == Line).ToList();
                        if (em.Count > 0)
                        {
                            foreach (var item in em)
                            {
                                double minutes = 0;
                                double kmTotales = 0.0;
                                double km = 0;
                                if (item3.numRuta == 1)
                                {
                                    var timeControls = context.TimeControl
                                    .Where(x => x.idEmployee == item.idEmployee && x.dateRegister.HasValue && (x.enable == 1 || x.enable == 3))
                                    .AsEnumerable()
                                    .OrderBy(x => Math.Abs((x.dateRegister.Value - boliviaTime).TotalSeconds))
                                    .ToList();
                                    var timeControl = timeControls.FirstOrDefault();

                                    if (timeControl != null)
                                    {
                                        var tim = context.Checkpoint.Where(x => x.idCheckpoint == timeControl.idCheckpoint && x.numberRoute == item3.numType).FirstOrDefault();
                                        if (tim != null)
                                        {
                                            var paradas = new List<ParadasOrden>();
                                            foreach (var paradass in checkpoints)
                                            {
                                                paradas.Add(new ParadasOrden(paradass.idCheckpoint, paradass.status, paradass.latitud, paradass.longitud));
                                            }
                                            var rota = routes.Where(x => x.numberRoute == item3.numRuta && x.numPoint == item3.inicio).FirstOrDefault();
                                            paradas.Add(new ParadasOrden(0, 7, rota.latitud, rota.logintud));
                                            List<CheckpointOrden> checkpointOrdens = new List<CheckpointOrden>();
                                            var checkTimeControl = checkpoints.Where(x => x.idCheckpoint == timeControl.idCheckpoint).FirstOrDefault();
                                            var routeCercanaTimeControl = routes.OrderBy((route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                (decimal)checkTimeControl.latitud,
                                                (decimal)checkTimeControl.longitud,
                                                route.latitud,
                                                route.logintud)));
                                            var routeTimeControl = routeCercanaTimeControl.FirstOrDefault();
                                            var checkOrdendesdeRutaInicial = checkpoints
                                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                        (decimal)routeTimeControl.latitud,
                                                        (decimal)routeTimeControl.logintud,
                                                        route.latitud,
                                                        route.longitud))
                                                    .ToList();
                                            var primeraParada = checkpoints.FirstOrDefault(r => r.status == 2);
                                            var paradasOrder = paradas.OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                (decimal)primeraParada.latitud,
                                                (decimal)primeraParada.longitud,
                                                route.latitud,
                                                route.longitud)).ToList();
                                            var checkOrder = new List<Checkpoint>();
                                            if (primeraParada != null)
                                            {
                                                checkOrder = checkpoints
                                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                        (decimal)primeraParada.latitud,
                                                        (decimal)primeraParada.longitud,
                                                        route.latitud,
                                                        route.longitud))
                                                    .ToList();
                                            }
                                            int countorder = 0;
                                            foreach (var item5 in checkOrder)
                                            {
                                                countorder++;
                                                checkpointOrdens.Add(new CheckpointOrden(item5.idCheckpoint, countorder));
                                            }
                                            CheckpointOrden final = new CheckpointOrden(); CheckpointOrden inicio = new CheckpointOrden();
                                            List<Checkpoint> ParadasAnteriores = new List<Checkpoint>();
                                            List<CheckNumRoute> checkNumRoutes = new List<CheckNumRoute>();
                                            foreach (var c in checkpointOrdens)
                                            {
                                                var parada = checkpoints.Where(x => x.idCheckpoint == c.idCheckpoint).FirstOrDefault();
                                                if (c.idCheckpoint == checkTimeControl.idCheckpoint)
                                                    inicio = c;
                                                ParadasAnteriores.Add(parada);
                                            }
                                            foreach (var para in checkpoints)
                                            {
                                                var paraNumPoint = routes
                                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                        (decimal)para.latitud,
                                                        (decimal)para.longitud,
                                                        route.latitud,
                                                        route.logintud))
                                                    .ToList();
                                                var primerParaNumPoint = paraNumPoint.FirstOrDefault();
                                                checkNumRoutes.Add(new CheckNumRoute(para.idCheckpoint, (int)primerParaNumPoint.numPoint));
                                            }
                                            var finalCheck = checkNumRoutes
                                                .Where(x => x.numPoint > item3.inicio)
                                                .OrderBy(x => x.numPoint)
                                                .FirstOrDefault();
                                            final = checkpointOrdens.Where(x => x.idCheckpoint == finalCheck.idCheckpoint).FirstOrDefault();
                                            var inicioCheck = checkNumRoutes
                                            .Where(x => x.numPoint <= item3.inicio)
                                            .OrderByDescending(x => x.numPoint)
                                            .FirstOrDefault();
                                            var inicioC = checkpointOrdens.Where(x => x.idCheckpoint == finalCheck.idCheckpoint).FirstOrDefault();
                                            var numInicioTimeControl = checkpointOrdens.Where(x => x.idCheckpoint == timeControl.idCheckpoint).FirstOrDefault();
                                            //for (int i = 0; i < paradasOrder.Count() - 1; i++)
                                            //{
                                            //    if (paradasOrder[i].status == 7)
                                            //    {
                                            //        int id = paradasOrder[i + 1].id;
                                            //        final = checkpointOrdens.Where(x => x.idCheckpoint == id).FirstOrDefault();
                                            //    }
                                            //}
                                            if (inicio.orden + 1 == final.orden)
                                            {
                                                var routesInicioFinal = routes.Where(item2 => item2.numPoint >= inicioCheck.numPoint && item2.numPoint <= finalCheck.numPoint).ToList();

                                                for (int l = 0; l < routesInicioFinal.Count() - 1; l++)
                                                {
                                                    var calc = muestraRutasMapas.CalcularDistanciaHaversine(
                                                    (decimal)routesInicioFinal[l].latitud,
                                                    (decimal)routesInicioFinal[l].logintud,
                                                    routesInicioFinal[l + 1].latitud,
                                                    routesInicioFinal[l + 1].logintud);
                                                    kmTotales = kmTotales + calc;
                                                }
                                                routesInicioFinal = routes.Where(item2 => item2.numPoint >= inicioCheck.numPoint && item2.numPoint <= item3.inicio).ToList();
                                                for (int l = 0; l < routesInicioFinal.Count() - 1; l++)
                                                {
                                                    var calc = muestraRutasMapas.CalcularDistanciaHaversine(
                                                    (decimal)routesInicioFinal[l].latitud,
                                                    (decimal)routesInicioFinal[l].logintud,
                                                    routesInicioFinal[l + 1].latitud,
                                                    routesInicioFinal[l + 1].logintud);
                                                    km = km + calc;
                                                }
                                                for (int i = inicio.orden - 1; i < final.orden - 1; i++)
                                                {
                                                    if (DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i + 1].minArrivalMarket.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        minutes = minutes + hour * 60 + minute;

                                                    }
                                                    else
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i + 1].minArrival.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        minutes = minutes + hour * 60 + minute;
                                                    }
                                                }
                                                TimeSpan diferencia = boliviaTime.Subtract(DateTime.Parse(timeControl.dateRegister.ToString()));
                                                int minutesDiferenciaComparacion = (int)diferencia.TotalMinutes;
                                                kmTotales = Math.Round(kmTotales, 2);
                                                km = Math.Round(km, 2);
                                                int res = (int)(minutes * km / kmTotales);

                                                var chofer = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                                                if (minutesDiferenciaComparacion < res)
                                                {
                                                    List<Star> stars = new List<Star>();
                                                    stars = context.Star.Where(x => x.idEmployee == item.idEmployee).ToList();
                                                    var star = stars.Where(x => DateTime.Parse(x.dateRegister.ToString()).Month == boliviaTime.Month && DateTime.Parse(x.dateRegister.ToString()).Year == boliviaTime.Year).ToList();
                                                    double start = 5;
                                                    if (stars.Count > 0)
                                                    {
                                                        int countstars = 1;
                                                        foreach (var st in star)
                                                        {
                                                            countstars++;
                                                            start = start + double.Parse(st.numStar.ToString());
                                                        }
                                                        start = Math.Round(start / countstars, 2);
                                                    }
                                                    int result = res - minutesDiferenciaComparacion;
                                                    choferesTiempoAuxi.Add(new ChoferesTiempoAuxi(chofer.name + " " + chofer.lastname + "(★" + start + ")" + " min. aprox. " + result, result));
                                                }
                                            }
                                            else if (inicio.orden < final.orden)
                                            {

                                                var routesInicioFinal = routes.Where(item2 => item2.numPoint >= inicioCheck.numPoint && item2.numPoint <= finalCheck.numPoint).ToList();

                                                for (int l = 0; l < routesInicioFinal.Count() - 1; l++)
                                                {
                                                    var calc = muestraRutasMapas.CalcularDistanciaHaversine(
                                                    (decimal)routesInicioFinal[l].latitud,
                                                    (decimal)routesInicioFinal[l].logintud,
                                                    routesInicioFinal[l + 1].latitud,
                                                    routesInicioFinal[l + 1].logintud);
                                                    kmTotales = kmTotales + calc;
                                                }
                                                routesInicioFinal = routes.Where(item2 => item2.numPoint >= inicioCheck.numPoint && item2.numPoint <= item3.inicio).ToList();
                                                for (int l = 0; l < routesInicioFinal.Count() - 1; l++)
                                                {
                                                    var calc = muestraRutasMapas.CalcularDistanciaHaversine(
                                                    (decimal)routesInicioFinal[l].latitud,
                                                    (decimal)routesInicioFinal[l].logintud,
                                                    routesInicioFinal[l + 1].latitud,
                                                    routesInicioFinal[l + 1].logintud);
                                                    km = km + calc;
                                                }
                                                for (int i = final.orden - 2; i < final.orden - 1; i++)
                                                {
                                                    if (DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i + 1].minArrivalMarket.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        minutes = minutes + hour * 60 + minute;

                                                    }
                                                    else
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i + 1].minArrival.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        minutes = minutes + hour * 60 + minute;
                                                    }
                                                }
                                                TimeSpan diferencia = boliviaTime.Subtract(DateTime.Parse(timeControl.dateRegister.ToString()));
                                                int minutesDiferenciaComparacion = (int)diferencia.TotalMinutes;
                                                kmTotales = Math.Round(kmTotales, 2);
                                                km = Math.Round(km, 2);
                                                int res = (int)(minutes * km / kmTotales);
                                                var chofer = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                                                int time = 0;

                                                for (int i = numInicioTimeControl.orden - 1; i < final.orden - 2; i++)
                                                {
                                                    if (DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i + 1].minArrivalMarket.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        time = time + hour * 60 + minute;

                                                    }
                                                    else
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i + 1].minArrival.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        time = time + hour * 60 + minute;
                                                    }
                                                }
                                                res = time + res;
                                                if (minutesDiferenciaComparacion < res)
                                                {
                                                    List<Star> stars = new List<Star>();
                                                    stars = context.Star.Where(x => x.idEmployee == item.idEmployee).ToList();
                                                    var star = stars.Where(x => DateTime.Parse(x.dateRegister.ToString()).Month == boliviaTime.Month && DateTime.Parse(x.dateRegister.ToString()).Year == boliviaTime.Year).ToList();
                                                    double start = 5;
                                                    if (stars.Count > 0)
                                                    {
                                                        int countstars = 1;
                                                        foreach (var st in star)
                                                        {
                                                            countstars++;
                                                            start = start + double.Parse(st.numStar.ToString());
                                                        }
                                                        start = Math.Round(start / countstars, 2);
                                                    }
                                                    int result = res - minutesDiferenciaComparacion;
                                                    choferesTiempoAuxi.Add(new ChoferesTiempoAuxi(chofer.name + " " + chofer.lastname + "(★" + start + ")" + " min. aprox. " + result, result));
                                                }
                                            }
                                        }

                                    }
                                }
                                else
                                {
                                    var timeControls = context.TimeControl
                                    .Where(x => x.idEmployee == item.idEmployee && x.dateRegister.HasValue && (x.enable == 2 || x.enable == 4))
                                    .AsEnumerable()
                                    .OrderBy(x => Math.Abs((x.dateRegister.Value - boliviaTime).TotalSeconds))
                                    .ToList();
                                    var timeControl = timeControls.FirstOrDefault();

                                    if (timeControl != null)
                                    {
                                        var tim = context.Checkpoint.Where(x => x.idCheckpoint == timeControl.idCheckpoint && x.numberRoute == item3.numType).FirstOrDefault();
                                        if (tim != null)
                                        {
                                            var paradas = new List<ParadasOrden>();
                                            foreach (var paradass in checkpoints)
                                            {
                                                paradas.Add(new ParadasOrden(paradass.idCheckpoint, paradass.status, paradass.latitud, paradass.longitud));
                                            }
                                            var rota = context.Route.Where(x => x.numberRoute == item3.numRuta && x.numPoint == item3.inicio).FirstOrDefault();
                                            paradas.Add(new ParadasOrden(0, 7, rota.latitud, rota.logintud));
                                            List<CheckpointOrden> checkpointOrdens = new List<CheckpointOrden>();
                                            var checkTimeControl = checkpoints.Where(x => x.idCheckpoint == timeControl.idCheckpoint).FirstOrDefault();
                                            var routeCercanaTimeControl = routes.OrderBy((route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                (decimal)checkTimeControl.latitud,
                                                (decimal)checkTimeControl.longitud,
                                                route.latitud,
                                                route.logintud)));
                                            var routeTimeControl = routeCercanaTimeControl.FirstOrDefault();
                                            var checkOrdendesdeRutaInicial = checkpoints
                                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                        (decimal)routeTimeControl.latitud,
                                                        (decimal)routeTimeControl.logintud,
                                                        route.latitud,
                                                        route.longitud))
                                                    .ToList();
                                            var checkAnteriorPosterior = new List<Checkpoint>();
                                            checkAnteriorPosterior.Add(checkOrdendesdeRutaInicial[0]);
                                            checkAnteriorPosterior.Add(checkOrdendesdeRutaInicial[1]);
                                            var primeraParada = checkpoints.FirstOrDefault(r => r.status == 3);
                                            var paradasOrder = paradas.OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                (decimal)primeraParada.latitud,
                                                (decimal)primeraParada.longitud,
                                                route.latitud,
                                                route.longitud)).ToList();
                                            var checkOrder = new List<Checkpoint>();
                                            if (primeraParada != null)
                                            {
                                                checkOrder = checkpoints
                                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                        (decimal)primeraParada.latitud,
                                                        (decimal)primeraParada.longitud,
                                                        route.latitud,
                                                        route.longitud))
                                                    .ToList();
                                            }
                                            int countorder = 0;
                                            foreach (var item5 in checkOrder)
                                            {
                                                countorder++;
                                                checkpointOrdens.Add(new CheckpointOrden(item5.idCheckpoint, countorder));
                                            }
                                            CheckpointOrden final = new CheckpointOrden(); CheckpointOrden inicio = new CheckpointOrden();
                                            List<Checkpoint> ParadasAnteriores = new List<Checkpoint>();
                                            List<CheckNumRoute> checkNumRoutes = new List<CheckNumRoute>();
                                            foreach (var c in checkpointOrdens)
                                            {
                                                var parada = checkpoints.Where(x => x.idCheckpoint == c.idCheckpoint).FirstOrDefault();
                                                if (c.idCheckpoint == checkTimeControl.idCheckpoint)
                                                    inicio = c;
                                                ParadasAnteriores.Add(parada);
                                            }
                                            foreach (var para in checkpoints)
                                            {
                                                var paraNumPoint = routes
                                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                        (decimal)para.latitud,
                                                        (decimal)para.longitud,
                                                        route.latitud,
                                                        route.logintud))
                                                    .ToList();
                                                var primerParaNumPoint = paraNumPoint.FirstOrDefault();
                                                checkNumRoutes.Add(new CheckNumRoute(para.idCheckpoint, (int)primerParaNumPoint.numPoint));
                                            }
                                            var finalCheck = checkNumRoutes
                                                .Where(x => x.numPoint > item3.inicio)
                                                .OrderBy(x => x.numPoint)
                                                .FirstOrDefault();
                                            final = checkpointOrdens.Where(x => x.idCheckpoint == finalCheck.idCheckpoint).FirstOrDefault();
                                            var inicioCheck = checkNumRoutes
                                                .Where(x => x.numPoint <= item3.inicio)
                                                .OrderByDescending(x => x.numPoint)
                                                .FirstOrDefault();
                                            var inicioC = checkpointOrdens.Where(x => x.idCheckpoint == finalCheck.idCheckpoint).FirstOrDefault();
                                            var numInicioTimeControl = checkpointOrdens.Where(x => x.idCheckpoint == timeControl.idCheckpoint).FirstOrDefault();
                                            //for (int i = 0; i < paradasOrder.Count() - 1; i++)
                                            //{
                                            //    if (paradasOrder[i].status == 7)
                                            //    {
                                            //        int id = paradasOrder[i + 1].id;
                                            //        final = checkpointOrdens.Where(x => x.idCheckpoint == id).FirstOrDefault();
                                            //    }
                                            //}
                                            if (inicio.orden + 1 == final.orden)
                                            {
                                                var routesInicioFinal = routes.Where(item2 => item2.numPoint >= inicioCheck.numPoint && item2.numPoint <= finalCheck.numPoint).ToList();

                                                for (int l = 0; l < routesInicioFinal.Count() - 1; l++)
                                                {
                                                    var calc = muestraRutasMapas.CalcularDistanciaHaversine(
                                                    (decimal)routesInicioFinal[l].latitud,
                                                    (decimal)routesInicioFinal[l].logintud,
                                                    routesInicioFinal[l + 1].latitud,
                                                    routesInicioFinal[l + 1].logintud);
                                                    kmTotales = kmTotales + calc;
                                                }
                                                routesInicioFinal = routes.Where(item2 => item2.numPoint >= inicioCheck.numPoint && item2.numPoint <= item3.inicio).ToList();
                                                for (int l = 0; l < routesInicioFinal.Count() - 1; l++)
                                                {
                                                    var calc = muestraRutasMapas.CalcularDistanciaHaversine(
                                                    (decimal)routesInicioFinal[l].latitud,
                                                    (decimal)routesInicioFinal[l].logintud,
                                                    routesInicioFinal[l + 1].latitud,
                                                    routesInicioFinal[l + 1].logintud);
                                                    km = km + calc;
                                                }
                                                for (int i = inicio.orden - 1; i < final.orden - 1; i++)
                                                {
                                                    if (DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i].minArrivalMarket.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        minutes = minutes + hour * 60 + minute;

                                                    }
                                                    else
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i].minArrival.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        minutes = minutes + hour * 60 + minute;
                                                    }
                                                }
                                                TimeSpan diferencia = boliviaTime.Subtract(DateTime.Parse(timeControl.dateRegister.ToString()));
                                                int minutesDiferenciaComparacion = (int)diferencia.TotalMinutes;
                                                kmTotales = Math.Round(kmTotales, 2);
                                                km = Math.Round(km, 2);
                                                int res = (int)(minutes * km / kmTotales);
                                                var chofer = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                                                if (minutesDiferenciaComparacion < res)
                                                {
                                                    List<Star> stars = new List<Star>();
                                                    stars = context.Star.Where(x => x.idEmployee == item.idEmployee).ToList();
                                                    var star = stars.Where(x => DateTime.Parse(x.dateRegister.ToString()).Month == boliviaTime.Month && DateTime.Parse(x.dateRegister.ToString()).Year == boliviaTime.Year).ToList();
                                                    double start = 5;
                                                    if (stars.Count > 0)
                                                    {
                                                        int countstars = 1;
                                                        foreach (var st in star)
                                                        {
                                                            countstars++;
                                                            start = start + double.Parse(st.numStar.ToString());
                                                        }
                                                        start = Math.Round(start / countstars, 2);
                                                    }
                                                    int result = res - minutesDiferenciaComparacion;
                                                    choferesTiempoAuxi.Add(new ChoferesTiempoAuxi(chofer.name + " " + chofer.lastname + "(★" + start + ")" + " min. aprox. " + result, result));
                                                }
                                            }
                                            else if (numInicioTimeControl.orden < final.orden)
                                            {
                                                var routesInicioFinal = routes.Where(item2 => item2.numPoint >= inicioCheck.numPoint && item2.numPoint <= finalCheck.numPoint).ToList();

                                                for (int l = 0; l < routesInicioFinal.Count() - 1; l++)
                                                {
                                                    var calc = muestraRutasMapas.CalcularDistanciaHaversine(
                                                    (decimal)routesInicioFinal[l].latitud,
                                                    (decimal)routesInicioFinal[l].logintud,
                                                    routesInicioFinal[l + 1].latitud,
                                                    routesInicioFinal[l + 1].logintud);
                                                    kmTotales = kmTotales + calc;
                                                }
                                                routesInicioFinal = routes.Where(item2 => item2.numPoint >= inicioCheck.numPoint && item2.numPoint <= item3.inicio).ToList();
                                                for (int l = 0; l < routesInicioFinal.Count() - 1; l++)
                                                {
                                                    var calc = muestraRutasMapas.CalcularDistanciaHaversine(
                                                    (decimal)routesInicioFinal[l].latitud,
                                                    (decimal)routesInicioFinal[l].logintud,
                                                    routesInicioFinal[l + 1].latitud,
                                                    routesInicioFinal[l + 1].logintud);
                                                    km = km + calc;
                                                }
                                                for (int i = final.orden - 2; i < final.orden - 1; i++)
                                                {
                                                    if (DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i].minArrivalMarket.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        minutes = minutes + hour * 60 + minute;

                                                    }
                                                    else
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i].minArrival.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        minutes = minutes + hour * 60 + minute;
                                                    }
                                                }
                                                TimeSpan diferencia = boliviaTime.Subtract(DateTime.Parse(timeControl.dateRegister.ToString()));
                                                int minutesDiferenciaComparacion = (int)diferencia.TotalMinutes;
                                                kmTotales = Math.Round(kmTotales, 2);
                                                km = Math.Round(km, 2);
                                                int res = (int)(minutes * km / kmTotales);
                                                var chofer = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                                                int time = 0;

                                                for (int i = numInicioTimeControl.orden - 1; i < final.orden - 2; i++)
                                                {
                                                    if (DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Wednesday || DateTime.Parse(timeControl.dateRegister.ToString()).DayOfWeek == DayOfWeek.Saturday)
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i].minArrivalMarket.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        time = time + hour * 60 + minute;

                                                    }
                                                    else
                                                    {
                                                        string[] calcularMinutes = ParadasAnteriores[i].minArrival.Split(':');
                                                        int hour = int.Parse(calcularMinutes[0]);
                                                        int minute = int.Parse(calcularMinutes[1]);
                                                        time = time + hour * 60 + minute;
                                                    }
                                                }
                                                res = time + res;
                                                if (minutesDiferenciaComparacion < res)
                                                {
                                                    List<Star> stars = new List<Star>();
                                                    stars = context.Star.Where(x => x.idEmployee == item.idEmployee).ToList();
                                                    var star = stars.Where(x => DateTime.Parse(x.dateRegister.ToString()).Month == boliviaTime.Month && DateTime.Parse(x.dateRegister.ToString()).Year == boliviaTime.Year).ToList();
                                                    double start = 5;
                                                    if (stars.Count > 0)
                                                    {
                                                        int countstars = 1;
                                                        foreach (var st in star)
                                                        {
                                                            countstars++;
                                                            start = start + double.Parse(st.numStar.ToString());
                                                        }
                                                        start = Math.Round(start / countstars, 2);
                                                    }
                                                    int result = res - minutesDiferenciaComparacion;
                                                    choferesTiempoAuxi.Add(new ChoferesTiempoAuxi(chofer.name + " " + chofer.lastname + "(★" + start + ")" + " min. aprox. " + result, result));
                                                }
                                            }
                                        }

                                        //choferesTiempo.Add(" "+ inicio.orden+" "+final.orden+" "+numInicioTimeControl.orden+" "+inicioCheck.numPoint+" "+finalCheck.numPoint);
                                    }
                                }

                            }
                        }
                    }
                    choferesTiempoAuxi = choferesTiempoAuxi.OrderBy(x => x.dateRegist).ToList();
                    choferesTiempoAuxi = choferesTiempoAuxi.Take(3).ToList();
                    foreach (var item in choferesTiempoAuxi)
                    {
                        choferesTiempo.Add(item.description);
                    }
                    muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                    muestraRutasMapas.ListParadasMostrar = nuevasParadas;
                    muestraRutasMapas.ListBloqueosMostrar = nuevosBloqueos;
                    mapa.Add(muestraRutasMapas);
                    if(!allLines.Any(x => x.nameLine == Line+" Ruta "+item3.numType))
                    allLines.Add(new MuestraTodasLineas(Line+" Ruta "+item3.numType, mapa, choferesTiempo));
                }
                return View(allLines);

            }
            catch (Exception)
            {
                throw;
            }
        }    
        public ActionResult actualizarCookie(string linea)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var cookieData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ticket.UserData);

                cookieData.Linea = linea;

                string userDataUpdated = Newtonsoft.Json.JsonConvert.SerializeObject(cookieData);

                FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(
                    ticket.Version,
                    ticket.Name,
                    ticket.IssueDate,
                    ticket.Expiration,
                    ticket.IsPersistent,
                    userDataUpdated,
                    ticket.CookiePath
                );

                string encryptedNewTicket = FormsAuthentication.Encrypt(newTicket);

                HttpCookie updatedAuthCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedNewTicket)
                {
                    HttpOnly = true,
                    Expires = newTicket.Expiration
                };

                Response.Cookies.Set(updatedAuthCookie);
            }

            return RedirectToAction("Index", "Home");
        }
        public ActionResult olvidarCookieLinea()
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                var cookieData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ticket.UserData);

                cookieData.Linea = "";  

                string userDataUpdated = Newtonsoft.Json.JsonConvert.SerializeObject(cookieData);

                FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(
                    ticket.Version,
                    ticket.Name,
                    ticket.IssueDate,
                    ticket.Expiration,
                    ticket.IsPersistent,
                    userDataUpdated,
                    ticket.CookiePath
                );

                string encryptedNewTicket = FormsAuthentication.Encrypt(newTicket);

                HttpCookie updatedAuthCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedNewTicket)
                {
                    HttpOnly = true,
                    Expires = newTicket.Expiration
                };
                Response.Cookies.Set(updatedAuthCookie);
            }
            return RedirectToAction("Index", "Home");

        }
    }
}
