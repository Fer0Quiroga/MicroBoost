using ProyectoControlLineaBus.Clases;
using ProyectoControlLineaBus.Models;
using QRCoder;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Shapes;

namespace ProyectoControlLineaBus.Controllers
{
    public class EmployeeController : Controller
    {
        // GET: Employee
        public ActionResult Index()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string linea = logi.GetLineaFromCookie(Request).ToString();
                List<MuestraEmployee> muestraEmployees = new List<MuestraEmployee>();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                List<Employee> employee = new List<Employee>();
                using (dbModels context = new dbModels())
                    employee = context.Employee.ToList().Where(x => x.status == 1 && x.idLine == linea).ToList();
                foreach (var item in employee)
                    using (dbModels context = new dbModels())
                    {
                        Person person = context.Person.Where(x => x.nit == item.idEmployee).FirstOrDefault();
                        if (person.idRole == 1)
                        {
                            double start = 5;
                            double count = 1;
                            var stars = context.Star.Where(x => x.idEmployee == item.idEmployee).ToList();
                            var star = stars.Where(x => DateTime.Parse(x.dateRegister.ToString()).Month == boliviaTime.Year && DateTime.Parse(x.dateRegister.ToString()).Month == boliviaTime.Year).ToList();
                            if (stars.Count > 0)
                                foreach (var item2 in stars)
                                {
                                    count++;
                                    start = start + double.Parse(item2.numStar.ToString());
                                }
                            start = start / count;
                            start = Math.Round(start, 2);
                            muestraEmployees.Add(new MuestraEmployee(person.nit, person.name + " " + person.lastname + " (★" + start + ")", (decimal)item.salary, item.plate, int.Parse(person.idRole.ToString()), " "));
                        }
                        else
                        muestraEmployees.Add(new MuestraEmployee(person.nit, person.name + " " + person.lastname, (decimal)item.salary, item.plate, int.Parse(person.idRole.ToString()), " "));
                    }
                foreach (var item in muestraEmployees)
                {
                    using (dbModels context = new dbModels())
                    {
                        Checkpoint checkpoint = context.Checkpoint.Where(x => x.idEmployee == item.idEmployee).FirstOrDefault();
                        if (checkpoint != null)
                            item.description = checkpoint.description;
                    }
                }
                return View(muestraEmployees);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }


        }
        public ActionResult VerificationChofer()
        {
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            Session["Chofer"] = null;
            Session["PersonChofer"] = null;
            return View();
        }
        [HttpPost]
        public ActionResult VerificationChofer(EmployeeChofer employeechofer)
        {
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            employeechofer.idEmployee = employeechofer.idEmployee.ToString().ToUpper().Trim();
            Employee em = new Employee();
            using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToUpper().Trim() == employeechofer.idEmployee && x.status == 1).FirstOrDefault();
            if (em != null)
            {
                ViewBag.NIT = "El empleado ya esta registrado";
                return View();
            }
            Person person = new Person();
            using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToUpper().ToString().Trim() == employeechofer.idEmployee && x.status == 1).FirstOrDefault();
            Session["Chofer"] = employeechofer.idEmployee;
            if (person == null) return RedirectToAction("CreateChofer");
            else if (person.status == 1 && person.idRole == 0)
            {
                Session["PersonChofer"] = person;
                return RedirectToAction("CreateExistPersonChofer");
            }
            ViewBag.NIT = "La persona ya esta trabajando";
            return View();
        }
        public ActionResult CreateExistPersonChofer()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            string linea = logi.GetLineaFromCookie(Request).ToString();
            if (logi.GetRoleSessionFromCookie(Request).ToString() != "3" || Session["Chofer"] == null || Session["PersonChofer"] == null) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public ActionResult CreateExistPersonChofer(EmployeeChofer employeechofer)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                if (logi.GetRoleSessionFromCookie(Request).ToString() != "3" || Session["Chofer"] == null || Session["PersonChofer"] == null) return RedirectToAction("Index", "Home");
                string numeroFormateado = "10";
                employeechofer.idEmployee = Session["Chofer"].ToString();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                using (dbModels context = new dbModels())
                {
                    Employee e = context.Employee.Where(x => x.plate.ToUpper().Trim() == employeechofer.plate.ToUpper().Trim() && x.status == 1).FirstOrDefault();
                    if (e != null)
                    {
                        TempData["Plate"] = "La placa ya está siendo utilizada";
                        return View();
                    }
                }
                Employee em = new Employee();
                employeechofer.idEmployee = employeechofer.idEmployee.ToString().ToUpper().Trim();
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToUpper().ToString().Trim() == employeechofer.idEmployee && x.status == 1).FirstOrDefault();
                if (em != null)
                {
                    TempData["NIT"] = "El empleado ya esta registrado";
                    return View();
                }
                User user = new User();
                using (dbModels context = new dbModels()) user = context.User.Where(x => x.email.ToLower().Trim() == employeechofer.email.ToLower().Trim()).FirstOrDefault();
                if (user != null)
                {
                    TempData["Mail"] = "El mail ya esta siendo utilizado";
                    return View();
                }
                Person person = new Person();
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString() == employeechofer.idEmployee.ToString()).FirstOrDefault();
                if (person == null)
                {
                    using (dbModels context = new dbModels())
                    {
                        Person personCreate = new Person();
                        personCreate.idRole = 1; personCreate.status = 1;
                        personCreate.dateRegister = boliviaTime;
                        context.Person.Add(personCreate);
                        context.SaveChanges();
                    }
                }
                else if (person.status == 0 || person.idRole == 0)
                    using (dbModels context = new dbModels())
                    {
                        person.idRole = 1; person.status = 1;
                        context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                else
                {
                    TempData["Person"] = "La persona ya está trabajando";
                    return View();
                }
                Employee employee = new Employee();
                employee.status = 1; employee.dateRegister = boliviaTime; employee.idLine = Linea;
                employee.plate = employeechofer.plate.ToUpper().Trim(); employee.idEmployee = employeechofer.idEmployee.Trim();
                employee.salary = employeechofer.salary; employee.enabled = 0;
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToString() == employeechofer.idEmployee.ToString()).FirstOrDefault();
                user = new User();
                if (em == null)
                    using (dbModels context = new dbModels())
                    {
                        context.Employee.Add(employee);
                        context.SaveChanges();
                    }
                else if (em.status == 0)
                    using (dbModels context = new dbModels())
                    {
                        context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                EmployeeControl employeeControl = new EmployeeControl();
                string username = employeeControl.GenerarUsername(person.name.Trim(), person.lastname.Trim());
                string pass = employeeControl.GenerarPassword();
                using (dbModels context = new dbModels())
                {
                    Encriptado encriptado = new Encriptado();
                    user = new User();
                    user.idEmployee = employee.idEmployee;
                    user.username = username;
                    user.password = encriptado.Encriptar(pass);
                    user.firstEntry = 0;
                    user.email = employeechofer.email.ToLower().Trim();
                    context.User.Add(user);
                    context.SaveChanges();
                    employeeControl.EnviarCorreo(employeechofer.email.ToLower().Trim(), username, pass);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult CreateChofer()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            string linea = logi.GetLineaFromCookie(Request).ToString();
            if (logi.GetRoleSessionFromCookie(Request).ToString() != "3" || Session["Chofer"] == null) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public ActionResult CreateChofer(EmployeeChofer employeechofer)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                if (logi.GetRoleSessionFromCookie(Request).ToString() != "3" || Session["Chofer"] == null) return RedirectToAction("Index", "Home");
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                string numeroFormateado = "10";
                employeechofer.idEmployee = Session["Chofer"].ToString();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                User user = new User();
                using (dbModels context = new dbModels()) user = context.User.Where(x => x.email.ToLower().Trim() == employeechofer.email.ToLower().Trim()).FirstOrDefault();
                if (user != null)
                {
                    TempData["Mail"] = "El mail ya esta siendo utilizado";
                    return View();
                }
                using (dbModels context = new dbModels()) 
                {
                    Employee e = context.Employee.Where(x => x.plate.ToUpper().Trim() == employeechofer.plate.ToUpper().Trim() && x.status == 1).FirstOrDefault();
                    if (e != null)
                    {
                        TempData["Plate"] = "La placa ya está siendo utilizada";
                        return View();
                    }
                }
                Employee em = new Employee();
                employeechofer.idEmployee = employeechofer.idEmployee.ToString().ToUpper().Trim();
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToUpper().ToString().Trim() == employeechofer.idEmployee.ToUpper().Trim() && x.status == 1).FirstOrDefault();
                if (em != null)
                {
                    TempData["NIT"] = "El empleado ya esta registrado";
                    return View();
                }
                Person person = new Person();
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString() == employeechofer.idEmployee.ToString()).FirstOrDefault();
                if (person == null)
                {
                    using (dbModels context = new dbModels())
                    {
                        person = new Person();
                        person.nit = employeechofer.idEmployee.Trim();
                        person.name = employeechofer.name.Trim();
                        person.lastname = employeechofer.lastname.Trim();
                        person.idRole = 1; person.status = 1;
                        person.dateRegister = boliviaTime;
                        context.Person.Add(person);
                        context.SaveChanges();
                    }
                }
                else if(person.status == 0 || person.idRole == 0)
                    using (dbModels context = new dbModels())
                    {
                        person.name = employeechofer.name.Trim();
                        person.lastname = employeechofer.lastname.Trim();
                        person.idRole = 1; person.status = 1;
                        context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                else
                {
                    TempData["Person"] = "La persona ya está trabajando";
                    return View();
                }
                Employee employee = new Employee();
                employee.status = 1; employee.dateRegister = boliviaTime;  employee.idLine = Linea;
                employee.plate = employeechofer.plate.ToUpper().Trim(); employee.idEmployee = employeechofer.idEmployee.Trim();
                employee.salary = employeechofer.salary; employee.enabled = 0;
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToString() == employeechofer.idEmployee.ToString()).FirstOrDefault();
                if (em == null)
                    using (dbModels context = new dbModels())
                    {
                        context.Employee.Add(employee);
                        context.SaveChanges();
                    }
                else if (em.status == 0)
                    using (dbModels context = new dbModels())
                    {
                        context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                EmployeeControl employeeControl = new EmployeeControl();
                string username = employeeControl.GenerarUsername(person.name.Trim(), person.lastname.Trim());
                string pass = employeeControl.GenerarPassword();
                using (dbModels context = new dbModels())
                {
                    Encriptado encriptado = new Encriptado();
                    user = new User();
                    user.idEmployee = employee.idEmployee;
                    user.username = username;
                    user.password = encriptado.Encriptar(pass);
                    user.firstEntry = 0;
                    user.email = employeechofer.email.ToLower().Trim();
                    context.User.Add(user);
                    context.SaveChanges();
                    employeeControl.EnviarCorreo(employeechofer.email.ToLower().Trim(), username, pass);
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult VerificationControl()
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            Session["Control"] = null;
            Session["PersonControl"] = null;
            return View();
        }
        [HttpPost]
        public ActionResult VerificationControl(EmployeeChofer employeechofer)
        {
            Login logi = new Login();
            var authResult = AutenticarPasosRol(3);
            if (authResult != null) return authResult;
            Employee em = new Employee();
            employeechofer.idEmployee = employeechofer.idEmployee.ToString().ToUpper().Trim();
            using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToUpper().ToString().Trim() == employeechofer.idEmployee.ToUpper().ToString().Trim() && x.status == 1).FirstOrDefault();
            if (em != null)
            {
                ViewBag.NIT = "El empleado ya esta registrado";
                return View();
            }
            Person person = new Person();
            using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToUpper().ToString().Trim() == employeechofer.idEmployee.ToUpper().ToString().Trim() && x.status == 1).FirstOrDefault();
            Session["Control"] = employeechofer.idEmployee;
            if (person == null) return RedirectToAction("CreateControlador");
            else if (person.status == 1 && person.idRole == 0)
            {
                Session["PersonControl"] = person;
                return RedirectToAction("CreateExistPersonControl");
            }
            ViewBag.NIT = "La persona ya esta trabajando";
            return View();
        }
        public ActionResult CreateExistPersonControl()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                if (Session["PersonControl"] == null || Session["Control"]==null) return RedirectToAction("Index", "Home");
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                Checkpoint valid = new Checkpoint();
                using (dbModels context = new dbModels()) valid = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status != 0).FirstOrDefault();
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status != 0).ToList();
                if (valid == null) return RedirectToAction("RegisterCheck", "Route");
                ViewBag.idCheckpoint = new SelectList(checkpoint, "idCheckpoint", "description");
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult CreateExistPersonControl(EmployeeControl employeeControl)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult; if (Session["PersonControl"] == null || Session["Control"] == null) return RedirectToAction("Index", "Home");
                string numeroFormateado = employeeControl.salary.ToString("0.00");
                employeeControl.idEmployee = Session["Control"].ToString();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                if (decimal.Parse(numeroFormateado) <= 0 || decimal.Parse(numeroFormateado) >= 10000)
                {
                    TempData["Salary"] = "El salario debe ser mayor a 0 y menor a 10000";
                    return View();
                }
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                using (dbModels context = new dbModels())
                    checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status != 0).ToList();
                if (checkpoint == null) return RedirectToAction("RegisterCheck", "Route");
                ViewBag.idCheckpoint = new SelectList(checkpoint, "idCheckpoint", "description");
                User user = new User();
                using (dbModels context = new dbModels()) user = context.User.Where(x => x.email.ToLower().Trim() == employeeControl.email.ToLower().Trim()).FirstOrDefault();
                if (user != null)
                {
                    TempData["Mail"] = "El mail ya esta siendo utilizado";
                    return View();
                }
                employeeControl.idEmployee = employeeControl.idEmployee.ToString().ToUpper().Trim();
                Employee em = new Employee();
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToString().ToUpper().Trim() == employeeControl.idEmployee.ToString().ToUpper().Trim() && x.status == 1).FirstOrDefault();
                if (em != null)
                {
                    TempData["NIT"] = "El empleado ya esta registrado";
                    return View();
                }
                Person person = new Person();
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString().ToUpper().Trim() == employeeControl.idEmployee.ToString()).FirstOrDefault();
                if (person == null)
                {
                    using (dbModels context = new dbModels())
                    {
                        Person personCreate = new Person();
                        personCreate.idRole = 2; personCreate.status = 1;
                        DateTime hoy = boliviaTime;
                        personCreate.dateRegister = hoy;
                        context.Person.Add(personCreate);
                        context.SaveChanges();
                    }
                }
                else if (person.status == 0 || person.idRole == 0)
                    using (dbModels context = new dbModels())
                    {
                        person.idRole = 2; person.status = 1;
                        context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                else
                {
                    TempData["Person"] = "La persona ya está trabajando";
                    return View();
                }
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString() == employeeControl.idEmployee.ToString()).FirstOrDefault();
                User validationMail = new User();
                using (dbModels context = new dbModels()) validationMail = context.User.Where(x => x.email.ToLower().Trim() == employeeControl.email.ToLower().Trim()).FirstOrDefault();
                if (validationMail != null)
                {
                    TempData["Mail"] = "El mail ya esta siendo utilizado";
                    return View();
                }
                Employee employee = new Employee();
                user = new User();
                employee.idEmployee = employeeControl.idEmployee.Trim(); employee.status = 1;
                employee.dateRegister = boliviaTime; employee.idLine = Linea;
                employee.salary = employeeControl.salary;
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToString() == employeeControl.idEmployee.ToString()).FirstOrDefault();
                if (em == null)
                    using (dbModels context = new dbModels())
                    {
                        context.Employee.Add(employee);
                        context.SaveChanges();
                    }
                else if (em.status == 0)
                    using (dbModels context = new dbModels())
                    {
                        context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                else
                {
                    TempData["NIT"] = "El empleado ya esta registrado";
                    return View();
                }
                string username = employeeControl.GenerarUsername(person.name.Trim(), person.lastname.Trim());
                string pass = employeeControl.GenerarPassword();
                using (dbModels context = new dbModels())
                {
                    Encriptado encriptado = new Encriptado();
                    user.username = username.Trim();
                    user.password = encriptado.Encriptar(pass);
                    user.email = employeeControl.email.ToLower().Trim();
                    user.idEmployee = employeeControl.idEmployee.Trim();
                    user.firstEntry = 0;
                    context.User.Add(user);
                    context.SaveChanges();
                    employeeControl.EnviarCorreo(employeeControl.email.ToLower().Trim(), username, pass);
                }
                using (dbModels context = new dbModels())
                {
                    Checkpoint checkpoint1 = context.Checkpoint.Where(x => x.idCheckpoint == employeeControl.idCheckpoint).FirstOrDefault();
                    checkpoint1.idEmployee = employeeControl.idEmployee.Trim();
                    context.Entry(checkpoint1).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult CreateControlador()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult; if (Session["Control"] == null) return RedirectToAction("Index", "Home");
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                Checkpoint valid = new Checkpoint();
                using (dbModels context = new dbModels())valid = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status != 0).FirstOrDefault();
                using (dbModels context = new dbModels()) checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status != 0).ToList();
                if (valid == null) return RedirectToAction("RegisterCheck", "Route");
                ViewBag.idCheckpoint = new SelectList(checkpoint, "idCheckpoint", "description");
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult CreateControlador(EmployeeControl employeeControl)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                if (Session["Control"] == null) return RedirectToAction("Index", "Home");
                string numeroFormateado = employeeControl.salary.ToString("0.00");
                employeeControl.idEmployee = Session["Control"].ToString();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                if (decimal.Parse(numeroFormateado) <= 0 || decimal.Parse(numeroFormateado) >= 10000)
                {
                    TempData["Salary"] = "El salario debe ser mayor a 0 y menor a 10000";
                    return View();
                }
                string Linea = logi.GetLineaFromCookie(Request).ToString();
                List<Checkpoint> checkpoint = new List<Checkpoint>();
                using (dbModels context = new dbModels())
                    checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status != 0).ToList();
                if (checkpoint == null) return RedirectToAction("RegisterCheck", "Route");
                ViewBag.idCheckpoint = new SelectList(checkpoint, "idCheckpoint", "description");
                User user = new User();
                using (dbModels context = new dbModels()) user = context.User.Where(x => x.email.ToLower().Trim() == employeeControl.email.ToLower().Trim()).FirstOrDefault();
                if (user != null)
                {
                    TempData["Mail"] = "El mail ya esta siendo utilizado";
                    return View();
                }
                employeeControl.idEmployee = employeeControl.idEmployee.ToString().ToUpper().Trim();
                Employee em = new Employee();
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToString().ToUpper().Trim() == employeeControl.idEmployee.ToString().ToUpper().Trim() && x.status == 1).FirstOrDefault();
                if (em != null)
                {
                    TempData["NIT"] = "El empleado ya esta registrado";
                    return View();
                }
                Person person = new Person();
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString().ToUpper().Trim() == employeeControl.idEmployee.ToString()).FirstOrDefault();
                if (person == null)
                {
                    using (dbModels context = new dbModels())
                    {
                        Person personCreate = new Person();
                        personCreate.nit = employeeControl.idEmployee.Trim();
                        personCreate.name = employeeControl.name.Trim();
                        personCreate.lastname = employeeControl.lastname.Trim();
                        personCreate.idRole = 2; personCreate.status = 1;
                        DateTime hoy = boliviaTime;
                        personCreate.dateRegister = hoy;
                        context.Person.Add(personCreate);
                        context.SaveChanges();
                    }
                }
                else if (person.status == 0 || person.idRole == 0)
                    using (dbModels context = new dbModels())
                    {
                        person.name = employeeControl.name.Trim();
                        person.lastname = employeeControl.lastname.Trim();
                        person.idRole = 2; person.status = 1;
                        context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                else
                {
                    TempData["Person"] = "La persona ya está trabajando";
                    return View();
                }
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString() == employeeControl.idEmployee.ToString()).FirstOrDefault();
                User validationMail = new User();
                using (dbModels context = new dbModels()) validationMail = context.User.Where(x => x.email.ToLower().Trim() == employeeControl.email.ToLower().Trim()).FirstOrDefault();
                if (validationMail != null)
                {
                    TempData["Mail"] = "El mail ya esta siendo utilizado";
                    return View();
                }
                Employee employee = new Employee();
                user = new User();
                employee.idEmployee = employeeControl.idEmployee.Trim(); employee.status = 1;
                employee.dateRegister = boliviaTime; employee.idLine = Linea;
                employee.salary = employeeControl.salary;
                using (dbModels context = new dbModels()) em = context.Employee.Where(x => x.idEmployee.ToString() == employeeControl.idEmployee.ToString()).FirstOrDefault();
                if (em == null)
                    using (dbModels context = new dbModels())
                    {
                        context.Employee.Add(employee);
                        context.SaveChanges();
                    }
                else if (em.status == 0)
                    using (dbModels context = new dbModels())
                    {
                        context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                else
                {
                    TempData["NIT"] = "El empleado ya esta registrado";
                    return View();
                }
                string username = employeeControl.GenerarUsername(person.name.Trim(), person.lastname.Trim());
                string pass = employeeControl.GenerarPassword();
                using (dbModels context = new dbModels())
                {
                    Encriptado encriptado = new Encriptado();
                    user.username = username.Trim();
                    user.password = encriptado.Encriptar(pass);
                    user.email = employeeControl.email.ToLower().Trim();
                    user.idEmployee = employeeControl.idEmployee.Trim();
                    user.firstEntry = 0;
                    context.User.Add(user);
                    context.SaveChanges();
                    employeeControl.EnviarCorreo(employeeControl.email.ToLower().Trim(), username, pass);
                }
                using (dbModels context = new dbModels())
                {
                    Checkpoint checkpoint1 = context.Checkpoint.Where(x => x.idCheckpoint == employeeControl.idCheckpoint).FirstOrDefault();
                    checkpoint1.idEmployee = employeeControl.idEmployee.Trim();
                    context.Entry(checkpoint1).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult EditChofer(string id)
        {
            try
            {
                if (id == null) return RedirectToAction("Index", "Home");
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Session["EmployeeEditChofer"] = null; Session["PersonaEditChofer"] = null;
                EmployeeChofer employeeChofer = new EmployeeChofer();
                Employee employee = new Employee();
                User user = new User();
                using (dbModels context = new dbModels()) employee = context.Employee.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                employeeChofer.idEmployee = id;
                employeeChofer.plate = employee.plate;
                Person person = new Person();
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString() == id).FirstOrDefault();
                employeeChofer.name = person.name;
                employeeChofer.lastname = person.lastname;
                using (dbModels context = new dbModels()) user = context.User.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                employeeChofer.email = user.email;
                if (employee.salary != null) employeeChofer.salary = decimal.Parse(employee.salary.ToString());
                Session["PersonaEditChofer"] = person; Session["EmployeeEditChofer"] = employee; Session["UserEditChofer"] = user;
                return View(employeeChofer);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult EditChofer(EmployeeChofer employeeChofer)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Employee employee = (Employee)Session["EmployeeEditChofer"];
                Person person = (Person)Session["PersonaEditChofer"];
                User user = (User)Session["UserEditChofer"];
                string numeroFormateado = "10";
                User validationMail = new User();
                using (dbModels context = new dbModels()) validationMail = context.User.Where(x => x.email.ToLower().Trim() == employeeChofer.email.ToLower().Trim()).FirstOrDefault();
                if (validationMail != null && user.email.ToLower().Trim() != employeeChofer.email.ToLower().Trim())
                {
                    TempData["Mail"] = "El mail ya esta siendo utilizado";
                    return View();
                }
                using (dbModels context = new dbModels())
                {
                    Employee e = context.Employee.Where(x => x.plate.ToUpper().Trim() == employeeChofer.plate.ToUpper().Trim()).FirstOrDefault();
                    if (e != null && employee.plate.ToUpper().Trim() != employeeChofer.plate.ToUpper().Trim())
                    {
                        TempData["Plate"] = "La placa ya esta siendo utilizada";
                        return View();
                    }
                }
                using (dbModels context = new dbModels())
                {
                    person.name = employeeChofer.name.Trim();
                    person.lastname = employeeChofer.lastname.Trim();
                    context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    employee.plate = employeeChofer.plate.ToUpper().Trim();
                    employee.salary = employeeChofer.salary;
                    context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    user.email = employeeChofer.email.ToLower().Trim();
                    context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                Session["EmployeeEditChofer"] = null; Session["PersonaEditChofer"] = null; Session["UserEditChofer"] = null;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult EditControlador(string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Session["EmployeeEditControl"] = null; Session["PersonaEditControl"] = null; Session["UserEditControl"] = null;
                EmployeeControl employeeControl = new EmployeeControl(); 
                User user = new User();
                Employee employee = new Employee();
                using (dbModels context = new dbModels()) employee = context.Employee.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                using (dbModels context = new dbModels()) user = context.User.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                employeeControl.idEmployee = id;
                employeeControl.email = user.email.ToLower().Trim();
                Person person = new Person();
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString() == id).FirstOrDefault();
                employeeControl.name = person.name;
                employeeControl.lastname = person.lastname;
                employeeControl.salary = (decimal)employee.salary;
                Session["PersonaEditControl"] = person; Session["UserEditControl"] = user; Session["EmployeeEditControl"] = employee;
                return View(employeeControl);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult EditControlador(EmployeeControl employeeControl)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                User validationMail = new User();
                User user = (User)Session["UserEditControl"]; Person person = (Person)Session["PersonaEditControl"]; Employee employee = (Employee)Session["EmployeeEditControl"];
                using (dbModels context = new dbModels()) validationMail = context.User.Where(x => x.email.ToLower().Trim() == employeeControl.email.ToLower().Trim()).FirstOrDefault();
                string numeroFormateado = employeeControl.salary.ToString("0.00");
                if (decimal.Parse(numeroFormateado) <= 0 || decimal.Parse(numeroFormateado) >= 10000)
                {
                    TempData["Salary"] = "El salario debe ser mayor a 0 y menor a 10000";
                    return View();
                }
                if (validationMail != null && user.email.ToLower().Trim() != employeeControl.email.ToLower().Trim())
                {
                    TempData["Mail"] = "El mail ya esta siendo utilizado";
                    return View();
                }
                using (dbModels context = new dbModels())
                {
                    person.name = employeeControl.name.Trim();
                    person.lastname = employeeControl.lastname.Trim();
                    context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    person.name = employeeControl.name;
                    person.lastname = employeeControl.lastname;
                    context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    user.email = employeeControl.email.ToLower().Trim();
                    context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    employee.salary = employeeControl.salary;
                    context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                Session["EmployeeEditControl"] = null; Session["PersonaEditControl"] = null; Session["UserEditControl"] = null;
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        // GET: Employee/Delete/5
        public ActionResult DeleteChofer(string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                Employee employee = new Employee();
                using (dbModels context = new dbModels()) employee = context.Employee.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                Person person = new Person();
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString() == id).FirstOrDefault();
                using (dbModels context = new dbModels())
                {
                    employee.status = 0;
                    employee.plate = null;
                    context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                List<Owner> owners = new List<Owner>();
                using (dbModels context = new dbModels()) owners = context.Owner.Where(x => x.idPerson.ToString() == id).ToList();
                if (owners.Count > 0)
                    using (dbModels context = new dbModels())
                    {
                        person.status = 1;
                        person.idRole = 0;
                        context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                else
                    using (dbModels context = new dbModels())
                    {
                        person.status = 0;
                        person.idRole = 0;
                        context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                using (dbModels context = new dbModels())
                {
                    User user = context.User.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                    context.User.Remove(user); context.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult DeleteControlador(string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                List<Owner> owners = new List<Owner>();
                using (dbModels context = new dbModels()) owners = context.Owner.Where(x => x.idPerson.ToString() == id).ToList();
                using (dbModels context = new dbModels())
                {
                    Employee employee = context.Employee.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                    employee.status = 0;
                    context.Entry(employee).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    Person person = context.Person.Where(x => x.nit.ToString() == id).FirstOrDefault();
                    if (owners.Count > 0) person.status = 1;
                    else person.status = 0;
                    person.idRole = 0;
                    context.Entry(person).State = System.Data.Entity.EntityState.Modified; context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    User user = context.User.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                    context.User.Remove(user); context.SaveChanges();
                }
                using (dbModels context = new dbModels())
                {
                    List<Checkpoint> checkpoint= context.Checkpoint.Where(x => x.idEmployee.ToString() == id).ToList();
                    if (checkpoint != null) 
                    {
                        foreach (var item in checkpoint)
                        {
                            Checkpoint check = context.Checkpoint.Where(x => x.idCheckpoint == item.idCheckpoint).FirstOrDefault();
                            check.idEmployee = null;
                            context.Entry(check).State = System.Data.Entity.EntityState.Modified; context.SaveChanges();
                        }
                        
                    } 
                }
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult CheckpointDelete(string id)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(3);
                if (authResult != null) return authResult;
                using (dbModels context = new dbModels())
                {
                    Checkpoint checkpoint = context.Checkpoint.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                    checkpoint.idEmployee = null;
                    context.Entry(checkpoint).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("Index");
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
                using (dbModels context = new dbModels())
                    checkpoint = context.Checkpoint.Where(x => x.idLine == Linea && x.idEmployee == null && x.status != 0).ToList();
                if (checkpoint == null) return RedirectToAction("RegisterCheck", "Route");
                ViewBag.idCheckpoint = new SelectList(checkpoint, "idCheckpoint", "description");
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
                return RedirectToAction("Index");
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
        public ActionResult DriverQR(string placa)
        {
            ViewBag.PlacaChofer = placa;
           return View();
        }

        public ActionResult GenerateQR(string content)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(("https://localhost:44328/Home/QRActions?placa=" + content), QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);

                using (Bitmap bitmap = qrCode.GetGraphic(20))
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return File(ms.ToArray(), "image/png");
                }
            }
        }

        public ActionResult PrintQRDiver(string placa)
        {
            try
            {
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
                return new ActionAsPdf("DriverQR", new {placa = placa}) { FileName = placa + "-" + boliviaTime.Day + boliviaTime.Month + boliviaTime.Year + ".pdf" };
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
