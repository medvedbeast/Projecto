using System;
using System.Collections.Generic;

namespace Projecto.Data
{
    public partial class IssuePriority
    {
        public IssuePriority()
        {
            Issues = new HashSet<Issue>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Issue> Issues { get; set; }
    }
}
