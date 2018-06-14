using Projecto.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class ProjectAssignmentsViewModel
    {
        public string ProjectName { get; set; }
        public IEnumerable<UserInfo> Users { get; set; }
        public IEnumerable<AssigneeInfo> Assignees { get; set; }

        public class UserInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class AssigneeInfo : IEquatable<AssigneeInfo>
        {
            public int Id { get; set; }
            public int RoleId { get; set; }
            public string Role { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public int AssignmentId { get; set; }

            public bool Equals(AssigneeInfo other) => Id == other.Id;
        }
    }
}
