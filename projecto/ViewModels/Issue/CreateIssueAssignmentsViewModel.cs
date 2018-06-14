using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.ViewModels
{
    public class CreateIssueAssignmentsViewModel
    {
        public int RoleId { get; set; }
        public IEnumerable<int> UserIdList { get; set; }
    }
}
