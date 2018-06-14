using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Projecto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Projecto.Components
{
    public class VersionIssuesSelect : ViewComponent
    {
        private DatabaseContext Database { get; set; }

        public VersionIssuesSelect(DatabaseContext context)
        {
            Database = context;
        }

        public IViewComponentResult Invoke(int versionId = 0, int selectedId = 0, int excludedId = -1)
        {
            string output = "";

            var issues = Database.Issue
                .Where(i => i.ProjectVersionId == versionId);
            if (issues.Count() > 0)
            {
                foreach (var i in issues)
                {
                    if (excludedId != i.Id)
                    {
                        string selected = selectedId == 0 ? "" : (selectedId == i.Id ? "selected='selected'" : "");
                        output += $"<option value='{i.Id}' {selected}>{i.Subject}</option>";
                    }
                }
            }

            return new HtmlContentViewComponentResult(new HtmlString(output));
        }
    }
}
