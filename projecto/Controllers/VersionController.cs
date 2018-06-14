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
    [Route("projects/{projectUrl}/versions")]
    public class VersionController : Controller
    {
        private DatabaseContext Database { get; }

        public VersionController(DatabaseContext context)
        {
            Database = context;
        }

        [Route("{versionUrl}")]
        public ViewResult Index(string projectUrl, string versionUrl)
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var userRole = Database.ProjectAssignment
                .Include(a => a.Role)
                .Include(a => a.Project)
                .Where(a => a.AssigneeId == userId && a.Project.Url == projectUrl)
                .Select(a => a.Role.Name)
                .FirstOrDefault();

            var version = Database.ProjectVersion
                .Include(v => v.Project)
                .Where(v => v.Url == versionUrl && v.Project.Url == projectUrl)
                .FirstOrDefault();

            int creatorId = Database.ProjectVersion
                .Include(v => v.Project)
                .ThenInclude(p => p.ProjectAssignments)
                .ThenInclude(pa => pa.Role)
                .SelectMany(v => v.Project.ProjectAssignments)
                .Where(a => a.Role.Name == "Creator" && a.ProjectId == version.ProjectId)
                .Select(a => a.AssigneeId)
                .FirstOrDefault();

            var issues = Database.Issue
                .Where(i => i.ProjectVersionId == version.Id)
                .Include(i => i.Type)
                .Include(i => i.Status)
                .Include(i => i.Priority)
                .OrderBy(i => i.StartDate)
                .ThenBy(i => i.ParentIssueId)
                .Select(i => new VersionViewModel.IssueInfo
                {
                    Id = i.Id,
                    Subject = i.Subject,
                    Type = i.Type.Name,
                    Status = i.Status.Name,
                    Priority = i.Priority.Name,
                    DueDate = i.DueDate
                });

            int openIssues = issues
                .Where(i => i.Status == "New" || i.Status == "In Progress" || i.Status == "Feedback")
                .Count();

            int closedIssues = issues
                .Where(i => i.Status != "New" && i.Status != "In Progress" && i.Status != "Feedback")
                .Count();

            int outdatedIssues = issues
                .Where(i => (i.Status == "New" || i.Status == "In Progress" || i.Status == "Feedback") && (i.DueDate < DateTime.Today))
                .Count();

            return View(new VersionViewModel
            {
                UserRole = userRole,
                Name = version.Name,
                Url = version.Url,
                Overview = version.Overview,
                StartDate = version.StartDate,
                DueDate = version.DueDate,
                ProjectName = version.Project.Name,
                ProjectUrl = version.Project.Url,
                ClosedIssues = closedIssues,
                OpenIssues = openIssues,
                OutdatedIssues = outdatedIssues,
                Issues = issues
            });
        }

        [Route("[action]")]
        public IActionResult Create(string projectUrl)
        {
            Project project = Database.Project
                .Where(p => p.Url == projectUrl)
                .Select(p => new Project
                {
                    Overview = p.Overview,
                    StartDate = p.StartDate,
                    DueDate = p.DueDate
                }).FirstOrDefault();

            return View(new CreateVersionViewModel
            {
                ProjectName = project.Name,
                Name = "New Project Version",
                Overview = project.Overview,
                Url = "new-version",
                StartDate = project.StartDate,
                DueDate = project.DueDate
            });
        }

        [HttpPost]
        [Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVersionViewModel model, string projectUrl)
        {
            if (ModelState.IsValid)
            {
                if (model.StartDate > model.DueDate)
                {
                    ModelState.AddModelError("DueDate", "Due date cannot be before start date.");
                    return View(model);
                }

                int versionCount = Database.ProjectVersion
                    .Include(v => v.Project)
                    .Where(v => v.Project.Url == projectUrl && v.Url == model.Url)
                    .Select(v => v.Id)
                    .Count();

                bool isUnique = versionCount == 0 ? true : false;
                if (!isUnique)
                {
                    ModelState.AddModelError("Version", "Such url is taken.");
                    return View(model);
                }

                Project project = Database.Project
                    .Where(p => p.Url == projectUrl)
                    .FirstOrDefault();

                if (model.StartDate < project.StartDate)
                {
                    ModelState.AddModelError("StartDate", $"Start date must be after or equal {project.StartDate:dd.MM.yy}");
                    return View(model);
                }
                if (model.DueDate > project.DueDate)
                {
                    ModelState.AddModelError("DueDate", $"Due date must be before or equal {project.DueDate:dd.MM.yy}");
                    return View(model);
                }

                ProjectVersion version = new ProjectVersion
                {
                    Name = model.Name,
                    Overview = model.Overview,
                    StartDate = model.StartDate,
                    DueDate = model.DueDate,
                    Url = model.Url,
                    ProjectId = project.Id
                };

                Database.ProjectVersion.Add(version);

                int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

                ProjectActivity activity = new ProjectActivity
                {
                    ProjectId = project.Id,
                    AuthorId = userId,
                    Content = $"Created version '{version.Name}'"
                };
                Database.ProjectActivity.Add(activity);

                await Database.SaveChangesAsync();

                return RedirectToAction("Index", "Version", new { versionUrl = version.Url });
            }

            return View(model);
        }

        [Route("{versionUrl}/[action]")]
        public ViewResult Update(string projectUrl, string versionUrl)
        {
            var version = Database.ProjectVersion
                .Include(v => v.Project)
                .Where(v => v.Url == versionUrl && v.Project.Url == projectUrl)
                .FirstOrDefault();

            return View(new UpdateVersionViewModel
            {
                Name = version.Name,
                Url = version.Url,
                Overview = version.Overview,
                StartDate = version.StartDate,
                DueDate = version.DueDate,
                ProjectName = version.Project.Name,
                ProjectUrl = version.Project.Url
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{versionUrl}/[action]")]
        public async Task<IActionResult> Update(UpdateVersionViewModel model, string projectUrl, string versionUrl)
        {
            if (ModelState.IsValid)
            {
                if (model.StartDate > model.DueDate)
                {
                    ModelState.AddModelError("DueDate", "Due date cannot be before start date.");
                    return View(model);
                }

                int versionCount = Database.ProjectVersion
                    .Include(v => v.Project)
                    .Where(v => v.Project.Url == projectUrl && v.Url == model.Url)
                    .Select(v => v.Id)
                    .Count();

                if (versionCount == 0)
                {
                    Project project = Database.Project
                    .Where(p => p.Url == projectUrl)
                    .FirstOrDefault();

                    if (model.StartDate < project.StartDate)
                    {
                        ModelState.AddModelError("StartDate", $"Start date must be after or equal {project.StartDate:dd.MM.yy HH:mm}");
                        return View(model);
                    }
                    if (model.DueDate > project.DueDate)
                    {
                        ModelState.AddModelError("DueDate", $"Due date must be before or equal {project.DueDate:dd.MM.yy HH:mm}");
                        return View(model);
                    }
                    ProjectVersion version = Database.ProjectVersion
                   .Where(v => v.ProjectId == project.Id && v.Url == versionUrl)
                   .FirstOrDefault();

                    version.Name = model.Name;
                    version.Url = model.Url;
                    version.Overview = model.Overview;
                    version.StartDate = model.StartDate;
                    version.DueDate = model.DueDate;


                    int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

                    ProjectActivity activity = new ProjectActivity
                    {
                        ProjectId = project.Id,
                        AuthorId = userId,
                        Content = $"Updated version '{version.Name}'"
                    };
                    Database.ProjectActivity.Add(activity);

                    await Database.SaveChangesAsync();

                    return RedirectToAction("Index", new { projectUrl = projectUrl, versionUrl = version.Url });
                }
                else
                {
                    ModelState.AddModelError("Version", "Such url is taken.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Route("{versionUrl}/[action]")]
        public async Task<IActionResult> Delete(string projectUrl, string versionUrl)
        {
            var version = Database.ProjectVersion
                .Include(v => v.Project)
                .Where(v => v.Project.Url == projectUrl && v.Url == versionUrl)
                .FirstOrDefault();

            int count = Database.ProjectVersion
                .Include(v => v.Project)
                .Where(v => v.Project.Url == projectUrl)
                .Count();

            if (count < 2)
            {
                return Json(new
                {
                    status = false
                });
            }

            Database.Remove(version);

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = version.ProjectId,
                AuthorId = userId,
                Content = $"Deleted version '{version.Name}'"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true,
                url = Url.Action("Index", "Project", new { projectUrl = projectUrl })
            });
        }

        [Route("{versionUrl}/[action]")]
        public IActionResult Board(string projectUrl, string versionUrl)
        {
            var projectId = Database.Project
                .Where(p => p.Url == projectUrl)
                .Select(p => p.Id)
                .FirstOrDefault();

            var version = Database.ProjectVersion
                .Where(v => v.Url == versionUrl && v.ProjectId == projectId)
                .FirstOrDefault();

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            var userRole = Database.ProjectAssignment
                .Include(a => a.Role)
                .Where(a => a.AssigneeId == userId && a.ProjectId == projectId)
                .Select(a => a.Role.Name)
                .FirstOrDefault();

            Dictionary<int, IEnumerable<VersionBoardViewModel.IssueInfo>> issues = new Dictionary<int, IEnumerable<VersionBoardViewModel.IssueInfo>>();

            var statuses = Database.IssueStatus
                .Include(s => s.Issues)
                .Select(s => new VersionBoardViewModel.StatusInfo
                {
                    Id = s.Id,
                    Name = s.Name,
                    Issues = s.Issues
                        .Where(i => i.ProjectVersionId == version.Id)
                        .Select(i => new VersionBoardViewModel.IssueInfo
                        {
                            Id = i.Id,
                            StatusId = i.StatusId,
                            Subject = i.Subject
                        })
                });

            return View(new VersionBoardViewModel
            {
                UserRole = userRole,
                Name = version.Name,
                Statuses = statuses
            });
        }

        [HttpPost]
        [Route("{versionUrl}/board/update")]
        public async Task<IActionResult> UpdateBoard([FromBody]UpdateVersionBoardViewModel model)
        {
            var issue = Database.Issue
                .Include(i => i.ProjectVersion)
                .Where(i => i.Id == model.IssueId)
                .FirstOrDefault();

            issue.StatusId = model.StatusId;

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = issue.ProjectVersion.ProjectId,
                AuthorId = userId,
                Content = $"Updated issue '#{issue.Id}: {issue.Subject}'"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true
            });
        }
    }
}
