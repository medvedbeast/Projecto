using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class IssueAssignment
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public int AssigneeId { get; set; }
        public int RoleId { get; set; }

        public User Assignee { get; set; }
        public Issue Issue { get; set; }
        public IssueAssignmentRole Role { get; set; }
    }
}
