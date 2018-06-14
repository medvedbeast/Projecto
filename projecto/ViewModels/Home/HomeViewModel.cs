using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<ProjectInfo> Projects { get; set; }
        public IEnumerable<IssueInfo> Issues { get; set; }

        public class ProjectInfo
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public int Done { get; set; }
        }

        public class IssueInfo
        {
            public int Id { get; set; }
            public string ProjectUrl { get; set; }
            public string VersionUrl { get; set; }
            public string Subject { get; set; }
            public string Type { get; set; }
            public string Status { get; set; }
            public string Priority { get; set; }
            public DateTime DueDate { get; set; }
        }
    }
}
