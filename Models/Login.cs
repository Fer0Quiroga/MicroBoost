using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Xml.Linq;

namespace ProyectoControlLineaBus.Models
{
    public class Login
    {
        [Display(Name = "Username")]
        [MinLength(3)]
        [MaxLength(15)]
        [Required(ErrorMessage = "El username es obligatorio")]
        public string Username{ get; set; }
        [Display(Name = "Contraseña")]
        [MinLength(3)]
        [MaxLength(100)]
        public string Password{ get; set; }
        public Login()
        {
            
        }
        public Login(string username, string password)
        {
            Username = username;
            Password = password;
        }
        public int? GetRoleSessionFromCookie(HttpRequestBase request)
        {
            // Obtener la cookie de autenticación
            HttpCookie authCookie = request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                // Desencriptar el ticket
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                if (ticket != null && !ticket.Expired)
                {
                    var cookieData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ticket.UserData);

                    // Retornar el valor de RoleSession
                    return (int?)cookieData.RoleId;
                }
            }

            // Si no se encuentra la cookie o está expirada, retornar null
            return null;
        }
        public string GetLineaFromCookie(HttpRequestBase request)
        {
            // Obtener la cookie de autenticación
            HttpCookie authCookie = request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                // Desencriptar el ticket
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                if (ticket != null && !ticket.Expired)
                {
                    var cookieData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ticket.UserData);

                    // Retornar el valor de Linea como string
                    return (string)cookieData.Linea;
                }
            }

            // Si no se encuentra la cookie o está expirada, retornar null
            return null;
        }
        public string GetEmployeeIdFromCookie(HttpRequestBase request)
        {
            HttpCookie authCookie = request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);

                if (ticket != null && !ticket.Expired)
                {
                    var cookieData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(ticket.UserData);
                    return (string)cookieData.EmployeeId;
                }
            }

            return null;
        }
    }
}