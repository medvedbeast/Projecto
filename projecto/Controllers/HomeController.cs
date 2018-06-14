using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projecto.Data;
using Projecto.Models;
using Projecto.ViewModels;

namespace Projecto.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private DatabaseContext Database { get; set; }

        public HomeController(DatabaseContext context)
        {
            Database = context;
        }

        public ViewResult Index()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var projectsFull = Database.ProjectAssignment
                .Where(a => a.AssigneeId == userId)
                .Select(a => a.Project);
            var projects = new List<HomeViewModel.ProjectInfo>();
            if (projectsFull.Count() > 0)
            {
                foreach (var p in projectsFull)
                {
                    double fullDuration = (p.DueDate - p.StartDate).TotalHours;
                    double elapsed = (DateTime.Now - p.StartDate).TotalHours;
                    int done = Convert.ToInt32(Math.Round(elapsed / (fullDuration * 0.01f)));
                    done = done < 0 ? 0 : (done > 100 ? 100 : done);

                    projects.Add(new HomeViewModel.ProjectInfo
                    {
                        Name = p.Name,
                        Url = p.Url,
                        Done = done
                    });
                }
            }

            var issues = Database.IssueAssignment
                .Include(a => a.Issue).ThenInclude(i => i.Type)
                .Include(a => a.Issue).ThenInclude(i => i.Status)
                .Include(a => a.Issue).ThenInclude(i => i.Priority)
                .Include(a => a.Issue).ThenInclude(i => i.ProjectVersion)
                .Include(a => a.Issue).ThenInclude(i => i.ProjectVersion).ThenInclude(v => v.Project)
                .Where(a => a.AssigneeId == userId && (a.Issue.Status.Name == "New" || a.Issue.Status.Name == "In Progress" || a.Issue.Status.Name == "Feedback"))
                .OrderBy(a => a.Issue.DueDate - DateTime.Today)
                .ThenBy(a => a.Issue.ParentIssueId)
                .ThenBy(a => a.IssueId)
                .Select(a => new HomeViewModel.IssueInfo
                {
                    Id = a.Issue.Id,
                    ProjectUrl = a.Issue.ProjectVersion.Project.Url,
                    VersionUrl = a.Issue.ProjectVersion.Url,
                    Subject = a.Issue.Subject,
                    Type = a.Issue.Type.Name,
                    Status = a.Issue.Status.Name,
                    Priority = a.Issue.Priority.Name,
                    DueDate = a.Issue.DueDate
                }).Take(5);

            return View(new HomeViewModel
            {
                Projects = projects,
                Issues = issues
            });
        }
    }
}