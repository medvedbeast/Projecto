using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class ProjectAssignment
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int AssigneeId { get; set; }
        public int RoleId { get; set; }

        public User Assignee { get; set; }
        public Project Project { get; set; }
        public ProjectAssignmentRole Role { get; set; }
    }
}
