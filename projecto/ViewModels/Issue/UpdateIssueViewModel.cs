using Projecto.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class UpdateIssueViewModel
    {
        public string ProjectUrl { get; set; }
        public int VersionId { get; set; }
        public string VersionUrl { get; set; }
        public int Id { get; set; }
        public int? ParentIssueId { get; set; }

        [IsRequired]
        [Display(Name = "Subject")]
        public string Subject { get; set; }

        [IsRequired]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [IsRequired]
        [Display(Name = "Issue type")]
        public int TypeId { get; set; }

        [IsRequired]
        [Display(Name = "Issue status")]
        public int StatusId { get; set; }

        [IsRequired]
        [Display(Name = "Issue priority")]
        public int PriorityId { get; set; }

        [IsRequired]
        [UIHint("date")]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [IsRequired]
        [UIHint("date")]
        [Display(Name = "Due date")]
        public DateTime DueDate { get; set; }

        [IsRequired]
        [Range(0, int.MaxValue, ErrorMessage = "{0} is out of range.")]
        [Display(Name = "Estimated time")]
        public int EstimatedTime { get; set; }

        [IsRequired]
        [Range(0, 100, ErrorMessage = "{0} is out of range.")]
        [Display(Name = "Progress")]
        public int Done { get; set; }
    }
}
