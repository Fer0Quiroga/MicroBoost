using ProyectoControlLineaBus.Clases;
using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProyectoControlLineaBus.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                using (dbModels context = new dbModels())
                {
                    List<Person> person = context.Person.Where(x => x.idRole == 3 && x.status == 1).ToList();
                    return View(person);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult VerificationGestor()
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }

        [HttpPost]
        public ActionResult VerificationGestor(EmployeeChofer employeechofer)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                employeechofer.idEmployee = employeechofer.idEmployee.ToString().ToUpper().Trim();
                Employee em = new Employee();
                Person person = new Person();
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToUpper().ToString().Trim() == employeechofer.idEmployee && x.status == 1).FirstOrDefault();
                Session["Gestor"] = employeechofer.idEmployee;
                if (person == null) return RedirectToAction("Create");
                ViewBag.NIT = "El usuario ya existe";
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                if (logi.GetRoleSessionFromCookie(Request).ToString() != "4" || Session["Gestor"] == null) return RedirectToAction("Index", "Admin");
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }

        [HttpPost]
        public ActionResult Create(EmployeeGestor employeechofer)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                if (logi.GetRoleSessionFromCookie(Request).ToString() != "4" || Session["Gestor"] == null) return RedirectToAction("Index", "Admin");
                employeechofer.idEmployee = Session["Gestor"].ToString();
                TimeZoneInfo boliviaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time");
                DateTime boliviaTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, boliviaTimeZone);
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
                        person = new Person();
                        person.nit = employeechofer.idEmployee.Trim();
                        person.name = employeechofer.name.Trim();
                        person.lastname = employeechofer.lastname.Trim();
                        person.phone = employeechofer.phone.Trim();
                        person.idRole = 3; person.status = 1;
                        person.dateRegister = boliviaTime;
                        context.Person.Add(person);
                        context.SaveChanges();
                    }
                }
                else if (person.status == 0 || person.idRole == 0)
                    using (dbModels context = new dbModels())
                    {
                        person.name = employeechofer.name.Trim();
                        person.lastname = employeechofer.lastname.Trim();
                        person.idRole = 4; person.status = 1;
                        person.phone = employeechofer.phone.Trim();
                        context.Entry(person).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                else
                {
                    TempData["Person"] = "La persona ya está trabajando";
                    return View();
                }
                Employee employee = new Employee();
                employee.dateRegister = boliviaTime;
                employee.idEmployee = employeechofer.idEmployee.Trim();
                employee.status = 1;
                Employee em = new Employee();
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

        public ActionResult Edit(string id)
        {
            try
            {
                if (id == null) return RedirectToAction("Index", "Home");
                Login logi = new Login();
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                Session["EmployeeEditChofer"] = null; Session["PersonaEditChofer"] = null;
                EmployeeGestor employeeChofer = new EmployeeGestor();
                Employee employee = new Employee();
                Person person = new Person();
                User user = new User();
                using (dbModels context = new dbModels()) employee = context.Employee.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                employeeChofer.idEmployee = id;
                using (dbModels context = new dbModels()) person = context.Person.Where(x => x.nit.ToString() == id).FirstOrDefault();
                employeeChofer.phone = person.phone;
                employeeChofer.name = person.name;
                employeeChofer.lastname = person.lastname;
                using (dbModels context = new dbModels()) user = context.User.Where(x => x.idEmployee.ToString() == id).FirstOrDefault();
                employeeChofer.email = user.email;
                Session["PersonaEditChofer"] = person; Session["EmployeeEditChofer"] = employee; Session["UserEditChofer"] = user;
                return View(employeeChofer);
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult Edit(EmployeeGestor employeeChofer)
        {
            try
            {
                Login logi = new Login();
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                Employee employee = (Employee)Session["EmployeeEditChofer"];
                Person person = (Person)Session["PersonaEditChofer"];
                User user = (User)Session["UserEditChofer"];
                User validationMail = new User();
                using (dbModels context = new dbModels()) validationMail = context.User.Where(x => x.email.ToLower().Trim() == employeeChofer.email.ToLower().Trim()).FirstOrDefault();
                if (validationMail != null && user.email.ToLower().Trim() != employeeChofer.email.ToLower().Trim())
                {
                    TempData["Mail"] = "El mail ya esta siendo utilizado";
                    return View();
                }
                using (dbModels context = new dbModels())
                {
                    person.name = employeeChofer.name.Trim();
                    person.lastname = employeeChofer.lastname.Trim();
                    person.phone = employeeChofer.phone.Trim();
                    context.Entry(person).State = System.Data.Entity.EntityState.Modified;
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

        // GET: Admin/Delete/5
        public ActionResult Delete(string id)
        {
            try
            {
                using (dbModels context = new dbModels())
                {
                    List<Line> lines = context.Line.Where(x => x.idAdmin == id).ToList();
                    foreach (var item in lines)
                    {
                        Line line = context.Line.Where(x => x.nameLine == item.nameLine).FirstOrDefault();
                        line.idAdmin = null;
                        context.Entry(line).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                    User user = context.User.Where(x => x.idEmployee == id).FirstOrDefault();
                    context.User.Remove(user);
                    Employee employee = context.Employee.Where(x => x.idEmployee == id).FirstOrDefault();
                    context.Employee.Remove(employee);
                    Person person = context.Person.Where(x => x.nit == id).FirstOrDefault();
                    context.Person.Remove(person);
                    context.SaveChanges();
                }
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
        public ActionResult AsignarGestor(string nameLine)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                ViewBag.NameLine = nameLine;
                using (dbModels context = new dbModels())
                {
                    List<Person> person = context.Person.Where(x => x.idRole == 3 && x.status == 1).ToList();
                    return View(person);
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }

        }
        public ActionResult AsignarGestors(string nameLine, string id)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                using (dbModels context = new dbModels())
                {
                    Line line = context.Line.Where(x => x.nameLine == nameLine).FirstOrDefault();
                    Person person = context.Person.Where(x => x.nit == id && x.idRole == 3 && x.status == 1).FirstOrDefault();
                    if (person != null &&  line != null) 
                    {
                        line.idAdmin = id;
                        context.Entry(line).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                    }
                }
                return RedirectToAction("Index", "Line");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult QuitarGestor(string nameLine)
        {
            try
            {
                var authResult = AutenticarPasosRol(4);
                if (authResult != null) return authResult;
                using (dbModels context = new dbModels())
                {
                    Line line = context.Line.Where(x => x.nameLine == nameLine).FirstOrDefault();
                    line.idAdmin = null;
                    context.Entry(line).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("Index","Line");
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }
    }
}
