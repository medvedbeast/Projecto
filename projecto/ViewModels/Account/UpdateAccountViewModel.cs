using Projecto.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class UpdateAccountViewModel
    {
        [IsRequired]
        [Display(Name = "First name")]
        public string FirstName { get; set; }

        [IsRequired]
        [Display(Name = "Last name")]
        public string LastName { get; set; }

        [IsRequired]
        [UIHint("email")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "{0} is invalid.")]
        public string Email { get; set; }

        [UIHint("password")]
        public string NewPassword { get; set; }

        [UIHint("password")]
        public string NewPasswordConfirmation { get; set; }
    }
}
