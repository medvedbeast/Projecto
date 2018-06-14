using Projecto.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class LoginViewModel
    {
        [IsRequired]
        [UIHint("email")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "{0} is invalid.")]
        public string Email { get; set; }

        [IsRequired]
        [UIHint("password")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }
    }
}
