using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class IssueType
    {
        public IssueType()
        {
            Issues = new HashSet<Issue>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Issue> Issues { get; set; }
    }
}
