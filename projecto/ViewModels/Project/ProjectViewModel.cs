using Projecto.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class ProjectViewModel
    {
        public string UserRole { get; set; }

        public string Name { get; set; }
        public string Overview { get; set; }
        public string Url { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int OpenIssues { get; set; }
        public int ClosedIssues { get; set; }
        public int OutdatedIssues { get; set; }

        public string ParentProjectUrl { get; set; }
        public string ParentProjectName { get; set; }

        public IEnumerable<AssigneeInfo> Assignees { get; set; }
        public IEnumerable<VersionInfo> Versions { get; set; }

        public class AssigneeInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Role { get; set; }
        }

        public class VersionInfo
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public int OpenIssues { get; set; }
        }
    }
}
