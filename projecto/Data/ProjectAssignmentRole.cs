using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class ProjectAssignmentRole
    {
        public ProjectAssignmentRole()
        {
            ProjectAssignments = new HashSet<ProjectAssignment>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<ProjectAssignment> ProjectAssignments { get; set; }
    }
}
