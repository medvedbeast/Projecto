using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class IssueViewModel
    {
        public string ProjectUrl { get; set; }
        public string ProjectName { get; set; }
        public string VersionUrl { get; set; }
        public string VersionName { get; set; }
        public string UserRole { get; set; }
        public string UserProjectRole { get; set; }

        public int Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int EstimatedTime { get; set; }
        public int Done { get; set; }

        public int? ParentIssueId { get; set; }
        public string ParentIssueSubject { get; set; }

        public IEnumerable<AssigneeInfo> Assignees { get; set; }
        public IEnumerable<AttachmentInfo> Attachments { get; set; }

        public class AssigneeInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
            public int IssueId { get; set; }
        }

        public class AttachmentInfo
        {
            public int Id { get; set; }
            public string Content { get; set; }
            public string Comment { get; set; }
        }
    }
}
