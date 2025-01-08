using ProyectoControlLineaBus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProyectoControlLineaBus.Clases;
using System.Web.UI;
using WebGrease;
using System.Web.Security;

namespace ProyectoControlLineaBus.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        // POST: Login
        [HttpPost]
        public ActionResult Index(Login login)
        {
            try
            {
                User users = new User();
                Employee employee = new Employee();
                Encriptado encriptado = new Encriptado();
                Person person = new Person();

                if (login.Password != null)
                {
                    using (dbModels context = new dbModels())
                    {
                        users = context.User.Where(x => x.username == login.Username).FirstOrDefault();
                        if (users != null)
                        {
                            if (encriptado.VerifyPassword(login.Password, users.password))
                            {
                                employee = context.Employee.Where(x => x.idEmployee == users.idEmployee && x.status != 0).FirstOrDefault();
                                if (employee != null)
                                {
                                    person = context.Person.Where(x => x.nit == employee.idEmployee).FirstOrDefault();
                                    if (person != null)
                                    {
                                        if (users.firstEntry == 0)
                                        {
                                            return RedirectToAction("Validacion", "Login", new { id = users.idEmployee });
                                        }
                                        if (employee.idLine != null)
                                        {
                                            var cookieData = new
                                            {
                                                EmployeeId = employee.idEmployee,
                                                RoleId = person.idRole,
                                                Linea = employee.idLine
                                            };

                                            string userData = Newtonsoft.Json.JsonConvert.SerializeObject(cookieData);

                                            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                                                1,
                                                users.username,
                                                DateTime.Now,
                                                DateTime.Now.AddMinutes(1440),
                                                true,
                                                userData,
                                                "/"
                                            );

                                            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                                            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                                            {
                                                HttpOnly = true,
                                                Expires = ticket.Expiration,
                                            };

                                            Response.Cookies.Add(authCookie);

                                            return RedirectToAction("Index", "Home");
                                        }
                                        else
                                        {
                                            var lines = context.Line.Where(x => x.idAdmin == employee.idEmployee).ToList();
                                            if (lines == null)
                                            {
                                                TempData["Login"] = "No tiene líneas gestionadas";
                                                return View();
                                            }
                                            else
                                            {
                                                var cookieData = new
                                                {
                                                    EmployeeId = employee.idEmployee,
                                                    RoleId = person.idRole,
                                                    Linea = ""
                                                };

                                                string userData = Newtonsoft.Json.JsonConvert.SerializeObject(cookieData);

                                                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                                                    1,
                                                    users.username,
                                                    DateTime.Now,
                                                    DateTime.Now.AddMinutes(1440),
                                                    true,
                                                    userData,
                                                    "/"
                                                );

                                                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                                                HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
                                                {
                                                    HttpOnly = true,
                                                    Expires = ticket.Expiration,
                                                };

                                                Response.Cookies.Add(authCookie);

                                                return RedirectToAction("Index", "Home");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                TempData["Login"] = "El nombre o la contraseña son incorrectos";
                return View();
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        // GET: Login
        public ActionResult Validacion(string id)
        {
            try
            {
                if (id == null) return RedirectToAction("Index", "Home");
                using (dbModels context = new dbModels())
                {
                    User user = context.User.Where(x => x.idEmployee == id).FirstOrDefault();
                    Session["userPassValid"] = user;
                    return View();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        // POST: Login/Validacion
        [HttpPost]
        public ActionResult Validacion(EditPass editPass)
        {
            try
            {
                Person valid = new Person();
                Encriptado encriptado = new Encriptado();
                if(editPass.Password == editPass.Password)
                using (dbModels context = new dbModels())
                {
                    User user = (User)Session["userPassValid"];
                        if (editPass.IsValidString(editPass.Password))
                        {
                            user.password = encriptado.Encriptar(editPass.Password);
                            user.firstEntry = 1;
                            context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                            context.SaveChanges();
                            return RedirectToAction("Index");
                        }
                }
                ViewBag.Mess = "La contraseña debe tener al menos un número y una letra en mayúscula";
                return View();
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        public ActionResult ForgotPassword()
        {
            try
            {
                return View();
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }
        // POST: Login/Validacion
        [HttpPost]
        public ActionResult ForgotPassword(EmployeeControl employeeControl)
        {
            try
            {
                using (dbModels context = new dbModels())
                {
                    User user = context.User.Where(x => x.email.ToLower().Trim() == employeeControl.email.ToLower().Trim()).FirstOrDefault();
                    if (user != null)
                    {
                        Encriptado encriptado = new Encriptado();
                        string pass = employeeControl.GenerarPassword();
                        user.password = encriptado.Encriptar(pass);
                        user.firstEntry = 0;
                        employeeControl.EnviarCorreo(employeeControl.email.ToLower().Trim(), user.username, pass);
                        context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                        context.SaveChanges();
                        ViewBag.Mess2 = "Se envío la nueva contraseña al correo";
                        return View();
                    }
                }
                if(employeeControl.email!=null)
                ViewBag.Mess = "El mail no existe";
                return View();
            }
            catch
            {
                Session["RoleSession"] = null;
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
