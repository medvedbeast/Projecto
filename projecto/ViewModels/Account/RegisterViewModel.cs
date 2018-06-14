using Projecto.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class RegisterViewModel
    {
        [IsRequired]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [IsRequired]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [IsRequired]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "{0} is invalid.")]
        [UIHint("email")]
        public string Email { get; set; }

        [IsRequired]
        [Display(Name = "Password")]
        [UIHint("password")]
        public string Password { get; set; }
    }
}
