using Projecto.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class UpdateVersionViewModel
    {
        [IsRequired]
        [Display(Name = "Version name")]
        public string Name { get; set; }

        [IsUrl]
        [IsRequired]
        [Display(Name = "Version url")]
        public string Url { get; set; }

        public string Overview { get; set; }

        [IsRequired]
        [UIHint("date")]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [IsRequired]
        [UIHint("date")]
        [Display(Name = "Due date")]
        public DateTime DueDate { get; set; }

        public string ProjectName { get; set; }
        public string ProjectUrl { get; set; }
    }
}
