using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Projecto.Data;
using Projecto.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Projecto.Components
{
    public class UserProjectsSelect : ViewComponent
    {
        private DatabaseContext Database { get; set; }

        public UserProjectsSelect(DatabaseContext context)
        {
            Database = context;
        }

        public IViewComponentResult Invoke(int selectedId = 0, int excludedId = -1)
        {
            string output = "";

            int id = Convert.ToInt32(HttpContext.User.Claims.First(c => c.Type == "UserId").Value);
            IEnumerable<Project> projects = Database.ProjectAssignment
                .Include(a => a.Project)
                .Where(a => a.AssigneeId == id)
                .Select(a => a.Project);
            if (projects.Count() > 0)
            {
                foreach (var p in projects)
                {
                    if (excludedId != p.Id)
                    {
                        string selected = selectedId == 0 ? "" : (selectedId == p.Id ? "selected='selected'" : "");
                        output += $"<option value='{p.Id}' {selected}>{p.Name}</option>";
                    }
                }
            }

            return new HtmlContentViewComponentResult(new HtmlString(output));
        }
    }
}
