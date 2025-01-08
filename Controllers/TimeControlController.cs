using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoControlLineaBus.Controllers
{
    public class TimeControlController : Controller
    {
        // GET: TimeControl
        public ActionResult Index()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(2);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string Employee = logi.GetEmployeeIdFromCookie(Request).ToString();
                List<TimeControl> timeControl = new List<TimeControl>();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                Checkpoint checkpoint = new Checkpoint();
                using (dbModels context = new dbModels())
                    checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == Employee).FirstOrDefault();
                if (checkpoint!=null)
                {
                    Employee employee = new Employee();
                    using (dbModels context = new dbModels())
                        timeControl = context.TimeControl.ToList().Where(x => x.idCheckpoint == checkpoint.idCheckpoint && DateTime.Parse(x.dateRegister.ToString()).ToString("yyyy-MM-dd") == boliviaTime.ToString("yyyy-MM-dd")).ToList();
                    ViewBag.Mensaje = "";
                    return View(timeControl);
                }
                ViewBag.Mensaje = "Aún no tienes asignado un punto de control";
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        // GET: TimeControl/Create
        public ActionResult Create()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(2);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string Employee = logi.GetEmployeeIdFromCookie(Request).ToString();
                Checkpoint checkpoint = new Checkpoint();
                List<Employee> em = new List<Employee>();
                MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                List<MuestraControlador> muestraControlador = new List<MuestraControlador>();
                int numTicket = 0; DateTime final = boliviaTime;
                int count = 0;
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.status != 0 && x.plate != null && x.enabled > 0 && x.idLine == Linea).ToList();
                if (em == null)
                {
                    TempData["Em"] = "Aun no hay choferes registrados";
                    return View();
                }
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == Employee).FirstOrDefault();
                foreach (var empleado in em)
                {
                    numTicket = 0;
                    final = boliviaTime;
                    List<string> retrasos = new List<string>();
                    List<string> incidencias = new List<string>();
                    int ordenCheck = 0;
                    int choferCheck = 0;
                    using (dbModels context = new dbModels())
                    {

                        var timeControls = context.TimeControl
                            .Where(x => x.idEmployee == empleado.idEmployee && x.dateRegister.HasValue)
                            .AsEnumerable()
                            .OrderBy(x => Math.Abs((x.dateRegister.Value - boliviaTime).TotalSeconds))
                            .ToList();

                        var timeControl = timeControls.FirstOrDefault();
                        var cverification = context.Checkpoint.Where(x => x.idCheckpoint == timeControl.idCheckpoint).FirstOrDefault();
                        int numTypeRoute = int.Parse(cverification.numberRoute.ToString());
                        if (timeControl != null)
                        {
                            if (timeControl.dateRegister != null && timeControl.numTicket != null)
                            {
                                var checkpoints = context.Checkpoint.Where(x => x.idLine == Linea && x.status != 0 && x.numberRoute == numTypeRoute).ToList();
                                var listCheckpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == Employee).ToList();
                                var checkpointsEnComun = checkpoints
                                .Where(cp => listCheckpoint.Any(lc => lc.idCheckpoint == cp.idCheckpoint))
                                .FirstOrDefault();
                                if (checkpointsEnComun != null)
                                {
                                    int statusPrimera = 2;
                                    if (timeControl.enable == 2 || timeControl.enable == 4) statusPrimera = 3;
                                    Checkpoint primerCheckpoint = checkpoints.FirstOrDefault(r => r.status == statusPrimera);
                                    if (primerCheckpoint != null)
                                    {
                                        checkpoints = checkpoints
                                            .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                                (decimal)primerCheckpoint.latitud,
                                                (decimal)primerCheckpoint.longitud,
                                                route.latitud,
                                                route.longitud))
                                            .ToList();
                                    }
                                    checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == Employee && x.numberRoute == numTypeRoute).FirstOrDefault();
                                    for (int i = 0; i < checkpoints.Count; i++)
                                    {
                                        count++;
                                        if (checkpoint.idCheckpoint == checkpoints[i].idCheckpoint) ordenCheck = count;
                                        if (timeControl.idCheckpoint == checkpoints[i].idCheckpoint) choferCheck = count;
                                    }
                                    if (timeControl.numTicket == null) { timeControl.numTicket = 0; numTicket = 0; }
                                    else { numTicket = (int)timeControl.numTicket; }
                                    if (timeControl.dateRegister != null) final = DateTime.Parse(timeControl.dateRegister.ToString());
                                }
                            }
                        }
                        if (ordenCheck > choferCheck)
                            muestraControlador.Add(new MuestraControlador(empleado.idEmployee, empleado.plate, null, null, final, numTicket));
                    }
                }
                muestraControlador = muestraControlador.OrderBy(x => x.numTicket).ToList();
                return View(muestraControlador);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // POST: TimeControl/Create
        [HttpPost]
        public ActionResult Create(string plate)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(2);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string Employee = logi.GetEmployeeIdFromCookie(Request).ToString();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                if (plate == null) return RedirectToAction("Create");
                Employee em = new Employee();
                using (dbModels context = new dbModels())
                {
                    em = context.Employee.Where(x => x.plate.ToLower().Trim() == plate.ToLower().Trim() && x.idLine == Linea).FirstOrDefault();
                }
                if (em == null)
                {
                    TempData["Em"] = "No existe ningún empleado con esa línea";
                    return View();
                }
                Checkpoint checkpoint = new Checkpoint();
                TimeControl time = new TimeControl();
                TimeControl timeAnterior = new TimeControl();
                using (dbModels context = new dbModels())
                {
                    int numTicket = context.TimeControl
                    .Where(x => x.plate == plate)
                    .Max(x => (int?)x.numTicket) ?? 0;
                    var times = context.TimeControl.Where(x => x.idEmployee == em.idEmployee && numTicket == x.numTicket).ToList();
                    var listChecks = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == Employee).ToList();
                    var idsCoincidentes = times
                    .Where(t => listChecks.Any(c => c.idCheckpoint == t.idCheckpoint))
                    .Select(t => t.idCheckpoint)
                    .FirstOrDefault();
                    checkpoint = context.Checkpoint.Where(x => x.idCheckpoint == idsCoincidentes && x.idLine == Linea && x.idEmployee == Employee).FirstOrDefault();
                    if (checkpoint != null)
                    {
                        time = context.TimeControl.Where(x => x.idCheckpoint == checkpoint.idCheckpoint && x.idEmployee == em.idEmployee && numTicket == x.numTicket).FirstOrDefault();
                        time.dateRegister = boliviaTime;
                        if (checkpoint.status == 3 || checkpoint.status == 2) { em.enabled = 0; context.Entry(em).State = System.Data.Entity.EntityState.Modified; }
                        if (numTicket != 1)
                        {
                            var timeControlChofer = context.TimeControl.Where(x => x.idCheckpoint == checkpoint.idCheckpoint).ToList();
                            var ticketCercano = timeControlChofer.Where(x => x.numTicket < numTicket).OrderByDescending(x => x.numTicket).FirstOrDefault();
                            if (ticketCercano != null)
                            {
                                timeAnterior = context.TimeControl.Where(x => x.idCheckpoint == checkpoint.idCheckpoint && x.numTicket == ticketCercano.numTicket).FirstOrDefault();
                                if (timeAnterior != null)
                                    if (timeAnterior.dateRegister == null)
                                    {
                                        timeAnterior.enable = timeAnterior.enable + 2;
                                        context.Entry(timeAnterior).State = System.Data.Entity.EntityState.Modified;
                                    }
                            }
                        }
                        context.Entry(time).State = System.Data.Entity.EntityState.Modified;

                        context.SaveChanges();
                    }
                   
                }
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
        public ActionResult OptionsPrincipalControl()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(2);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string idEmployee = logi.GetEmployeeIdFromCookie(Request).ToString();
                using (dbModels context = new dbModels())
                {
                    var checkpoint = context.Checkpoint.Where(x => x.idEmployee == idEmployee).FirstOrDefault();
                    if (checkpoint.status == 2 || checkpoint.status == 3)
                        return View();
                    else return RedirectToAction("Create", "TimeControl");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult SalidaCreate()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(2);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string Employee = logi.GetEmployeeIdFromCookie(Request).ToString();
                Checkpoint checkpoint = new Checkpoint();
                List<Employee> em = new List<Employee>();
                MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                List<MuestraControlador> muestraControlador = new List<MuestraControlador>();
                int numTicket = 0; DateTime final = boliviaTime;
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.status != 0 && x.plate != null && (x.enabled == null || x.enabled == 0) && x.idLine == Linea).ToList();
                if (em == null)
                {
                    TempData["Em"] = "Aun no hay choferes registrados";
                    return View();
                }
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == Employee).FirstOrDefault();
                if (checkpoint.status < 2) return RedirectToAction("Error", "Home");
                foreach (var empleado in em)
                {
                    numTicket = 0;
                    final = boliviaTime;
                    List<string> retrasos = new List<string>();
                    List<string> incidencias = new List<string>();
                    using (dbModels context = new dbModels())
                    {

                        var timeControls = context.TimeControl
                            .Where(x => x.idEmployee == empleado.idEmployee && x.dateRegister.HasValue)
                            .AsEnumerable()
                            .OrderBy(x => Math.Abs((x.dateRegister.Value - boliviaTime).TotalSeconds))
                            .ToList();

                        var timeControl = timeControls.FirstOrDefault();
                        if (timeControl != null)
                        {
                            var cverification = context.Checkpoint.Where(x => x.idCheckpoint == timeControl.idCheckpoint).FirstOrDefault();
                            int numTypeRoute = int.Parse(cverification.numberRoute.ToString());
                            if (timeControl.dateRegister != null && timeControl.numTicket != null)
                            {
                                timeControls = context.TimeControl.Where(x => x.numTicket == timeControl.numTicket && x.idEmployee == timeControl.idEmployee).ToList();

                                List<Checkpoint> checkpoints = context.Checkpoint.Where(x => x.idLine == Linea && x.status != 0 && x.numberRoute == numTypeRoute).ToList();
                                if (timeControls.Any())
                                {
                                    int statusPrimera = 2;
                                    if (timeControl.enable == 2 || timeControl.enable == 4) statusPrimera = 3;
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
                                            incidencias.Add("El conductor no llego a la " + check.description+ " RUTA "+check.numberRoute);
                                        }
                                        if (item.enable > 2 && item.dateRegister != null)
                                        {
                                            var check = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                            var timeControlChofer = context.TimeControl.Where(x => x.idCheckpoint == check.idCheckpoint).ToList();
                                            var ticketCercano = timeControlChofer.Where(x => x.numTicket > timeControl.numTicket).OrderBy(x => x.numTicket).FirstOrDefault();
                                            var timeControlChoferLePaso = context.TimeControl.Where(x => x.numTicket == ticketCercano.numTicket && x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                            var chofer = context.Person.Where(x => x.nit == timeControlChoferLePaso.idEmployee).FirstOrDefault();
                                            incidencias.Add("Al conductor le pasó " + chofer.name + " " + chofer.lastname + " en " + check.description + " RUTA " + check.numberRoute);
                                        }
                                        if (item.dateRegister != null)
                                        {
                                            var check = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                            retrasos.Add("Pasó " + check.description + " el: " + item.dateRegister + " RUTA " + check.numberRoute);
                                        }
                                    }
                                    List<TimeControl> orderTimeControl = new List<TimeControl>();
                                    foreach (var item in checkpointsorder)
                                    {
                                        TimeControl timeControlOrder = timeControls.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                                        orderTimeControl.Add(timeControlOrder);
                                    }
                                    if (timeControl.enable == 1 || timeControl.enable == 3)
                                    {
                                        for (int i = 0; i < orderTimeControl.Count - 1; i++)
                                        {
                                            if (orderTimeControl[i + 1].dateRegister.HasValue && orderTimeControl[i].dateRegister.HasValue)
                                            {
                                                var checkpointPrimer = checkpoints.Where(x => x.status == 2 && x.idLine == Linea).FirstOrDefault();
                                                var timeControlInicio = timeControls.Where(x => x.idCheckpoint == checkpointPrimer.idCheckpoint && timeControl.numTicket == x.numTicket).FirstOrDefault();
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
                                                    incidencias.Add("El conductor se retrazó en " + check.description + " con " + diferencia + "min." + " RUTA " + check.numberRoute);
                                                }
                                            }
                                        }
                                    }
                                    else if (timeControl.enable == 2 || timeControl.enable == 4)
                                    {
                                        for (int i = 0; i < checkpoints.Count - 1; i++)
                                        {
                                            if (orderTimeControl[i].dateRegister.HasValue && orderTimeControl[i + 1].dateRegister.HasValue)
                                            {
                                                var checkpointPrimer = checkpoints.Where(x => x.status == 3 && x.idLine == Linea).FirstOrDefault();
                                                var timeControlInicio = timeControls.Where(x => x.idCheckpoint == checkpointPrimer.idCheckpoint && timeControl.numTicket == x.numTicket).FirstOrDefault();
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
                                                    incidencias.Add("El conductor se retrazó en " + check.description + " con " + diferencia + "min." + " RUTA " + check.numberRoute);
                                                }
                                            }
                                        }
                                    }
                                    if (timeControl.numTicket == null) { timeControl.numTicket = 0; numTicket = 0; }
                                    else { numTicket = (int)timeControl.numTicket; }
                                    if (timeControl.dateRegister != null) final = DateTime.Parse(timeControl.dateRegister.ToString());
                                }
                            }
                        }
                        muestraControlador.Add(new MuestraControlador(empleado.idEmployee, empleado.plate, retrasos, incidencias, final, numTicket));
                    }
                }
                List<Checkpoint> checkpointss = new List<Checkpoint>();
                using (dbModels context = new dbModels())
                    checkpointss = context.Checkpoint.Where(x => x.idEmployee ==Employee && x.status > 0).ToList();
                muestraControlador = muestraControlador.OrderBy(x => x.numTicket).ToList();
                int firstCheckpointId = checkpointss.FirstOrDefault()?.numberRoute ?? 0;
                ViewBag.Checkpoints = new SelectList(checkpointss, "numberRoute", "numberRoute", firstCheckpointId);
                return View(muestraControlador);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
            

        }
        [HttpPost]
        public ActionResult SalidaCreate(string plate, int SelectedCheckpointId)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(2);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string Employee = logi.GetEmployeeIdFromCookie(Request).ToString();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                Employee em = new Employee();
                using (dbModels context = new dbModels())
                {
                    em = context.Employee.Where(x => x.plate.ToLower().Trim() == plate.ToLower().Trim() && x.idLine == Linea).FirstOrDefault();
                }
                if (em == null)
                {
                    TempData["Em"] = "No existe ningún empleado con esa línea";
                    return View();
                }
                Checkpoint checkpoint = new Checkpoint();
                List<Checkpoint> checksver = new List<Checkpoint>();
                List<int> numTicketss = new List<int>();
                
                using (dbModels context = new dbModels())
                {
                    checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == Employee && x.numberRoute == SelectedCheckpointId).FirstOrDefault();
                    var checks = context.Checkpoint.Where(x => x.idLine == Linea && (x.status == 2 || x.status == 3)).ToList();
                    foreach (var item in checks)
                    {
                        int timec = context.TimeControl
                        .Where(x => x.idCheckpoint == item.idCheckpoint)
                        .Select(x => (int?)x.numTicket)
                        .Max() ?? 0;
                        numTicketss.Add(timec);
                    }
                }
                using (dbModels context = new dbModels())
                {
                    int numTicket = numTicketss.Max();
                    var checks = context.Checkpoint.Where(x => x.idLine == Linea && x.status != 0 && x.numberRoute == SelectedCheckpointId).ToList();
                    int enabledEmployee = 1; int enableTimeControl = 1;
                    if (checkpoint.status == 3) { enabledEmployee = 2; enableTimeControl = 2; }
                    if (checks.Count > 0)
                    {
                        numTicket++;
                        foreach (var check in checks)
                        {
                            TimeControl time = new TimeControl();
                            if (checkpoint.idCheckpoint != check.idCheckpoint) time.dateRegister = null;
                            else time.dateRegister = boliviaTime;
                            time.idCheckpoint = check.idCheckpoint;
                            time.idEmployee = em.idEmployee;
                            time.plate = plate.ToUpper().Trim();
                            time.enable = enableTimeControl;
                            time.numTicket = numTicket;

                            context.TimeControl.Add(time);
                        }
                        em.enabled = enabledEmployee;
                        context.Entry(em).State = System.Data.Entity.EntityState.Modified;
                    }
                    context.SaveChanges();
                }
                return RedirectToAction("SalidaCreate");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
