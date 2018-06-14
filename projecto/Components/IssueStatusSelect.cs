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
    public class IssueStatusSelect : ViewComponent
    {
        private DatabaseContext Database { get; set; }

        public IssueStatusSelect(DatabaseContext context)
        {
            Database = context;
        }

        public IViewComponentResult Invoke(int selectedId = 0)
        {
            string output = "";

            var statuses = Database.IssueStatus
                .Select(s => s);
            if (statuses.Count() > 0)
            {
                int index = 0;
                foreach (var s in statuses)
                {
                    string selected = selectedId == 0 ? (index == 0 ? "selected = 'selected'" : "") : (selectedId == s.Id ? "selected='selected'" : "");
                    output += $"<option value='{s.Id}' {selected}>{s.Name.ToLower()}</option>";
                    index++;
                }
            }

            return new HtmlContentViewComponentResult(new HtmlString(output));
        }
    }
}
