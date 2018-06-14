using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class User
    {
        public User()
        {
            IssueAssignments = new HashSet<IssueAssignment>();
            ProjectActivities = new HashSet<ProjectActivity>();
            ProjectAssignments = new HashSet<ProjectAssignment>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int GroupId { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime LastSeenOn { get; set; }

        public UserGroup Group { get; set; }
        public ICollection<IssueAssignment> IssueAssignments { get; set; }
        public ICollection<ProjectActivity> ProjectActivities { get; set; }
        public ICollection<ProjectAssignment> ProjectAssignments { get; set; }
    }
}
