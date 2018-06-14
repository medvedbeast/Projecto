using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class IssueAssignmentsViewModel
    {
        public int IssueId { get; set; }
        public IEnumerable<UserInfo> Users { get; set; }
        public IEnumerable<AssigneeInfo> Assignees { get; set; }

        public class UserInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class AssigneeInfo : IEquatable<AssigneeInfo>
        {
            public int AssignmentId { get; set; }
            public int Id { get; set; }
            public int RoleId { get; set; }
            public string Role { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            public bool Equals(AssigneeInfo other) => Id == other.Id;
        }
    }
}
