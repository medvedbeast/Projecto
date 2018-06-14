using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class VersionBoardViewModel
    {
        public string UserRole { get; set; }
        public string Name { get; set; }
        
        public IEnumerable<StatusInfo> Statuses { get; set; }

        public class IssueInfo
        {
            public int Id { get; set; }
            public string Subject { get; set; }
            public int StatusId { get; set; }
        }

        public class StatusInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<IssueInfo> Issues { get; set; }
        }
    }
}
