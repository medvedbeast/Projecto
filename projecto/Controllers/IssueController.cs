using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projecto.Data;
using Projecto.Models;
using Projecto.ViewModels;
using System.Drawing;

namespace Projecto.Controllers
{
    [Authorize]
    [Route("projects/{projectUrl}/versions/{versionUrl}/issues")]
    public class IssueController : Controller
    {
        private DatabaseContext Database { get; }

        public IssueController(DatabaseContext context)
        {
            Database = context;
        }

        [Route("{issueId:int}")]
        public IActionResult Index(string projectUrl, string versionUrl, int issueId)
        {
            var issue = Database.Issue
                .Include(i => i.Type)
                .Include(i => i.Status)
                .Include(i => i.Priority)
                .Where(i => i.Id == issueId)
                .FirstOrDefault();

            Issue parentIssue = null;
            if (issue.ParentIssueId != null)
            {
                parentIssue = Database.Issue
                    .Where(i => i.Id == issue.ParentIssueId)
                    .FirstOrDefault();
            }

            var version = Database.ProjectVersion
                .Include(v => v.Project)
                .Where(v => v.Project.Url == projectUrl && v.Url == versionUrl)
                .FirstOrDefault();

            var project = Database.Project
                .Where(p => p.Url == projectUrl)
                .FirstOrDefault();

            var assignments = Database.IssueAssignment
                .Include(i => i.Assignee)
                .Include(i => i.Role)
                .Where(i => i.IssueId == issueId)
                .OrderBy(i => i.RoleId)
                .Select(i => new IssueViewModel.AssigneeInfo
                {
                    IssueId = i.IssueId,
                    Name = $"{i.Assignee.FirstName} {i.Assignee.LastName}",
                    Id = i.AssigneeId,
                    Role = i.Role.Name
                });

            var attachments = Database.IssueAttachment
                .Where(a => a.IssueId == issueId)
                .Select(a => new IssueViewModel.AttachmentInfo
                {
                    Id = a.Id,
                    Content = a.Content,
                    Comment = a.Comment
                });

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var userRole = assignments
                .Where(a => a.Id == userId)
                .Select(a => a.Role)
                .FirstOrDefault();

            var userProjectRole = Database.ProjectAssignment
                .Include(a => a.Role)
                .Where(a => a.AssigneeId == userId && a.ProjectId == project.Id)
                .Select(a => a.Role.Name)
                .FirstOrDefault();

            return View(new IssueViewModel
            {
                UserProjectRole = userProjectRole,
                UserRole = userRole,
                ProjectUrl = projectUrl,
                ProjectName = project.Name,
                VersionUrl = versionUrl,
                VersionName = version.Name,
                Id = issue.Id,
                Subject = issue.Subject,
                Description = issue.Description,
                Type = issue.Type.Name,
                Status = issue.Status.Name,
                Priority = issue.Priority.Name,
                StartDate = issue.StartDate,
                DueDate = issue.DueDate,
                EstimatedTime = issue.EstimatedTime,
                Done = issue.Done,
                ParentIssueId = parentIssue?.Id,
                ParentIssueSubject = parentIssue?.Subject,
                Assignees = assignments,
                Attachments = attachments
            });
        }

        [Route("[action]")]
        public IActionResult Create(string projectUrl, string versionUrl)
        {
            var version = Database.ProjectVersion
                .Include(v => v.Project)
                .Where(v => v.Project.Url == projectUrl && v.Url == versionUrl)
                .FirstOrDefault();

            return View(new CreateIssueViewModel
            {
                ProjectUrl = projectUrl,
                VersionName = version.Name,
                VersionId = version.Id,
                VersionUrl = versionUrl,
                StartDate = version.StartDate,
                DueDate = version.DueDate,
                ParentIssueId = 0,
                EstimatedTime = 0,
                Done = 0,
                TypeId = 0,
                StatusId = 0,
                PriorityId = 0
            });
        }

        [HttpPost]
        [Route("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateIssueViewModel model, string projectUrl, string versionUrl)
        {
            if (ModelState.IsValid)
            {
                int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

                if (model.StartDate > model.DueDate)
                {
                    ModelState.AddModelError("DueDate", "Due date cannot be before start date.");
                    return View(model);
                }

                var version = Database.ProjectVersion
                    .Include(v => v.Project)
                    .Where(v => v.Url == versionUrl && v.Project.Url == projectUrl)
                    .FirstOrDefault();

                if (model.StartDate < version.StartDate)
                {
                    ModelState.AddModelError("StartDate", $"Start date must be after or equal {version.StartDate:dd.MM.yy}");
                    return View(model);
                }
                if (model.DueDate > version.DueDate)
                {
                    ModelState.AddModelError("DueDate", $"Due date must be before or equal {version.DueDate:dd.MM.yy}");
                    return View(model);
                }

                Issue issue = new Issue
                {
                    Description = model.Description,
                    Done = model.Done,
                    DueDate = model.DueDate,
                    EstimatedTime = model.EstimatedTime,
                    ParentIssueId = model.ParentIssueId == 0 ? (int?)null : model.ParentIssueId,
                    PriorityId = model.PriorityId,
                    StartDate = model.StartDate,
                    StatusId = model.StatusId,
                    Subject = model.Subject,
                    ProjectVersionId = version.Id,
                    TypeId = model.TypeId
                };
                Database.Issue.Add(issue);

                int roleId = Database.IssueAssignmentRole
                    .Where(r => r.Name == "Creator")
                    .Select(r => r.Id)
                    .FirstOrDefault();

                IssueAssignment assignment = new IssueAssignment
                {
                    AssigneeId = userId,
                    IssueId = issue.Id,
                    RoleId = roleId
                };
                Database.IssueAssignment.Add(assignment);

                var user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

                await Database.SaveChangesAsync();

                ProjectActivity activity = new ProjectActivity
                {
                    ProjectId = version.ProjectId,
                    AuthorId = userId,
                    Content = $"Created issue '#{issue.Id}: {issue.Subject}'"
                };
                Database.ProjectActivity.Add(activity);
                await Database.SaveChangesAsync();

                return RedirectToAction("Index", "Issue", new { projectUrl = projectUrl, versionUrl = versionUrl, issueId = issue.Id });
            }

            return View(model);
        }

        [Route("{issueId:int}/[action]")]
        public IActionResult Update(string projectUrl, string versionUrl, int issueId)
        {
            var issue = Database.Issue
                .Where(i => i.Id == issueId)
                .FirstOrDefault();

            var version = Database.ProjectVersion
                .Where(v => v.Id == issue.ProjectVersionId)
                .FirstOrDefault();

            return View(new UpdateIssueViewModel
            {
                Id = issueId,
                ProjectUrl = projectUrl,
                VersionId = version.Id,
                VersionUrl = versionUrl,
                Subject = issue.Subject,
                ParentIssueId = issue.ParentIssueId,
                Description = issue.Description,
                TypeId = issue.TypeId,
                StatusId = issue.StatusId,
                PriorityId = issue.PriorityId,
                StartDate = issue.StartDate,
                DueDate = issue.DueDate,
                EstimatedTime = issue.EstimatedTime,
                Done = issue.Done
            });
        }

        [HttpPost]
        [Route("{issueId:int}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateIssueViewModel model, string projectUrl, string versionUrl, int issueId)
        {
            if (ModelState.IsValid)
            {
                int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

                if (model.StartDate > model.DueDate)
                {
                    ModelState.AddModelError("DueDate", "Due date cannot be before start date.");
                    return View(model);
                }

                var version = Database.ProjectVersion
                    .Include(v => v.Project)
                    .Where(v => v.Url == versionUrl && v.Project.Url == projectUrl)
                    .FirstOrDefault();

                if (model.StartDate < version.StartDate)
                {
                    ModelState.AddModelError("StartDate", $"Start date must be after or equal {version.StartDate:dd.MM.yy}");
                    return View(model);
                }
                if (model.DueDate > version.DueDate)
                {
                    ModelState.AddModelError("DueDate", $"Due date must be before or euqal {version.DueDate:dd.MM.yy}");
                    return View(model);
                }

                Issue issue = Database.Issue
                    .Where(i => i.Id == issueId)
                    .FirstOrDefault();

                issue.Subject = model.Subject;
                issue.Description = model.Description;
                issue.TypeId = model.TypeId;
                issue.StatusId = model.StatusId;
                issue.PriorityId = model.PriorityId;
                issue.StartDate = model.StartDate;
                issue.DueDate = model.DueDate;
                issue.EstimatedTime = model.EstimatedTime;
                issue.Done = model.Done;
                issue.ProjectVersionId = version.Id;
                issue.ParentIssueId = model.ParentIssueId == 0 ? (int?)null : model.ParentIssueId;

                var user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

                await Database.SaveChangesAsync();

                ProjectActivity activity = new ProjectActivity
                {
                    ProjectId = version.ProjectId,
                    AuthorId = userId,
                    Content = $"Updated issue '#{issue.Id}: {issue.Subject}'"
                };
                Database.ProjectActivity.Add(activity);

                await Database.SaveChangesAsync();

                return RedirectToAction("Index", "Issue", new { projectUrl = projectUrl, versionUrl = versionUrl, issueId = issue.Id });
            }

            return View(model);
        }

        [HttpPost]
        [Route("{issueId:int}/[action]")]
        public async Task<IActionResult> Delete(string projectUrl, string versionUrl, int issueId)
        {
            var issue = Database.Issue
                .Where(i => i.Id == issueId)
                .FirstOrDefault();

            var version = Database.ProjectVersion
                .Where(v => v.Id == issue.ProjectVersionId)
                .FirstOrDefault();

            Database.Remove(issue);

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

            await Database.SaveChangesAsync();

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = version.ProjectId,
                AuthorId = userId,
                Content = $"Deleted issue '#{issue.Id}: {issue.Subject}'"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return RedirectToAction("Index", "Version", new { projectUrl = projectUrl, versionUrl = versionUrl });
        }

        [HttpPost]
        [Route("{issueId:int}/attachments/create")]
        public async Task<IActionResult> CreateAttachment([FromBody]CreateAttachmentViewModel model, [FromRoute]int issueId)
        {
            int size = model.Content.Length;

            IssueAttachment attachment = new IssueAttachment
            {
                IssueId = issueId,
                Content = model.Content,
                Comment = model.Comment
            };

            Database.IssueAttachment.Add(attachment);

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

            var issue = Database.Issue
                .Include(i => i.ProjectVersion)
                .Where(i => i.Id == issueId)
                .FirstOrDefault();

            await Database.SaveChangesAsync();

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = issue.ProjectVersion.ProjectId,
                AuthorId = userId,
                Content = $"File attached to issue '#{issue.Id}: {issue.Subject}'"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true,
                url = Url.Action("Index")
            });
        }

        [HttpPost]
        [Route("{issueId:int}/attachments/delete")]
        public async Task<IActionResult> DeleteAttachment([FromBody]DeleteAttachmentViewModel model, int issueId)
        {
            var attachment = Database.IssueAttachment
                .Where(a => a.Id == model.Id)
                .FirstOrDefault();

            Database.Remove(attachment);

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

            var issue = Database.Issue
                .Include(i => i.ProjectVersion)
                .Where(i => i.Id == issueId)
                .FirstOrDefault();
            
            await Database.SaveChangesAsync();

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = issue.ProjectVersion.ProjectId,
                AuthorId = userId,
                Content = $"File detached from issue '#{issue.Id}: {issue.Subject}'"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true
            });
        }

        [Route("{issueId:int}/[action]")]
        public IActionResult Assignments(int issueId)
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var userRole = Database.IssueAssignment
                .Include(a => a.Role)
                .Where(a => a.AssigneeId == userId)
                .Select(a => a.Role.Name)
                .FirstOrDefault();

            var userProjectRole = Database.ProjectAssignment
                .Include(a => a.Role)
                .Where(a => a.AssigneeId == userId)
                .Select(a => a.Role.Name)
                .FirstOrDefault();

            if (userRole == null || userRole == "Watcher")
            {
                if (userProjectRole != "Creator" && userProjectRole != "Manager")
                {
                    return RedirectToAction("Index");
                }
            }

            var issue = Database.Issue
                .Include(i => i.ProjectVersion)
                .Where(i => i.Id == issueId)
                .FirstOrDefault();

            var assignees = Database.IssueAssignment
                .Include(a => a.Assignee)
                .Include(a => a.Role)
                .Where(a => a.IssueId == issueId)
                .Select(a => new IssueAssignmentsViewModel.AssigneeInfo
                {
                    Id = a.AssigneeId,
                    AssignmentId = a.Id,
                    RoleId = a.RoleId,
                    Name = $"{a.Assignee.FirstName} {a.Assignee.LastName}",
                    Role = a.Role.Name
                }).OrderBy(a => a.RoleId);

            var users = Database.ProjectAssignment
                .Include(a => a.Assignee)
                .Include(a => a.Role)
                .Where(a => a.ProjectId == issue.ProjectVersion.ProjectId
                    && !assignees.Contains(new IssueAssignmentsViewModel.AssigneeInfo { Id = a.AssigneeId }))
                .Select(a => new IssueAssignmentsViewModel.UserInfo
                {
                    Id = a.AssigneeId,
                    Name = $"{a.Assignee.FirstName} {a.Assignee.LastName}"
                });

            return View(new IssueAssignmentsViewModel
            {
                IssueId = issueId,
                Assignees = assignees,
                Users = users
            });
        }

        [HttpPost]
        [Route("{issueId:int}/assignments/create")]
        public async Task<IActionResult> CreateAssignments([FromBody]CreateIssueAssignmentsViewModel model, int issueId)
        {

            foreach (int id in model.UserIdList)
            {
                var assignment = new IssueAssignment
                {
                    AssigneeId = id,
                    IssueId = issueId,
                    RoleId = model.RoleId
                };

                Database.IssueAssignment.Add(assignment);
            }

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

            var issue = Database.Issue
                .Include(i => i.ProjectVersion)
                .Where(i => i.Id == issueId)
                .FirstOrDefault();

            string roleName = Database.IssueAssignmentRole
                .Where(r => r.Id == model.RoleId)
                .Select(r => r.Name)
                .FirstOrDefault();

            await Database.SaveChangesAsync();

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = issue.ProjectVersion.ProjectId,
                AuthorId = userId,
                Content = $"{user.FirstName} {user.LastName} was assigned as {roleName} on issue '#{issue.Id}: {issue.Subject}'"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true,
                url = Url.Action("Assignments")
            });
        }

        [HttpPost]
        [Route("{issueId:int}/assignments/delete")]
        public async Task<IActionResult> DeleteAssignment([FromBody]DeleteIssueAssignmentViewModel model, int issueId)
        {
            var assignment = Database.IssueAssignment
                .Where(a => a.Id == model.Id)
                .FirstOrDefault();

            Database.Remove(assignment);

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

            var issue = Database.Issue
                .Include(i => i.ProjectVersion)
                .Where(i => i.Id == issueId)
                .FirstOrDefault();

            await Database.SaveChangesAsync();

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = issue.ProjectVersion.ProjectId,
                AuthorId = userId,
                Content = $"{user.FirstName} {user.LastName} was removed from issue '#{issue.Id}: {issue.Subject}'"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true,
                url = Url.Action("Assignments")
            });
        }

        [HttpPost]
        [Route("{issueId:int}/assignments/update")]
        public async Task<IActionResult> UpdateAssignment([FromBody]UpdateIssueAssignmentViewModel model, int issueId)
        {
            var assignment = Database.IssueAssignment
                .Where(a => a.Id == model.AssignmentId)
                .FirstOrDefault();

            assignment.RoleId = model.RoleId;

            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            var user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

            var issue = Database.Issue
                .Include(i => i.ProjectVersion)
                .Where(i => i.Id == issueId)
                .FirstOrDefault();

            string roleName = Database.IssueAssignmentRole
                .Where(r => r.Id == model.RoleId)
                .Select(r => r.Name)
                .FirstOrDefault();

            await Database.SaveChangesAsync();

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = issue.ProjectVersion.ProjectId,
                AuthorId = userId,
                Content = $"{user.FirstName} {user.LastName} was re-assigned as {roleName} on issue '#{issue.Id}: {issue.Subject}'"
            };
            Database.ProjectActivity.Add(activity);

            await Database.SaveChangesAsync();

            return Json(new
            {
                status = true,
                url = Url.Action("Assignments")
            });
        }
    }
}
