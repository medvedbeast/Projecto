using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class ProjectDiagramViewModel
    {
        public string ProjectName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public int MonthsCount { get; set; }

        public IEnumerable<VersionInfo> Versions { get; set; }

        public class VersionInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime DueDate { get; set; }
            public IEnumerable<IssueInfo> Issues { get; set; }
        }

        public class IssueInfo
        {
            public int Id { get; set; }
            public string Subject { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime DueDate { get; set; }
            public string Status { get; set; }
            public int Done { get; set; }
        }
    }
}
