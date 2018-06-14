using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class ProjectVersion
    {
        public ProjectVersion()
        {
            Issues = new HashSet<Issue>();
        }

        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }

        public Project Project { get; set; }
        public ICollection<Issue> Issues { get; set; }
    }
}
