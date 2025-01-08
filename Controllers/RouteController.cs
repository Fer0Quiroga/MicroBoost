using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Web.Mvc;
using System.Globalization;
using System.Threading;
using System.Web.UI.WebControls;
using Login = ProyectoControlLineaBus.Models.Login;



namespace ProyectoControlLineaBus.Controllers
{
    public class RouteController : Controller
    {
        // GET: Route
        public ActionResult Index(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
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
                                    nuevosBloqueos.Add(new BloqueosMostrar(item.idDeviation, "Desvío " + place1.description + " y " + place2.description, listBloqueos));
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
                    ViewBag.numTypeRoute = numTypeRoute;
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
        public ActionResult ListRoutes()
        {

                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<Route> routes = new List<Route>();
                using (dbModels context = new dbModels())
                {
                    routes = context.Route.Where(x => x.idLine == Line).GroupBy(x => x.numTypeRoute).Select(x => x.FirstOrDefault()).ToList();
                }
                return View(routes);

        }

        // GET: Route/Create
        public ActionResult Create()
        {
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            return View();
        }

        // POST: Route/Create
        [HttpPost]
        public ActionResult Create(MuestraRoute muestraRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string description = muestraRoute.extention.Trim() + " " + muestraRoute.descriptionPlace.Trim();
                Place place = new Place();
                using (dbModels context = new dbModels())
                {
                    place = context.Place.Where(x => x.description.ToLower().Trim() == description.ToLower().Trim()).FirstOrDefault();
                }
                if (place == null)
                    using (dbModels context = new dbModels())
                    {
                        place = new Place();
                        place.description = description;
                        context.Place.Add(place);
                        context.SaveChanges();
                    }
                using (dbModels context = new dbModels())
                {
                    place = context.Place.Where(x => x.description.ToLower().Trim() == description.ToLower().Trim()).FirstOrDefault();
                    Route route = new Route();
                    decimal latitud = decimal.Parse(muestraRoute.latitud_longitud.Split(',')[0].ToString());
                    decimal longitud = decimal.Parse(muestraRoute.latitud_longitud.Split(',')[1].ToString());
                    route.idLine = Linea;
                    route.idPlace = place.idPlace;
                    route.latitud = latitud;
                    route.logintud = longitud;
                    context.Route.Add(route);
                    context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        // GET: Route/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Route route = new Route();
                int numTypeRoute = 1;
                using (dbModels context = new dbModels())
                {
                    route = context.Route.Where(x => x.idRoute == id).FirstOrDefault();
                    numTypeRoute = int.Parse(route.numTypeRoute.ToString());
                    var deviation = context.DeviationRote.Where(x => x.idRouteEnd == route.idRoute || x.idRouteStart == route.idRoute).ToList();
                    context.Route.Remove(route);
                    if (deviation.Count > 0)
                    {
                        foreach (var item in deviation)
                        {
                            context.DeviationRote.Remove(item);
                        }
                    }
                    context.SaveChanges();
                }
                return RedirectToAction("Index","Route", new { numTypeRoute = numTypeRoute });
            }
            catch
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
        public ActionResult IndexCheckAndRoute()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<MuestraRutasMapas> mapa = new List<MuestraRutasMapas>();
                int count = 0; 
                for (int i = 1; i <= 4; i++)
                {
                    MuestraRutasMapas muestraRutasMapas = new MuestraRutasMapas
                    {
                        ListRutasMostrar = new List<RutasMostrar>(),
                        ListParadasMostrar = new List<ParadasMostrar>(),
                        NumeroRuta = i
                    };

                    List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                    List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();

                    using (dbModels context = new dbModels())
                    {
                        var checkpoints =  context.Checkpoint.Where(x => x.idLine == Line && x.status == 1 && x.numberRoute == 0).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i)
                            .ToList();

                        if (routes.Count > 0)
                        {
                            count = 0;
                            foreach (var item in routes)
                            {
                                count++;
                                Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                                if (place != null)
                                {
                                    nuevasRutas.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description +"-" +count, item.status, (int)item.numPoint));
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
                        }

                        if (checkpoints.Count > 0)
                        {
                            count = 0;
                            foreach (var item in checkpoints)
                            {
                                count++;
                                nuevasParadas.Add(new ParadasMostrar(item.idCheckpoint, item.description + "-" + count + "", item.status, item.latitud, item.longitud));
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
                        }
                    }
                    muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                    muestraRutasMapas.ListParadasMostrar = nuevasParadas;
                    mapa.Add(muestraRutasMapas);
                }

                return View(mapa);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult IndexUbicacionesRuta(int NumRuta, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                if (NumRuta.ToString() == null) return RedirectToAction("IndexCheckAndRoute", "Route");
                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<RutasMostrar> rutasList = new List<RutasMostrar>();

                using (dbModels context = new dbModels())
                {
                    var routes = context.Route
                        .Where(x => x.idLine == Line && x.numberRoute == NumRuta && x.numTypeRoute == numTypeRoute)
                        .OrderBy(x => x.numPoint)
                        .ToList();

                    foreach (var item in routes)
                    {
                        Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                        if (place != null)
                        {
                            rutasList.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description, item.status, (int)item.numPoint));
                        }
                    }
                }
                ViewBag.RouteChunks = rutasList;
                ViewBag.NumRuta = NumRuta;
                Session["numRuta"] = NumRuta;
                ViewBag.numTypeRoute = numTypeRoute;
                return View();
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpPost]
        public ActionResult IndexUbicacionesRuta(string ubicaciones, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                if (Session["numRuta"] == null) return RedirectToAction("IndexCheckAndRoute", "Route");
                if (ubicaciones == null) return RedirectToAction("IndexCheckAndRoute", "Route");
                string Line = logi.GetLineaFromCookie(Request).ToString();
                int numRut = int.Parse(Session["numRuta"].ToString());

                // Deserializar el JSON a un array de strings
                string[] ubicacionesConvert = JsonConvert.DeserializeObject<string[]>(ubicaciones);
                int? maxNumPoint = 0;
                using (dbModels context = new dbModels()) maxNumPoint = context.Route.Where(x => x.idLine == Line).Max(x => x.numPoint);
                int count = 0; int? numPoint = maxNumPoint;
                using (dbModels context = new dbModels())
                {
                    foreach (var item in ubicacionesConvert)
                    {
                        count++;
                        if (!string.IsNullOrEmpty(item))
                        {
                            // Dividir la cadena en nombre, latitud y longitud
                            string[] seccionesNombreLatitudLongitud = item.Split('/');

                            if (seccionesNombreLatitudLongitud.Length >= 3) // Asegurarse de que hay suficientes datos
                            {
                                string description = seccionesNombreLatitudLongitud[0].Trim();
                                if (description == null) return RedirectToAction("IndexCheckAndRoute", "Route");
                                decimal latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                decimal longitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                                Place place = context.Place.FirstOrDefault(x => x.description == description);
                                if (place == null)
                                {
                                    place = new Place
                                    {
                                        description = description
                                    };
                                    context.Place.Add(place);
                                    context.SaveChanges(); // Guardar para obtener el idPlace generado
                                }
                                numPoint++;
                                // Crear la nueva ruta (Route)
                                Route route = new Route
                                {
                                    numberRoute = numRut,
                                    idLine = Line,
                                    status = 0,
                                    latitud = latitud,
                                    logintud = longitud,
                                    idPlace = place.idPlace,
                                    numPoint = numPoint,
                                    numTypeRoute = numTypeRoute
                                };

                                context.Route.Add(route);
                            }
                        }
                    }
                    context.SaveChanges(); // Guardar todos los cambios
                }

                return RedirectToAction("IndexUbicacionesRuta", "Route", new { NumRuta = numRut, numTypeRoute });
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult RevertDelete(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Route route = new Route();
                if (Session["numRuta"] == null) return RedirectToAction("IndexCheckAndRoute", "Route");
                string Line = logi.GetLineaFromCookie(Request).ToString();
                int numRut = int.Parse(Session["numRuta"].ToString());
                int? maxNumPoint = 0;
                using (dbModels context = new dbModels()) maxNumPoint = context.Route.Where(x => x.idLine == Line && x.numberRoute == numRut && x.numTypeRoute == numTypeRoute).Max(x => x.numPoint);
                if(maxNumPoint > 1)
                using (dbModels context = new dbModels())
                {
                    route = context.Route.Where(x => x.numPoint == maxNumPoint && x.numberRoute == numRut && x.numTypeRoute == numTypeRoute && x.idLine == Line).FirstOrDefault();
                    var deviation = context.DeviationRote.Where(x => x.idRouteEnd == route.idRoute || x.idRouteStart == route.idRoute).ToList();
                    context.Route.Remove(route);
                    if (deviation.Count > 0)
                    {
                        foreach (var item in deviation)
                        {
                            context.DeviationRote.Remove(item);
                        }
                    }
                    context.SaveChanges();
                }
                return RedirectToAction("IndexUbicacionesRuta", "Route", new { NumRuta = numRut, numTypeRoute = numTypeRoute });
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult RegisterCheck(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
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
                        NumeroRuta = i
                    };

                    List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                    List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();

                    using (dbModels context = new dbModels())
                    {
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status == 1 && x.numberRoute == numTypeRoute).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();

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
                            foreach (var item in nuevasParadas)
                            {
                                count++;
                                item.description = item.description + "-" + count;
                            }
                        }

                        if (checkpoints.Count > 0)
                        {
                            count = 0;
                            foreach (var item in checkpoints)
                            {
                                
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
                            foreach (var item in nuevasParadas)
                            {
                                count++;
                                item.description = item.description + "-" + count;
                            }
                        }
                    }
                    muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                    muestraRutasMapas.ListParadasMostrar = nuevasParadas;
                    mapa.Add(muestraRutasMapas);
                }
                ViewBag.numTypeRoute = numTypeRoute;
                return View(mapa);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public ActionResult RegisterCheck(string ubicaciones, string hora, string minuto, string horaMercado, string minutoMercado, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                if (ubicaciones == null) return RedirectToAction("RegisterCheck", "Route");


                using (dbModels context = new dbModels())
                {
                    // Dividir la cadena en nombre, latitud y longitud
                    string[] seccionesNombreLatitudLongitud = ubicaciones.Split('/');

                    if (seccionesNombreLatitudLongitud.Length >= 3) // Asegurarse de que hay suficientes datos
                    {
                        string horaCompleta = hora + ":" + minuto;
                        string horaCompletaMercado = horaMercado + ":" + minutoMercado;
                        string description = seccionesNombreLatitudLongitud[0].Trim();
                        if (description == null) return RedirectToAction("IndexCheckAndRoute", "Route");
                        decimal latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        decimal longitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        // Crear la nueva ruta (Route)
                        Checkpoint checkpoint = new Checkpoint
                        {
                            numberRoute = numTypeRoute,
                            idLine = Line,
                            status = 1,
                            latitud = latitud,
                            longitud = longitud,
                            description = "Parada " + description,
                            minArrival = horaCompleta,
                            minArrivalMarket = horaCompletaMercado
                        };
                        context.Checkpoint.Add(checkpoint);
                    }


                    context.SaveChanges(); // Guardar todos los cambios
                }
                return RedirectToAction("RegisterCheck", "Route", new { numTypeRoute = numTypeRoute});
            }
            catch (Exception)
            {

                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult EditCheck(int id, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
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
                        NumeroRuta = i
                    };

                    List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                    List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();

                    using (dbModels context = new dbModels())
                    {
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status == 1 && x.numberRoute == numTypeRoute && x.idCheckpoint == id).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();

                        if (routes.Count > 0)
                        {
                            count = 0;
                            foreach (var item in routes)
                            {
                                count++;
                                Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                                if (place != null)
                                {
                                    nuevasRutas.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description + "-" + count, item.status, (int)item.numPoint));
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
                        }

                        if (checkpoints.Count > 0)
                        {
                            
                            foreach (var item in checkpoints)
                            {
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
                        }
                    }
                    List<string> horas = new List<string>();
                    List<string> minutos = new List<string> {"00","10", "20", "30", "40", "50"};
                    for (int hour = 0; hour < 24; hour++)
                    {
                        horas.Add(hour.ToString("D2"));
                    }
                    Checkpoint check = new Checkpoint();
                    using (dbModels context = new dbModels()) check = context.Checkpoint.Where(x => x.idCheckpoint == id).FirstOrDefault();
                        // Separar la descripción en horas y minutos
                    string[] HourMinArrival = check.minArrival.Trim().Split(':');
                    string[] HourMinArrivalMarket = check.minArrivalMarket.Trim().Split(':');
                    // Asignar los SelectList al ViewBag
                    ViewBag.Hours = new SelectList(horas, HourMinArrival[0]);
                    ViewBag.Minutes = new SelectList(minutos, HourMinArrival[1]);
                    ViewBag.Hours2 = new SelectList(horas, HourMinArrivalMarket[0]);
                    ViewBag.Minutes2 = new SelectList(minutos, HourMinArrivalMarket[1]);
                    muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                    muestraRutasMapas.ListParadasMostrar = nuevasParadas;
                    ViewBag.numTypeRoute = numTypeRoute;
                    Session["idEditChek"] = id;
                    mapa.Add(muestraRutasMapas);
                }

                return View(mapa);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public ActionResult EditCheck(string ubicaciones, string hora, string minuto, string horaMercado, string minutoMercado)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                if(Session["idEditChek"] == null) return RedirectToAction("Error", "Home");
                string Line = logi.GetLineaFromCookie(Request).ToString();
                int id = int.Parse(Session["idEditChek"].ToString());
                Checkpoint check = new Checkpoint();
                using (dbModels context = new dbModels()) check = context.Checkpoint.Where(x => x.idCheckpoint == id).FirstOrDefault();
                using (dbModels context = new dbModels())
                {
                    // Dividir la cadena en nombre, latitud y longitud
                    string[] seccionesNombreLatitudLongitud = ubicaciones.Split('/');
                    string horaCompleta = hora + ":" + minuto;
                    string horaCompletaMercado = horaMercado + ":" + minutoMercado;
                    if (seccionesNombreLatitudLongitud.Length >= 3) // Asegurarse de que hay suficientes datos
                    {
                        string description = seccionesNombreLatitudLongitud[0].Trim();
                        check.description = "Parada "+description;
                        check.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        check.longitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    check.minArrival = horaCompleta;
                    check.minArrivalMarket = horaCompletaMercado;
                    context.Entry(check).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges(); // Guardar todos los cambios
                }
                return RedirectToAction("Index", "Route", new { numTypeRoute  = check.numberRoute});
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ViewDeviation1(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
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
                                    nuevosBloqueos.Add(new BloqueosMostrar(item.idDeviation, "Desvío " + place1.description + " y " + place2.description, listBloqueos, (double)route1.latitud, (double)route2.logintud));
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
        public ActionResult ViewDeviation2(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
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
                                if (route1 != null && route2!=null )
                                {
                                    var routesDeviation = routes.Where(x => x.numPoint >= route1.numPoint && x.numPoint <= route2.numPoint && x.idLine == Line).ToList();
                                    foreach (var item2 in routesDeviation)
                                    {
                                        listBloqueos.Add(new RutasMostrar(item2.idRoute, item2.latitud, item2.logintud, "", item2.status, (int)item2.numPoint));
                                    }
                                    var place1 = context.Place.Where(x => x.idPlace == route1.idPlace).FirstOrDefault();
                                    var place2 = context.Place.Where(x => x.idPlace == route2.idPlace).FirstOrDefault();
                                    nuevosBloqueos.Add(new BloqueosMostrar(item.idDeviation, "Desvío " + place1.description + " y " + place2.description, listBloqueos, (double)route1.latitud, (double)route2.logintud));
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
        public ActionResult DeleteDeviation1(int id)
        {
            try
            {
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                DeviationRote deviationRoute = new DeviationRote();
                int numTypeRoute = 1;
                using (dbModels context = new dbModels())
                {
                    deviationRoute = context.DeviationRote.Where(x => x.idDeviation == id).FirstOrDefault();
                    var route = context.Route.Where(x => x.idRoute == deviationRoute.idRouteStart).FirstOrDefault();
                    numTypeRoute = int.Parse(route.numTypeRoute.ToString());
                    context.DeviationRote.Remove(deviationRoute);
                    context.SaveChanges();
                }
                return RedirectToAction("ViewDeviation1", new { numTypeRoute  = numTypeRoute });
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult DeleteDeviation2(int id)
        {
            try
            {
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                DeviationRote deviationRoute = new DeviationRote();
                int numTypeRoute = 1;
                using (dbModels context = new dbModels())
                {
                    deviationRoute = context.DeviationRote.Where(x => x.idDeviation == id).FirstOrDefault();
                    var route = context.Route.Where(x => x.idRoute == deviationRoute.idRouteStart).FirstOrDefault();
                    numTypeRoute = int.Parse(route.numTypeRoute.ToString());
                    context.DeviationRote.Remove(deviationRoute);
                    context.SaveChanges();
                }
                return RedirectToAction("ViewDeviation2", new { numTypeRoute = numTypeRoute });
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ReportRuta1Deviation(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<Route> routesVer = new List<Route>();
                using (dbModels context = new dbModels())
                    routesVer = context.Route.Where(x => x.numberRoute == 1 && x.idLine == Line && x.numTypeRoute == numTypeRoute).ToList();
                if (routesVer.Count < 3) return RedirectToAction("ViewDeviation1", "Route", new { numTypeRoute = numTypeRoute });
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
                        var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == i && x.idLine == Line && x.numTypeRoute == numTypeRoute).ToList();
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
                                    nuevosBloqueos.Add(new BloqueosMostrar(item.idDeviation, "Desvío " + place1.description + " y " + place2.description, listBloqueos, (double)route1.latitud, (double)route2.logintud));
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
        [HttpPost]
        public ActionResult ReportRuta1Deviation(string ubicaciones, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();

                string[] ubicacionesConvert = ubicaciones.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                List<Route> routes = new List<Route>();
                using (dbModels context = new dbModels())
                    routes = context.Route.Where(x => x.numberRoute == 1 && x.idLine == Line && x.status != 2 && x.numTypeRoute == numTypeRoute).ToList();
                if (routes.Count < 2) return RedirectToAction("ViewDeviation2", "Route", new { numTypeRoute = numTypeRoute });
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
                    int idInicio = 0; int idFinal = 0; bool verificacion = false; int count = 0;
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
                        if (RouteOrder[0] == null || RouteOrder2[count] == null)
                        {
                            return RedirectToAction("ViewDeviation1", "Route", new { numTypeRoute  = numTypeRoute });
                        }
                        if (RouteOrder[0].numPoint > RouteOrder2[count].numPoint)
                        {
                            idInicio = int.Parse(RouteOrder2[count].idRoute.ToString());
                            idFinal = int.Parse(RouteOrder[0].idRoute.ToString());
                        }
                        else
                        {
                            idFinal = int.Parse(RouteOrder2[count].idRoute.ToString());
                            idInicio = int.Parse(RouteOrder[0].idRoute.ToString());
                        }
                        if (idInicio != idFinal) verificacion = true;
                        count++;
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
                    return RedirectToAction("ViewDeviation1", "Route", new { numTypeRoute = numTypeRoute });
                }
                else return RedirectToAction("ViewDeviation1", "Route", new { numTypeRoute = numTypeRoute });
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
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;

                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<Route> routesVer = new List<Route>();
                using (dbModels context = new dbModels())
                    routesVer = context.Route.Where(x => x.numberRoute == 2 && x.idLine == Line && x.numTypeRoute == numTypeRoute).ToList();
                if (routesVer.Count < 3) return RedirectToAction("ViewDeviation1", "Route", new { numTypeRoute = numTypeRoute });
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
                        var deviation = context.DeviationRote.Where(x => x.idLine == Line && x.numberRoute == i && x.idLine == Line && x.numTypeRoute == numTypeRoute).ToList();
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
                                    nuevosBloqueos.Add(new BloqueosMostrar(item.idDeviation, "Desvío " + place1.description + " y " + place2.description, listBloqueos, (double)route1.latitud, (double)route2.logintud));
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
        [HttpPost]
        public ActionResult ReportRuta2Deviation(string ubicaciones, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                List<Route> routesVer = new List<Route>();
                using (dbModels context = new dbModels())
                    routesVer = context.Route.Where(x => x.numberRoute == 2 && x.idLine == Line && x.numTypeRoute == numTypeRoute).ToList();
                if(routesVer.Count<2) return RedirectToAction("ViewDeviation2", "Route", new { numTypeRoute = numTypeRoute });
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
                    int idInicio = 0; int idFinal = 0; bool verificacion = false; int count = 0;
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
                        if (RouteOrder[0] == null || RouteOrder2[count] == null)
                        {
                            return RedirectToAction("ViewDeviation2", "Route", new { numTypeRoute  = numTypeRoute });
                        }
                        if (RouteOrder[0].numPoint > RouteOrder2[count].numPoint)
                        {
                            idInicio = int.Parse(RouteOrder2[count].idRoute.ToString());
                            idFinal = int.Parse(RouteOrder[0].idRoute.ToString());
                        }
                        else
                        {
                            idFinal = int.Parse(RouteOrder2[count].idRoute.ToString());
                            idInicio = int.Parse(RouteOrder[0].idRoute.ToString());
                        }
                        if (idInicio != idFinal) verificacion = true;
                        count++;
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
                    return RedirectToAction("ViewDeviation2", "Route", new { numTypeRoute  = numTypeRoute });
                }
                else return RedirectToAction("ViewDeviation2", "Route", new { numTypeRoute = numTypeRoute });
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }
        public ActionResult DeleteAllRoutes1()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();

                List<Route> route = new List<Route>();
                using (dbModels context = new dbModels()) route = context.Route.Where(x => x.idLine == Line && x.numberRoute == 1).ToList();
                if (route.Count > 0)
                {
                    using (dbModels context = new dbModels())
                    {
                        foreach (var item in route)
                        {
                            context.Route.Remove(item);
                        }
                        context.SaveChanges();
                    }
                }

                return RedirectToAction("Index", "Route");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult DeleteAllRoutes2()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();

                List<Route> route = new List<Route>();
                using (dbModels context = new dbModels()) route = context.Route.Where(x => x.idLine == Line && x.numberRoute == 2).ToList();
                if (route.Count > 0)
                {
                    using (dbModels context = new dbModels())
                    {
                        foreach (var item in route)
                        {
                            context.Route.Remove(item);
                        }
                        context.SaveChanges();
                    }
                }

                return RedirectToAction("Index", "Route");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult Parada1(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
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
                        NumeroRuta = i
                    };

                    List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                    List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();

                    using (dbModels context = new dbModels())
                    {
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status == 2 && x.numberRoute == numTypeRoute).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();

                        if (routes.Count > 0)
                        {
                            count = 0;
                            foreach (var item in routes)
                            {
                                count++;
                                Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                                if (place != null)
                                {
                                    nuevasRutas.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description + "-" + count, item.status, (int)item.numPoint));
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
                        }

                        if (checkpoints.Count > 0)
                        {

                            foreach (var item in checkpoints)
                            {
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
                        }
                    }
                    muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                    muestraRutasMapas.ListParadasMostrar = nuevasParadas;
                    ViewBag.numTypeRoute = numTypeRoute;
                    mapa.Add(muestraRutasMapas);
                }
                Session["numTypeRoute"] = numTypeRoute;
                return View(mapa);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public ActionResult Parada1(string ubicaciones)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                if (Session["numTypeRoute"] == null)
                {

                }
                int numTypeRoute = int.Parse(Session["numTypeRoute"].ToString());
                Checkpoint check = new Checkpoint();
                Route route = new Route();
                using (dbModels context = new dbModels()) check = context.Checkpoint.Where(x => x.status == 2 && x.idLine == Line && x.numberRoute == numTypeRoute).FirstOrDefault();
                using (dbModels context = new dbModels()) route = context.Route.Where(x => x.status == 2 && x.idLine == Line && x.numberRoute == 1 && x.numTypeRoute == numTypeRoute).FirstOrDefault();
                if (check != null && route != null)
                    using (dbModels context = new dbModels())
                    {
                        // Dividir la cadena en nombre, latitud y longitud
                        string[] seccionesNombreLatitudLongitud = ubicaciones.Split('/');
                        if (seccionesNombreLatitudLongitud.Length >= 3) // Asegurarse de que hay suficientes datos
                        {
                            check.description = "Parada Principal 1";
                            check.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            check.longitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            route.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            route.logintud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            
                            check.minArrival = "00:00";
                            check.minArrivalMarket = "00:00";
                            context.Entry(route).State = System.Data.Entity.EntityState.Modified;
                            context.Entry(check).State = System.Data.Entity.EntityState.Modified;
                            context.SaveChanges(); // Guardar todos los cambios
                        }

                    }
                else
                {
                    using (dbModels context = new dbModels())
                    {
                        check = new Checkpoint();
                        route = new Route();
                        string[] seccionesNombreLatitudLongitud = ubicaciones.Split('/');
                        if (seccionesNombreLatitudLongitud.Length >= 3)
                        {
                            string description = seccionesNombreLatitudLongitud[0].Trim();
                            Place place = context.Place.FirstOrDefault(x => x.description == description);
                            if (place == null)
                            {
                                place = new Place
                                {
                                    description = description
                                };
                                context.Place.Add(place);
                                context.SaveChanges();
                            }
                            check.description = "Parada Principal 1";
                            check.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            check.longitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            check.status = 2; check.numberRoute = numTypeRoute;
                            check.idLine = Line; check.minArrival = "00:00"; check.minArrivalMarket = "00:00";

                            route.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            route.logintud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            route.idPlace = place.idPlace; route.idLine = Line; route.numberRoute = 1; route.status = 2; route.numPoint = 1;
                            route.numTypeRoute = numTypeRoute;
                            context.Route.Add(route);
                            context.Checkpoint.Add(check);
                            context.SaveChanges();
                            Session["numTypeRoute"] = null;
                        }
                    }
                }
                return RedirectToAction("HomeLine", "Line");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult Parada2(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
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
                        NumeroRuta = i
                    };

                    List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                    List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();

                    using (dbModels context = new dbModels())
                    {
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status == 3 && x.numberRoute == numTypeRoute).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();

                        if (routes.Count > 0)
                        {
                            count = 0;
                            foreach (var item in routes)
                            {
                                count++;
                                Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                                if (place != null)
                                {
                                    nuevasRutas.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description + "-" + count, item.status, (int)item.numPoint));
                                }
                            }

                            if (nuevasRutas.Any())
                            {
                                RutasMostrar primeraRuta = nuevasRutas.FirstOrDefault(r => r.status == 3);
                                if (primeraRuta != null)
                                {
                                    nuevasRutas = nuevasRutas
                                        .OrderBy(route => route.numPoint)
                                        .ToList();
                                }
                            }
                        }

                        if (checkpoints.Count > 0)
                        {

                            foreach (var item in checkpoints)
                            {
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
                        }
                    }
                    List<string> horas = new List<string>();
                    List<string> minutos = new List<string> { "00", "10", "20", "30", "40", "50" };
                    for (int hour = 0; hour < 24; hour++)
                    {
                        horas.Add(hour.ToString("D2"));
                    }
                    Checkpoint check = new Checkpoint();
                    using (dbModels context = new dbModels()) check = context.Checkpoint.Where(x => x.status == 3 && x.idLine == Line && x.numberRoute == numTypeRoute).FirstOrDefault();
                    if (check != null) 
                    {
                        string[] HourMinArrival = check.minArrival.Trim().Split(':');
                        string[] HourMinArrivalMarket = check.minArrivalMarket.Trim().Split(':');
                        ViewBag.Hours = new SelectList(horas, HourMinArrival[0]);
                        ViewBag.Minutes = new SelectList(minutos, HourMinArrival[1]);
                        ViewBag.Hours2 = new SelectList(horas, HourMinArrivalMarket[0]);
                        ViewBag.Minutes2 = new SelectList(minutos, HourMinArrivalMarket[1]);
                    }
                    else
                    {
                        ViewBag.Hours = new SelectList(horas, "00");
                        ViewBag.Minutes = new SelectList(minutos, "00");
                        ViewBag.Hours2 = new SelectList(horas, "00");
                        ViewBag.Minutes2 = new SelectList(minutos, "00");
                    }
                    muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                    muestraRutasMapas.ListParadasMostrar = nuevasParadas;
                    mapa.Add(muestraRutasMapas);
                }
                Session["numTypeRoute"] = numTypeRoute;
                ViewBag.numTypeRoute = numTypeRoute;
                return View(mapa);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        [HttpPost]
        public ActionResult Parada2(string ubicaciones, string hora, string minuto, string horaMercado, string minutoMercado)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                if (Session["numTypeRoute"] == null)
                {

                }
                int numTypeRoute = int.Parse(Session["numTypeRoute"].ToString());
                Checkpoint check = new Checkpoint();
                Route route = new Route();
                using (dbModels context = new dbModels()) check = context.Checkpoint.Where(x => x.status == 3 && x.idLine == Line && x.numberRoute == numTypeRoute).FirstOrDefault();
                using (dbModels context = new dbModels()) route = context.Route.Where(x => x.status == 2 && x.idLine == Line && x.numberRoute == 2 && x.numTypeRoute == numTypeRoute).FirstOrDefault();
                if (check != null && route != null)
                    using (dbModels context = new dbModels())
                    {
                        string[] seccionesNombreLatitudLongitud = ubicaciones.Split('/');
                        if (seccionesNombreLatitudLongitud.Length >= 3) 
                        {
                            check.description = "Parada Principal Ruta 2";
                            check.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            check.longitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            route.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            route.logintud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        }
                        check.minArrival = hora + ":" + minuto;
                        check.minArrivalMarket = horaMercado + ":" + minutoMercado;
                        context.Entry(route).State = System.Data.Entity.EntityState.Modified;
                        context.Entry(check).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges(); 
                    }
                else
                {
                    using (dbModels context = new dbModels())
                    {
                        check = new Checkpoint();
                        route = new Route();
                        string[] seccionesNombreLatitudLongitud = ubicaciones.Split('/');
                        if (seccionesNombreLatitudLongitud.Length >= 3) 
                        {
                            string description = seccionesNombreLatitudLongitud[0].Trim();
                            Place place = context.Place.FirstOrDefault(x => x.description == description);
                            if (place == null)
                            {
                                place = new Place
                                {
                                    description = description
                                };
                                context.Place.Add(place);
                                context.SaveChanges(); 
                            }
                            check.description = "Parada Principal Ruta 2";
                            check.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            check.longitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            check.status = 3; check.numberRoute = numTypeRoute;
                            check.idLine = Line; check.minArrival = hora + ":" + minuto; check.minArrivalMarket = horaMercado + ":" + minutoMercado;

                            route.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            route.logintud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                            route.idPlace = place.idPlace; route.idLine = Line; route.numberRoute = 2; route.status = 2; route.numPoint = 1;
                            route.numTypeRoute = numTypeRoute;
                            context.Route.Add(route);
                            context.Checkpoint.Add(check);
                            context.SaveChanges();
                            Session["numTypeRoute"] = null;
                        }
                    }
                }
                return RedirectToAction("HomeLine", "Line");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult EditParada1Check(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
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
                        NumeroRuta = i
                    };

                    List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                    List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();

                    using (dbModels context = new dbModels())
                    {
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.numberRoute == numTypeRoute && x.status == 2).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();

                        if (routes.Count > 0)
                        {
                            count = 0;
                            foreach (var item in routes)
                            {
                                count++;
                                Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                                if (place != null)
                                {
                                    nuevasRutas.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description + "-" + count, item.status, (int)item.numPoint));
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
                        }

                        if (checkpoints.Count > 0)
                        {

                            foreach (var item in checkpoints)
                            {
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
                        }
                    }
                    muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                    muestraRutasMapas.ListParadasMostrar = nuevasParadas;
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
        [HttpPost]
        public ActionResult EditParada1Check(string ubicaciones, string hora, string minuto, string horaMercado, string minutoMercado, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                Checkpoint check = new Checkpoint();
                Route route = new Route();
                using (dbModels context = new dbModels()) check = context.Checkpoint.Where(x => x.idLine == Line && x.status == 2 && x.numberRoute == numTypeRoute).FirstOrDefault();
                using (dbModels context = new dbModels()) route = context.Route.Where(x => x.idLine == Line && x.status == 2 && x.numberRoute == 1 && x.numTypeRoute == numTypeRoute).FirstOrDefault();
                using (dbModels context = new dbModels())
                {
                    // Dividir la cadena en nombre, latitud y longitud
                    string[] seccionesNombreLatitudLongitud = ubicaciones.Split('/');
                    if (seccionesNombreLatitudLongitud.Length >= 3) // Asegurarse de que hay suficientes datos
                    {
                        check.description = "Parada Principal Ruta 1";
                        check.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        check.longitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        route.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        route.logintud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);

                        check.minArrival = "00:00";
                        check.minArrivalMarket = "00:00";
                        context.Entry(route).State = System.Data.Entity.EntityState.Modified;
                        context.Entry(check).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges(); // Guardar todos los cambios
                    }

                }
                return RedirectToAction("Index", "Route", new { numTypeRoute  = numTypeRoute });
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult EditParada2Check(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
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
                        NumeroRuta = i
                    };

                    List<RutasMostrar> nuevasRutas = new List<RutasMostrar>();
                    List<ParadasMostrar> nuevasParadas = new List<ParadasMostrar>();

                    using (dbModels context = new dbModels())
                    {
                        var checkpoints = context.Checkpoint.Where(x => x.idLine == Line && x.status == 3 && x.numberRoute == numTypeRoute).ToList();
                        var routes = context.Route
                            .Where(x => x.idLine == Line && x.numberRoute == i && x.numTypeRoute == numTypeRoute)
                            .ToList();

                        if (routes.Count > 0)
                        {
                            count = 0;
                            foreach (var item in routes)
                            {
                                count++;
                                Place place = context.Place.FirstOrDefault(x => x.idPlace == item.idPlace);
                                if (place != null)
                                {
                                    nuevasRutas.Add(new RutasMostrar(item.idRoute, item.latitud, item.logintud, place.description + "-" + count, item.status, (int)item.numPoint));
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
                        }

                        if (checkpoints.Count > 0)
                        {

                            foreach (var item in checkpoints)
                            {
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
                        }
                    }
                    List<string> horas = new List<string>();
                    List<string> minutos = new List<string> { "00", "10", "20", "30", "40", "50" };
                    for (int hour = 0; hour < 24; hour++)
                    {
                        horas.Add(hour.ToString("D2"));
                    }
                    Checkpoint check = new Checkpoint();
                    using (dbModels context = new dbModels()) check = context.Checkpoint.Where(x => x.idLine == Line && x.status == 3 && x.numberRoute == numTypeRoute).FirstOrDefault();
                    // Separar la descripción en horas y minutos
                    string[] HourMinArrival = check.minArrival.Trim().Split(':');
                    string[] HourMinArrivalMarket = check.minArrivalMarket.Trim().Split(':');
                    // Asignar los SelectList al ViewBag
                    ViewBag.Hours = new SelectList(horas, HourMinArrival[0]);
                    ViewBag.Minutes = new SelectList(minutos, HourMinArrival[1]);
                    ViewBag.Hours2 = new SelectList(horas, HourMinArrivalMarket[0]);
                    ViewBag.Minutes2 = new SelectList(minutos, HourMinArrivalMarket[1]);
                    muestraRutasMapas.ListRutasMostrar = nuevasRutas;
                    muestraRutasMapas.ListParadasMostrar = nuevasParadas;
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
        [HttpPost]
        public ActionResult EditParada2Check(string ubicaciones, string hora, string minuto, string horaMercado, string minutoMercado, int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                Checkpoint check = new Checkpoint();
                Route route = new Route();
                using (dbModels context = new dbModels()) check = context.Checkpoint.Where(x => x.idLine == Line && x.status == 3 && x.numberRoute == numTypeRoute).FirstOrDefault();
                using (dbModels context = new dbModels()) route = context.Route.Where(x => x.idLine == Line && x.status == 2 && x.numberRoute == 2 && x.numTypeRoute == numTypeRoute).FirstOrDefault();
                using (dbModels context = new dbModels())
                {
                    // Dividir la cadena en nombre, latitud y longitud
                    string[] seccionesNombreLatitudLongitud = ubicaciones.Split('/');
                    if (seccionesNombreLatitudLongitud.Length >= 3) // Asegurarse de que hay suficientes datos
                    {
                        check.description = "Parada Principal Ruta 2";
                        check.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        check.longitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        route.latitud = Convert.ToDecimal(seccionesNombreLatitudLongitud[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                        route.logintud = Convert.ToDecimal(seccionesNombreLatitudLongitud[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    check.minArrival = hora + ":" + minuto;
                    check.minArrivalMarket = horaMercado + ":" + minutoMercado;
                    context.Entry(route).State = System.Data.Entity.EntityState.Modified;
                    context.Entry(check).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges(); // Guardar todos los cambios
                }
                return RedirectToAction("Index", "Route", new { numTypeRoute  = numTypeRoute });
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult CreateNewRuta()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                int max = 0;
                using (dbModels context = new dbModels())
                {
                    var checks = context.Checkpoint.Where(x => x.idLine == Line && x.status > 0);
                    max = int.Parse(checks.Max(x => x.numberRoute).ToString());
                    max += 1;
                }
                if (max > 0)
                {
                    return RedirectToAction("Parada1", new { numTypeRoute = max });
                }
                return RedirectToAction("Error", "Home");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult DeleteRuta(int numTypeRoute)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Line = logi.GetLineaFromCookie(Request).ToString();
                using (dbModels context = new dbModels())
                {
                    List<Checkpoint> checks = context.Checkpoint.Where(x => x.idLine == Line && x.numberRoute == numTypeRoute && x.status > 0).ToList();
                    List<DeviationRote> deviations = context.DeviationRote.Where(x => x.idLine == Line && x.numTypeRoute == numTypeRoute).ToList();
                    List<Route> routes = context.Route.Where(x => x.idLine == Line && x.numTypeRoute == numTypeRoute).ToList();
                    foreach (var item in checks)
                    {
                        Checkpoint check = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                        check.idEmployee = null;
                        check.status = 0;
                        context.Entry(check).State = System.Data.Entity.EntityState.Modified;
                    }
                    foreach (var item in deviations)
                    {
                        DeviationRote check = context.DeviationRote.Where(x => x.idDeviation == item.idDeviation).FirstOrDefault();
                        context.DeviationRote.Remove(check);
                    }
                    foreach (var item in routes)
                    {
                        Route check = context.Route.Where(x => x.idRoute == item.idRoute).FirstOrDefault();
                        context.Route.Remove(check);
                    }
                    context.SaveChanges();
                }
                return RedirectToAction("ListRoutes", "Route");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
