using Newtonsoft.Json;
using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace ProyectoControlLineaBus.Controllers
{
    public class ChoferController : Controller
    {
        // GET: Chofer
        public ActionResult Index()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(1);
            if (authResult != null) return authResult;
            return View();
            
        }
        public ActionResult ListRoutes()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(1);
            if (authResult != null) return authResult;
            string Line = logi.GetLineaFromCookie(Request).ToString();
            List<Route> routes = new List<Route>();
            using (dbModels context = new dbModels())
            {
                routes = context.Route.Where(x => x.idLine == Line).GroupBy(x => x.numTypeRoute).Select(x => x.FirstOrDefault()).ToList();
            }
            return View(routes);
        }

        public ActionResult ViewRuta1(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(1);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<MuestraRutasMapas> mapa = new List<MuestraRutasMapas>();
                int count = 0;
                for (int i = 1; i <= 1; i++)
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
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status != 0 && x.numberRoute == numTypeRoute).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();
                        var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute).ToList();
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
                    ViewBag.numTypeRoute = numTypeRoute;
                }

                return View(mapa);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ViewRuta2(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(1);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<MuestraRutasMapas> mapa = new List<MuestraRutasMapas>();
                int count = 0;
                for (int i = 2; i <= 2; i++)
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
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status != 0 && x.numberRoute == numTypeRoute).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();
                        var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute).ToList();
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
                    ViewBag.numTypeRoute = numTypeRoute;
                    mapa.Add(muestraRutasMapas);
                }

                return View(mapa);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }


        // GET: Chofer/Create
        public ActionResult ViewRutas()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(1);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<MuestraRutasMapas> mapa = new List<MuestraRutasMapas>();
                int count = 0;
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
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status != 0 && x.numberRoute == 0).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i)
                            .ToList();
                        var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == i).ToList();
                        if (deviation.Count > 0)
                        {
                            foreach (var item in deviation)
                            {
                                List<RutasMostrar> listBloqueos = new List<RutasMostrar>();
                                var route1 = context.Route.Where(x => x.idRoute == item.idRouteStart && x.idLine == Line).FirstOrDefault();
                                var route2 = context.Route.Where(x => x.idRoute == item.idRouteEnd && x.idLine == Line).FirstOrDefault();
                                var routesDeviation = context.Route.Where(x => x.numPoint >= route1.numPoint && x.numPoint <= route2.numPoint && x.idLine == Line).ToList();
                                foreach (var item2 in routesDeviation)
                                {
                                    listBloqueos.Add(new RutasMostrar(item2.idRoute, item2.latitud, item2.logintud, "", item2.status, (int)item2.numPoint));
                                }
                                var place1 = context.Place.Where(x => x.idPlace == route1.idPlace).FirstOrDefault();
                                var place2 = context.Place.Where(x => x.idPlace == route2.idPlace).FirstOrDefault();
                                nuevosBloqueos.Add(new BloqueosMostrar(item.idDeviation, "Bloqueo " + place1.description + " y " + place2.description, listBloqueos));
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

                return View(mapa);
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
        public ActionResult ReportRuta1Deviation(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(1);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<MuestraRutasMapas> mapa = new List<MuestraRutasMapas>();
                int count = 0;
                for (int i = 1; i <= 1; i++)
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
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status != 0 && x.numberRoute == numTypeRoute).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();
                        var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute).ToList();
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
                    ViewBag.numTypeRoute = numTypeRoute;
                }

                return View(mapa);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public ActionResult ReportRuta1Deviation(string ubicaciones, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(1);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();

                string[] ubicacionesConvert = ubicaciones.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (ubicacionesConvert.Length == 2)
                {
                    // Procesar la primera ubicación
                    string[] primeraUbicacion = ubicacionesConvert[0].Split('/');
                    decimal latitud1 = Convert.ToDecimal(primeraUbicacion[0], System.Globalization.CultureInfo.InvariantCulture);
                    decimal longitud1 = Convert.ToDecimal(primeraUbicacion[1], System.Globalization.CultureInfo.InvariantCulture);

                    // Procesar la segunda ubicación
                    string[] segundaUbicacion = ubicacionesConvert[1].Split('/');
                    decimal latitud2 = Convert.ToDecimal(segundaUbicacion[0], System.Globalization.CultureInfo.InvariantCulture);
                    decimal longitud2 = Convert.ToDecimal(segundaUbicacion[1], System.Globalization.CultureInfo.InvariantCulture);

                    List<Route> RouteList = new List<Route>(); List<Route> RouteOrder = new List<Route>(); List<Route> RouteOrder2 = new List<Route>();
                    MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas();
                    using (dbModels context = new dbModels()) RouteList = context.Route.Where(x => x.idLine == Line && x.numberRoute == 1 && x.status != 2 && x.numTypeRoute == numTypeRoute).ToList();
                    List<Route> RouteList2 = RouteList;
                    int idInicio = 0; int idFinal = 0; bool verificacion = false;
                    while (verificacion == false)
                    {
                        RouteOrder = RouteList
                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                        (decimal)latitud1,
                                        (decimal)longitud1,
                                        (decimal)route.latitud,
                                        (decimal)route.logintud))
                                    .ToList();
                        RouteOrder2 = RouteList2
                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                        (decimal)latitud2,
                                        (decimal)longitud2,
                                        (decimal)route.latitud,
                                        (decimal)route.logintud))
                                    .ToList();
                        if (RouteOrder[0] == null || RouteOrder2[0] == null)
                        {
                            return RedirectToAction("Index", "Chofer");
                        }
                        if (RouteOrder[0].numPoint > RouteOrder2[0].numPoint)
                        {
                            idInicio = int.Parse(RouteOrder2[0].idRoute.ToString());
                            idFinal = int.Parse(RouteOrder[0].idRoute.ToString());
                        }
                        else
                        {
                            idFinal = int.Parse(RouteOrder2[0].idRoute.ToString());
                            idInicio = int.Parse(RouteOrder[0].idRoute.ToString());
                        }
                        if (idInicio != idFinal) verificacion = true;
                    }
                    using (dbModels context = new dbModels())
                    {
                        DeviationRote deviationRoute = new DeviationRote
                        {
                            numberRoute = 1,
                            idLine = Line,
                            idRouteStart = idInicio,
                            idRouteEnd = idFinal,
                            numTypeRoute = numTypeRoute
                        };
                        context.DeviationRote.Add(deviationRoute);
                        context.SaveChanges();
                    }
                    return RedirectToAction("ViewRuta1", "Chofer", new { numTypeRoute  = numTypeRoute });
                }
                else return RedirectToAction("ViewRuta1", "Chofer", new { numTypeRoute = numTypeRoute });
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportRuta2Deviation(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(1);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<MuestraRutasMapas> mapa = new List<MuestraRutasMapas>();
                int count = 0;
                for (int i = 2; i <= 2; i++)
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
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status != 0 && x.numberRoute == numTypeRoute).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();
                        var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute).ToList();
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
                    ViewBag.numTypeRoute = numTypeRoute;
                }

                return View(mapa);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public ActionResult ReportRuta2Deviation(string ubicaciones, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(1);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();

                string[] ubicacionesConvert = ubicaciones.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (ubicacionesConvert.Length == 2)
                {
                    // Procesar la primera ubicación
                    string[] primeraUbicacion = ubicacionesConvert[0].Split('/');
                    decimal latitud1 = Convert.ToDecimal(primeraUbicacion[0], System.Globalization.CultureInfo.InvariantCulture);
                    decimal longitud1 = Convert.ToDecimal(primeraUbicacion[1], System.Globalization.CultureInfo.InvariantCulture);

                    // Procesar la segunda ubicación
                    string[] segundaUbicacion = ubicacionesConvert[1].Split('/');
                    decimal latitud2 = Convert.ToDecimal(segundaUbicacion[0], System.Globalization.CultureInfo.InvariantCulture);
                    decimal longitud2 = Convert.ToDecimal(segundaUbicacion[1], System.Globalization.CultureInfo.InvariantCulture);

                    List<Route> RouteList = new List<Route>(); List<Route> RouteOrder = new List<Route>(); List<Route> RouteOrder2 = new List<Route>();
                    MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas();
                    using (dbModels context = new dbModels()) RouteList = context.Route.Where(x => x.idLine == Line && x.numberRoute == 2 && x.status != 2 && x.numTypeRoute == numTypeRoute).ToList();
                    List<Route> RouteList2 = RouteList;
                    int idInicio = 0; int idFinal = 0; bool verificacion = false;
                    while (verificacion == false)
                    {
                        RouteOrder = RouteList
                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                        (decimal)latitud1,
                                        (decimal)longitud1,
                                        (decimal)route.latitud,
                                        (decimal)route.logintud))
                                    .ToList();
                        RouteOrder2 = RouteList2
                                    .OrderBy(route => muestraRutasMapas.CalcularDistanciaHaversine(
                                        (decimal)latitud2,
                                        (decimal)longitud2,
                                        (decimal)route.latitud,
                                        (decimal)route.logintud))
                                    .ToList();
                        if (RouteOrder[0] == null || RouteOrder2[0] == null)
                        {
                            return RedirectToAction("Index", "Chofer");
                        }
                        if (RouteOrder[0].numPoint > RouteOrder2[0].numPoint)
                        {
                            idInicio = int.Parse(RouteOrder2[0].idRoute.ToString());
                            idFinal = int.Parse(RouteOrder[0].idRoute.ToString());
                        }
                        else
                        {
                            idFinal = int.Parse(RouteOrder2[0].idRoute.ToString());
                            idInicio = int.Parse(RouteOrder[0].idRoute.ToString());
                        }
                        if (idInicio != idFinal) verificacion = true;
                    }
                    using (dbModels context = new dbModels())
                    {
                        DeviationRote deviationRoute = new DeviationRote
                        {
                            numberRoute = 2,
                            idLine = Line,
                            idRouteStart = idInicio,
                            idRouteEnd = idFinal,
                            numTypeRoute = numTypeRoute
                        };
                        context.DeviationRote.Add(deviationRoute);
                        context.SaveChanges();
                    }
                    return RedirectToAction("ViewRuta2", "Chofer", new { numTypeRoute = numTypeRoute });
                }
                else return RedirectToAction("ViewRuta2", "Chofer", new { numTypeRoute = numTypeRoute });
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult reportChoferBusMechanicalFailures(string description)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(1);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                string Employe = logi.GetEmployeeIdFromCookie(Request).ToString();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                Employee employee = new Employee();
                using (dbModels context = new dbModels())
                {
                    employee = context.Employee.Where(x => x.status == 1 && x.idEmployee == Employe && x.idLine == Line).FirstOrDefault();
                    if (employee == null) return RedirectToAction("Error", "Home");
                    MechanicalFailures report = new MechanicalFailures();
                    report.idEmployee = employee.idEmployee; 
                    report.plate = employee.plate;
                    report.description = description;
                    report.dateRegister = boliviaTime;
                    context.MechanicalFailures.Add(report);
                    context.SaveChanges();
                }
                return RedirectToAction("Index", "Chofer");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult reportBusMechanicalFailures(string plate, string description)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                Employee employee = new Employee();
                using (dbModels context = new dbModels())
                {
                    employee = context.Employee.Where(x => x.status == 1 && x.plate == plate && x.idLine == Line).FirstOrDefault();
                    if (employee == null) return RedirectToAction("Error", "Home");
                    MechanicalFailures report = new MechanicalFailures();
                    report.idEmployee = employee.idEmployee;
                    report.plate = plate;
                    report.description = description;
                    report.dateRegister = boliviaTime;
                    context.MechanicalFailures.Add(report);
                    context.SaveChanges();
                }
                return RedirectToAction("Index", "Employee");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
