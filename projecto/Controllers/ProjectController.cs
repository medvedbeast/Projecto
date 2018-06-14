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
    [Route("projects")]
    [Authorize]
    public class ProjectController : Controller
    {
        private DatabaseContext Database { get; set; }

        public ProjectController(DatabaseContext context)
        {
            Database = context;
        }

        [Route("{projectUrl}")]
        public ActionResult Index(string projectUrl)
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var project = Database.Project
                .Include(p => p.ParentProject)
                .Include(p => p.ProjectVersions)
                .Where(p => p.Url == projectUrl)
                .FirstOrDefault();

            int creatorId = Database.ProjectAssignment
                .Include(a => a.Role)
                .Where(a => a.ProjectId == project.Id && a.Role.Name == "Creator")
                .Select(a => a.AssigneeId)
                .FirstOrDefault();

            if (project == null)
            {
                return StatusCode(404);
            }

            List<Issue> projectIssues = new List<Issue>();
            List<ProjectViewModel.VersionInfo> versions = new List<ProjectViewModel.VersionInfo>();
            if (project.ProjectVersions.Count > 0)
            {
                foreach (ProjectVersion version in project.ProjectVersions)
                {
                    var versionIssues = Database.Issue
                        .Include(i => i.Status)
                        .Where(i => i.ProjectVersionId == version.Id);

                    var versionInfo = new ProjectViewModel.VersionInfo
                    {
                        Name = version.Name,
                        Url = Url.Action("Index", "Version", new { versionUrl = version.Url }),
                        OpenIssues = versionIssues.Count() > 0 ? versionIssues.Where(i => i.Status.Name != "Closed").Count() : 0
                    };

                    projectIssues.AddRange(versionIssues);
                    versions.Add(versionInfo);
                }
            }

            var assignees = Database.ProjectAssignment
                .Include(a => a.Role)
                .Include(a => a.Assignee)
                .Where(a => a.ProjectId == project.Id)
                .OrderBy(a => a.RoleId)
                .Select(a => new ProjectViewModel.AssigneeInfo
                {
                    Id = a.AssigneeId,
                    Name = $"{a.Assignee.FirstName} {a.Assignee.LastName}",
                    Role = a.Role.Name
                });

            var userRole = Database.ProjectAssignment
                .Include(a => a.Role)
                .Where(a => a.AssigneeId == userId && a.ProjectId == project.Id)
                .Select(a => a.Role.Name)
                .FirstOrDefault();

            int openIssues = projectIssues
                .Where(i => i.Status.Name == "New" || i.Status.Name == "In Progress" || i.Status.Name == "Feedback")
                .Count();

            int closedIssues = projectIssues
                .Where(i => i.Status.Name != "New" && i.Status.Name != "In Progress" && i.Status.Name != "Feedback")
                .Count();

            int outdatedIssues = projectIssues
                .Where(i => (i.Status.Name == "New" || i.Status.Name == "In Progress" || i.Status.Name == "Feedback") && (i.DueDate < DateTime.Today))
                .Count();

            return View(new ProjectViewModel
            {
                UserRole = userRole,
                Name = project.Name,
                Overview = project.Overview,
                Url = project.Url,
                StartDate = project.StartDate,
                DueDate = project.DueDate,
                OpenIssues = openIssues,
                ClosedIssues = closedIssues,
                OutdatedIssues = outdatedIssues,
                ParentProjectName = project.ParentProject?.Name,
                ParentProjectUrl = project.ParentProject?.Url,
                Versions = versions,
                Assignees = assignees
            });
        }

        [Route("[action]")]
        public ViewResult Create()
        {
            return View(new CreateProjectViewModel
            {
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(1)
            });
        }

        [HttpPost]
        [Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.StartDate > model.DueDate)
                {
                    ModelState.AddModelError("DueDate", "Due date cannot be before start date.");
                    return View(model);
                }

                string[] routeNames = { "create", "index", "delete", "activities", "gantt", "assignments", "update", "wiki" };
                int urlCount = Database.Project
                    .Count(p => p.Url == model.Url);
                if (urlCount == 0 && !routeNames.Contains(model.Url))
                {
                    Project project = new Project
                    {
                        Name = model.Name,
                        Url = model.Url.ToLower(),
                        Overview = model.Overview ?? "todo: ADD PROJECT OVERVIEW",
                        StartDate = model.StartDate,
                        DueDate = model.DueDate,
                        ParentProjectId = model.ParentProjectId == 0 ? (int?)null : model.ParentProjectId,
                        Wiki = "todo: ADD PROJECT WIKI"
                    };
                    Database.Project.Add(project);

                    int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
                    int roleId = Database.ProjectAssignmentRole
                        .Where(r => r.Name == "Creator")
                        .Select(r => r.Id)
                        .FirstOrDefault();
                    ProjectAssignment assignment = new ProjectAssignment
                    {
                        ProjectId = project.Id,
                        AssigneeId = userId,
                        RoleId = roleId
                    };
                    Database.ProjectAssignment.Add(assignment);

                    ProjectVersion version = new ProjectVersion
                    {
                        ProjectId = project.Id,
                        Url = "default",
                        Name = $"{project.Name} // Default version",
                        Overview = "Default project version",
                        StartDate = project.StartDate,
                        DueDate = project.DueDate
                    };
                    Database.ProjectVersion.Add(version);

                    ProjectActivity activity = new ProjectActivity
                    {
                        ProjectId = project.Id,
                        AuthorId = userId,
                        Content = $"Project created"
                    };
                    Database.ProjectActivity.Add(activity);

                    await Database.SaveChangesAsync();

                    return RedirectToAction("Index", new { projectUrl = project.Url });
                }
                else
                {
                    ModelState.AddModelError("Url", "Such url is taken.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [Route("{projectUrl}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string projectUrl)
        {
            Project project = Database.Project
                .Where(p => p.Url == projectUrl)
                .FirstOrDefault();

            Database.Remove(project);

            await Database.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [Route("{projectUrl}/[action]")]
        public ViewResult Update(string projectUrl)
        {
            Project project = Database.Project
                .Where(p => p.Url == projectUrl)
                .FirstOrDefault();

            return View(new UpdateProjectViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Url = project.Url,
                Overview = project.Overview,
                StartDate = project.StartDate,
                DueDate = project.DueDate,
                ParentProjectId = project.ParentProjectId
            });
        }

        [HttpPost]
        [Route("{projectUrl}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateProjectViewModel model, [FromRoute]string projectUrl)
        {
            if (ModelState.IsValid)
            {
                if (model.StartDate > model.DueDate)
                {
                    ModelState.AddModelError("DueDate", "Due date cannot be before start date.");
                    return View(model);
                }

                int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

                Project project = Database.Project
                    .Where(p => p.Url == projectUrl)
                    .FirstOrDefault();

                int access = Database.ProjectAssignment
                    .Include(a => a.Role)
                    .Where(a => a.ProjectId == project.Id && a.AssigneeId == userId && a.Role.Name == "Creator")
                    .Count();

                if (access != 1)
                {
                    return StatusCode(403);
                }

                string[] routeNames = { "create", "index", "delete" };
                int urlCount = Database.Project
                    .Count(p => p.Url == model.Url && p.Id != project.Id);
                if (urlCount == 0 && !routeNames.Contains(model.Url))
                {
                    project.Name = model.Name;
                    project.Url = model.Url;
                    project.Overview = model.Overview;
                    project.StartDate = model.StartDate;
                    project.DueDate = model.DueDate;
                    project.ParentProjectId = model.ParentProjectId == 0 ? null : model.ParentProjectId;

                    ProjectActivity activity = new ProjectActivity
                    {
                        ProjectId = project.Id,
                        AuthorId = userId,
                        Content = $"Project information updated"
                    };
                    Database.ProjectActivity.Add(activity);

                    await Database.SaveChangesAsync();

                    return RedirectToAction("Index", "Project", new { projectUrl = project.Url });
                }
                else
                {
                    ModelState.AddModelError("Url", "Such url is taken.");
                }
            }

            return View(model);
        }

        [Route("{projectUrl}/[action]")]
        public IActionResult Assignments(string projectUrl)
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var userRole = Database.ProjectAssignment
                .Include(a => a.Role)
                .Where(a => a.AssigneeId == userId)
                .Select(a => a.Role.Name)
                .FirstOrDefault();

            if (userRole != "Creator" && userRole != "Manager")
            {
                return RedirectToAction("Index");
            }

            var project = Database.Project
                .Where(p => p.Url == projectUrl)
                .FirstOrDefault();

            var assignees = Database.ProjectAssignment
                .Include(a => a.Assignee)
                .Include(a => a.Project)
                .Include(a => a.Role)
                .Where(a => a.Project.Url == projectUrl)
                .Select(a => new ProjectAssignmentsViewModel.AssigneeInfo
                {
                    Id = a.AssigneeId,
                    Name = $"{a.Assignee.FirstName} {a.Assignee.LastName}",
                    Email = a.Assignee.Email,
                    RoleId = a.RoleId,
                    Role = a.Role.Name,
                    AssignmentId = a.Id
                }).OrderBy(a => a.Role);

            var users = Database.User
                .Where(u => u.Id != userId && !assignees.Contains(new ProjectAssignmentsViewModel.AssigneeInfo { Id = u.Id }))
                .Select(u => new ProjectAssignmentsViewModel.UserInfo
                {
                    Id = u.Id,
                    Name = $"{u.FirstName} {u.LastName}"
                });

            return View(new ProjectAssignmentsViewModel
            {
                ProjectName = project.Name,
                Assignees = assignees,
                Users = users
            });
        }

        [HttpPost]
        [Route("{projectUrl}/assignments/create")]
        public async Task<IActionResult> CreateAssignments([FromBody]CreateProjectAssignmentsViewModel model, string projectUrl)
        {
            int projectId = Database.Project
                .Where(p => p.Url == projectUrl)
                .Select(p => p.Id)
                .FirstOrDefault();

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            foreach (int id in model.UserIdList)
            {
                var user = Database.User
                    .Where(u => u.Id == id)
                    .FirstOrDefault();

                string roleName = Database.ProjectAssignmentRole
                    .Where(r => r.Id == model.RoleId)
                    .Select(r => r.Name)
                    .FirstOrDefault();

                var assignment = new ProjectAssignment
                {
                    ProjectId = projectId,
                    RoleId = model.RoleId,
                    AssigneeId = id
                };
                Database.ProjectAssignment.Add(assignment);

                ProjectActivity activity = new ProjectActivity
                {
                    ProjectId = projectId,
                    AuthorId = userId,
                    Content = $"{user.FirstName} {user.LastName} was assigned as {roleName}"
                };
                Database.ProjectActivity.Add(activity);


            }


            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true,
                url = Url.Action("Assignments")
            });
        }

        [HttpPost]
        [Route("{projectUrl}/assignments/delete")]
        public async Task<IActionResult> DeleteAssignment([FromBody]DeleteProjectAssignmentViewModel model, string projectUrl)
        {
            var assignment = Database.ProjectAssignment
                .Where(a => a.Id == model.Id)
                .FirstOrDefault();

            var user = Database.User
                .Where(u => u.Id == assignment.AssigneeId)
                .FirstOrDefault();

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var issueAssignments = Database.IssueAssignment
                .Where(a => a.AssigneeId == user.Id);

            Database.RemoveRange(issueAssignments);

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = assignment.ProjectId,
                AuthorId = userId,
                Content = $"{user.FirstName} {user.LastName} was removed from project"
            };
            Database.ProjectActivity.Add(activity);

            Database.Remove(assignment);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true,
                url = Url.Action("Assignments")
            });
        }

        [HttpPost]
        [Route("{projectUrl}/assignments/update")]
        public async Task<IActionResult> UpdateAssignment([FromBody]UpdateProjectAssignmentViewModel model, string projectUrl)
        {
            var assignment = Database.ProjectAssignment
                .Where(a => a.Id == model.AssignmentId)
                .FirstOrDefault();

            assignment.RoleId = model.RoleId;

            var user = Database.User
               .Where(u => u.Id == assignment.AssigneeId)
               .FirstOrDefault();

            string roleName = Database.ProjectAssignmentRole
                .Where(r => r.Id == model.RoleId)
                .Select(r => r.Name)
                .FirstOrDefault();

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = assignment.ProjectId,
                AuthorId = userId,
                Content = $"{user.FirstName} {user.LastName} was re-assigned as {roleName}"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true,
                url = Url.Action("Assignments")
            });
        }

        [Route("{projectUrl}/activities")]
        public IActionResult Activities(string projectUrl)
        {
            var project = Database.Project
                .Where(p => p.Url == projectUrl)
                .FirstOrDefault();

            var activities = Database.ProjectActivity
                .Include(a => a.Author)
                .Where(a => a.ProjectId == project.Id)
                .OrderByDescending(a => a.Time)
                .Select(a => new ProjectActivitiesViewModel.ActivityInfo
                {
                    Time = a.Time,
                    Content = a.Content,
                    Author = $"{a.Author.FirstName} {a.Author.LastName}",
                    AuthorId = a.AuthorId
                });
            return View(new ProjectActivitiesViewModel
            {
                Activities = activities,
                ProjectName = project.Name
            });
        }

        [Route("{projectUrl}/wiki")]
        public IActionResult Wiki(string projectUrl)
        {
            var project = Database.Project
                .Where(p => p.Url == projectUrl)
                .FirstOrDefault();

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var userRole = Database.ProjectAssignment
                .Include(a => a.Role)
                .Where(a => a.ProjectId == project.Id && a.AssigneeId == userId)
                .Select(a => a.Role.Name)
                .FirstOrDefault();

            var safeContent = project.Wiki
                .Replace("\"", "\\\"")
                .Replace("\'", "\\\'")
                .Replace("`", "&#96;")
                .Replace("\n", "");

            return View(new ProjectWikiViewModel
            {
                UserRole = userRole,
                Content = safeContent,
                ProjectName = project.Name
            });
        }

        [HttpPost]
        [Route("{projectUrl}/wiki/update")]
        public async Task<IActionResult> UpdateWiki([FromBody]UpdateProjectWikiViewModel model, string projectUrl)
        {
            var project = Database.Project
                .Where(p => p.Url == projectUrl)
                .FirstOrDefault();

            project.Wiki = model.Content;

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

            await Database.SaveChangesAsync();

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = project.Id,
                AuthorId = userId,
                Content = $"Project WIKI updated"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true
            });
        }

        [Route("{projectUrl}/[action]")]
        public IActionResult Gantt(string projectUrl)
        {
            var project = Database.Project
                .Where(p => p.Url == projectUrl)
                .FirstOrDefault();

            List<ProjectDiagramViewModel.VersionInfo> versions = Database.ProjectVersion
                .Where(v => v.ProjectId == project.Id)
                .Select(v => new ProjectDiagramViewModel.VersionInfo
                {
                    Id = v.Id,
                    Name = v.Name,
                    Url = v.Url,
                    StartDate = v.StartDate,
                    DueDate = v.DueDate
                })
                .ToList();

            foreach (var version in versions)
            { 
                var versionIssues = Database.Issue
                    .Include(i => i.Status)
                    .Where(i => i.ProjectVersionId == version.Id)
                    .OrderBy(i => i.StartDate)
                    .ThenBy(i => i.ParentIssueId)
                    .ThenBy(i => i.Id)
                    .Select(i => new ProjectDiagramViewModel.IssueInfo
                    {
                        Id = i.Id,
                        Subject = i.Subject,
                        Status = i.Status.Name,
                        StartDate = i.StartDate,
                        DueDate = i.DueDate,
                        Done = i.Done
                    });

                version.Issues = versionIssues;
            }

            DateTime startDate = project.StartDate;
            DateTime dueDate = project.DueDate;
            int monthsCount = (dueDate.Month - startDate.Month) + 1;

            return View(new ProjectDiagramViewModel
            {
                ProjectName = project.Name,
                Versions = versions,
                StartDate = startDate,
                DueDate = dueDate,
                MonthsCount = monthsCount
            });
        }
    }
}