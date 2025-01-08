using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ProyectoControlLineaBus.Models
{
    public class EditPass
    {
        public EditPass()
        {
            
        }

        public EditPass(string password, string confirmPassword)
        {
            Password = password;
            ConfirmPassword = confirmPassword;
        }

        [Display(Name = "Escriba la nueva Contraseña")]
        [MinLength(4)]
        [MaxLength(15)]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "La Contraseña es obligatoria")]
        public string Password { get; set; }
        [Display(Name = "Escriba Nuevamente la Contraseña")]
        [MinLength(4)]
        [MaxLength(15)]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "La Contraseña es obligatoria")]
        public string ConfirmPassword { get; set; }
        public bool IsValidString(string input)
        {
            if (input.Length >= 4)
            {
                int digitCount = 0;
                int uppercaseCount = 0;
                foreach (char c in input)
                {
                    if (char.IsDigit(c)) 
                        digitCount++;
                    else if (char.IsUpper(c))
                        uppercaseCount++;
                }
                if (digitCount >= 1 && uppercaseCount >= 1) return true;
            }
            return false;
        }

    }
}