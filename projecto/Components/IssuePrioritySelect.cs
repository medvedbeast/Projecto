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
    public class IssuePrioritySelect : ViewComponent
    {
        private DatabaseContext Database { get; set; }

        public IssuePrioritySelect(DatabaseContext context)
        {
            Database = context;
        }

        public IViewComponentResult Invoke(int selectedId = 0)
        {
            string output = "";

            var priorities = Database.IssuePriority
                .Select(p => p);
            if (priorities.Count() > 0)
            {
                int index = 0;
                foreach (var p in priorities)
                {
                    string selected = selectedId == 0 ? (index == 0 ? "selected = 'selected'" : "") : (selectedId == p.Id ? "selected='selected'" : "");
                    output += $"<option value='{p.Id}' {selected}>{p.Name.ToLower()}</option>";
                    index++;
                }
            }

            return new HtmlContentViewComponentResult(new HtmlString(output));
        }
    }
}
