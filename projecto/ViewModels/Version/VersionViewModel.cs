using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class VersionViewModel
    {
        public string UserRole { get; set; }

        public string Name { get; set; }
        public string Url { get; set; }
        public string Overview { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

        public int OpenIssues { get; set; }
        public int ClosedIssues { get; set; }
        public int OutdatedIssues { get; set; }

        public string ProjectName { get; set; }
        public string ProjectUrl { get; set; }

        public IEnumerable<IssueInfo> Issues { get; set; }

        public class IssueInfo
        {
            public int Id { get; set; }
            public string Subject { get; set; }
            public string Type { get; set; }
            public string Status { get; set; }
            public string Priority { get; set; }
            public DateTime DueDate { get; set; }
        }
    }
}
