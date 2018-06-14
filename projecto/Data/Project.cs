using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class Project
    {
        public Project()
        {
            ProjectActivities = new HashSet<ProjectActivity>();
            ProjectAssignments = new HashSet<ProjectAssignment>();
            ProjectVersions = new HashSet<ProjectVersion>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Overview { get; set; }
        public int? ParentProjectId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Wiki { get; set; }

        public Project ParentProject { get; set; }
        public ICollection<ProjectActivity> ProjectActivities { get; set; }
        public ICollection<ProjectAssignment> ProjectAssignments { get; set; }
        public ICollection<ProjectVersion> ProjectVersions { get; set; }
    }
}
