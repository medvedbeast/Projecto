using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class ProjectActivitiesViewModel
    {
        public string ProjectName { get; set; }

        public IEnumerable<ActivityInfo> Activities { get; set; }

        public class ActivityInfo
        {
            public DateTime Time { get; set; }
            public string Content { get; set; }
            public string Author { get; set; }
            public int AuthorId { get; set; }
        }
    }
}
