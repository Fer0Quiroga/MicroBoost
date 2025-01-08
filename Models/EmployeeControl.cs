using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ProyectoControlLineaBus.Models
{
    public class EmployeeControl
    {
        [Display(Name = "CI")]
        [MinLength(3)]
        [MaxLength(16)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El CI es obligatorio")]
        public string idEmployee{ get; set; }
        [Display(Name = "Salario")]
        [DataType(DataType.Currency)]
        public decimal salary { get; set; }
        [Display(Name = "E-mail")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "El E-mail es obligatorio")]
        public string email { get; set; }
        public int idCheckpoint { get; set; }
        [Display(Name = "Nombre")]
        [MinLength(3)]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string name { get; set; }
        [Display(Name = "Apellido")]
        [MinLength(3)]
        [MaxLength(50)]
        [DataType(DataType.Text)]
        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string lastname { get; set; }
        public Checkpoint checkpoint { get; set; }
        public EmployeeControl()
        {
            
        }

        public EmployeeControl(string idEmployee, decimal salary, string email, int idCheckpoint, string name, string lastname, Checkpoint checkpoint) : this(idEmployee, salary, email, idCheckpoint)
        {
            this.name = name;
            this.lastname = lastname;
            this.checkpoint = checkpoint;
        }

        public EmployeeControl(string idEmployee, decimal salary, string email, int idCheckpoint)
        {
            this.idEmployee = idEmployee;
            this.salary = salary;
            this.email = email;
            this.idCheckpoint = idCheckpoint;
        }
        public string GenerarUsername(string name, string lastname)
        {
            User user = new User();
            string username = ""; char u = name[0]; char l = lastname[0];
            Random rdm = new Random();
            string usernameVer = "";
            while (username == usernameVer)
            {
                username = u.ToString()+ l.ToString();
                for (int i = 0; i < 5; i++)
                {
                    username = username + rdm.Next(0, 10);
                }
                using (dbModels context = new dbModels())
                {
                    user = context.User.Where(x => x.username.ToString() == username).FirstOrDefault();
                }
                if (user != null) usernameVer = user.password;
            }
            return username.ToUpper().Trim();
        }
        public string GenerarPassword()
        {
            string pass = "";
            Random rdm = new Random();
            for (int i = 0; i < 6; i++) pass = pass + rdm.Next(0, 10);
            return pass;
        }
        public void EnviarCorreo(string email, string username, string contrasenia)
        {
            string smtpServer = "smtp.gmail.com"; // Cambia esto por la dirección de tu servidor SMTP
            int smtpPort = 587; // Puerto SMTP (puede variar según el proveedor)
            string correoRemitente = "BusCbba@gmail.com"; // Cambia esto por tu dirección de correo
            string contraseña = "xnqz uvhs weyv pdiq"; // Cambia esto por tu contraseña de correo

            // Configurar el mensaje
            string destinatario = email; // Cambia esto por la dirección de correo del destinatario
            string asunto = "Cuenta";
            string cuerpoMensaje = "La cuenta fue creada excitosamente \n Username: "+ username+"\n Password: "+contrasenia;

            // Crear un objeto de la clase SmtpClient
            SmtpClient clienteSmtp = new SmtpClient(smtpServer, smtpPort);
            clienteSmtp.Credentials = new NetworkCredential(correoRemitente, contraseña);
            clienteSmtp.EnableSsl = true; // Habilitar SSL si es necesario

            // Crear un objeto de la clase MailMessage
            MailMessage mensaje = new MailMessage(correoRemitente, destinatario, asunto, cuerpoMensaje);

            try
            {
                // Enviar el correo
                clienteSmtp.Send(mensaje);
                Console.WriteLine("Correo enviado con éxito.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar el correo: " + ex.Message);
            }

        }
    }
}