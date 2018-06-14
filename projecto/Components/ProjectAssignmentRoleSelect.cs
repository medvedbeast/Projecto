using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Projecto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.Components
{
    public class ProjectAssignmentRoleSelect : ViewComponent
    {
        private DatabaseContext Database { get; set; }

        public ProjectAssignmentRoleSelect(DatabaseContext context)
        {
            Database = context;
        }

        public IViewComponentResult Invoke(string exclude = "")
        {
            string output = "";

            var roles = Database.ProjectAssignmentRole
                .Select(r => r);
            if (roles.Count() > 0)
            {
                foreach (var r in roles)
                {
                    if (exclude == "" || r.Name != exclude)
                    {
                        output += $"<option value='{r.Id}'>{r.Name.ToLower()}</option>";
                    }
                }
            }

            return new HtmlContentViewComponentResult(new HtmlString(output));
        }
    }
}
