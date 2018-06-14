using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class AccountViewModel
    {
        public bool IsSelf { get; set; }
        public bool IsPasswordSet { get; set; }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Group { get; set; }

        public int OpenProjects { get; set; }
        public int OpenIssues { get; set; }
        public DateTime RegisteredOn { get; set; }
        public DateTime LastSeenOn { get; set; }

        public IEnumerable<ActivityInfo> Activities { get; set; }

        public class ActivityInfo
        {
            public DateTime Time { get; set; }
            public string ProjectName { get; set; }
            public string ProjectUrl { get; set; }
            public string Content { get; set; }
        }
    }
}
