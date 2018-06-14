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
    public class IssueTypeSelect : ViewComponent
    {
        private DatabaseContext Database { get; set; }

        public IssueTypeSelect(DatabaseContext context)
        {
            Database = context;
        }

        public IViewComponentResult Invoke(int selectedId = 0)
        {
            string output = "";

            var types = Database.IssueType
                .Select(t => t);
            if (types.Count() > 0)
            {
                int index = 0;
                foreach (var t in types)
                {
                    string selected = selectedId == 0 ? (index == 0 ? "selected = 'selected'" : "") : (selectedId == t.Id ? "selected='selected'" : "");
                    output += $"<option value='{t.Id}' {selected}>{t.Name.ToLower()}</option>";
                    index++;
                }
            }

            return new HtmlContentViewComponentResult(new HtmlString(output));
        }
    }
}
