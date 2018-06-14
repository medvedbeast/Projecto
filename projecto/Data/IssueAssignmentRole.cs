using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class IssueAssignmentRole
    {
        public IssueAssignmentRole()
        {
            IssueAssignments = new HashSet<IssueAssignment>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<IssueAssignment> IssueAssignments { get; set; }
    }
}
