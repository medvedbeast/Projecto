using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Projecto.Data;
using Projecto.Models;
using Projecto.ViewModels;

namespace Projecto.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private DatabaseContext Database { get; set; }
        private IConfiguration Configuration { get; set; }

        public AccountController(DatabaseContext context, IConfiguration configuration)
        {
            Database = context;
            Configuration = configuration;
        }

        [Route("[controller]")]
        public IActionResult Index()
        {
            int currentUserId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            return Index(currentUserId);
        }

        [Authorize]
        [Route("[controller]/{id:int}")]
        public IActionResult Index(int id)
        {
            int currentUserId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            bool isSelf = (id == currentUserId);

            var user = Database.User
                .Include(u => u.Group)
                .Where(u => u.Id == id)
                .FirstOrDefault();
            if (user == null)
            {
                return StatusCode(404);
            }

            int openProjects = Database.ProjectAssignment
                .Include(a => a.Project)
                .Where(a => a.AssigneeId == id)
                .Count();

            int openIssues = Database.IssueAssignment
                .Include(a => a.Issue)
                .ThenInclude(i => i.Status)
                .Where(a => a.Issue.Status.Name == "New" || a.Issue.Status.Name == "In Progress" || a.Issue.Status.Name == "Feedback")
                .Count();

            var activities = Database.ProjectActivity
                .Include(a => a.Project)
                .Where(a => a.AuthorId == id)
                .OrderByDescending(a => a.Time)
                .Select(a => new AccountViewModel.ActivityInfo
                {
                    Time = a.Time,
                    ProjectName = a.Project.Name,
                    ProjectUrl = a.Project.Url,
                    Content = a.Content
                }).Take(15);


            return View(new AccountViewModel
            {
                IsSelf = isSelf,
                IsPasswordSet = user.Password != null,
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Group = user.Group.Name,
                RegisteredOn = user.RegisteredOn,
                LastSeenOn = user.LastSeenOn,
                OpenProjects = openProjects,
                OpenIssues = openIssues,
                Activities = activities
            });
        }

        [AllowAnonymous]
        public ViewResult Login(string returnUrl)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl ?? Url.Action("Index", "Home")
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                HashAlgorithm algorithm = SHA256.Create();
                string password = Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.ASCII.GetBytes(model.Password)));
                User user = await Database.User
                    .Include(u => u.Group)
                    .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == password);

                if (user != null)
                {
                    await Authenticate(user);

                    return Redirect(model.ReturnUrl);

                }
                ModelState.AddModelError("", "You have entered invalid login or password.");
            }
            return View(model);

        }

        [AllowAnonymous]
        public ViewResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                HashAlgorithm algorithm = SHA256.Create();
                string password = Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.ASCII.GetBytes(model.Password)));
                User user = await Database.User.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == password);
                if (user == null)
                {
                    int defaultGroupId = 2;
                    user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Password = password,
                        GroupId = defaultGroupId
                    };
                    Database.User.Add(user);

                    await Database.SaveChangesAsync();

                    user.Group = Database.UserGroup.Where(g => g.Id == defaultGroupId).FirstOrDefault();
                    await Authenticate(user);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "You have entered incorrect email or password");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Login");
        }

        [HttpPost]
        public JsonResult Invite([FromBody]InviteViewModel model)
        {
            SmtpClient client = new SmtpClient(Configuration["Smtp:Host"], int.Parse(Configuration["Smtp:Port"]));
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(Configuration["Smtp:User"], Configuration["Smtp:Password"]);

            MailMessage message = new MailMessage();
            message.From = new MailAddress("invite@projec.to");
            message.To.Add(new MailAddress(model.Email));

            StreamReader reader = new StreamReader("Emails/invitation.html");
            string letter = reader.ReadToEnd();
            reader.Dispose();
            reader.Close();
            message.Body = letter;
            message.IsBodyHtml = true;

            message.Subject = "You are the chosen one";

            client.Send(message);

            return Json(new
            {
                status = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            IEnumerable<Project> projects = Database.ProjectAssignment
                .Include(a => a.Role)
                .Include(a => a.Project)
                .Where(a => a.AssigneeId == userId && a.Role.Name == "Creator")
                .Select(a => a.Project);
            Database.RemoveRange(projects);

            User user = Database.User
                .Where(u => u.Id == userId)
                .FirstOrDefault();
            Database.Remove(user);

            await Database.SaveChangesAsync();

            await HttpContext.SignOutAsync();

            return RedirectToAction("Login");
        }

        public IActionResult Update()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

            User user = Database.User
                .Where(u => u.Id == userId)
                .FirstOrDefault();

            return View(new UpdateAccountViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.NewPassword != "" || model.NewPasswordConfirmation != "")
                {
                    if (model.NewPassword != model.NewPasswordConfirmation)
                    {
                        ModelState.AddModelError("NewPasswordConfirmation", "Passwords do not match.");
                        return View(model);
                    }
                }

                int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);

                User user = Database.User
                    .Where(u => u.Id == userId)
                    .FirstOrDefault();

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;

                if (model.NewPassword != null)
                {
                    HashAlgorithm algorithm = SHA256.Create();
                    string password = Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.ASCII.GetBytes(model.NewPassword)));
                    user.Password = password;
                }

                await Database.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [AllowAnonymous]
        [Route("[controller]/login/external")]
        public IActionResult ExternalLogin(string provider)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("HandleExternalLogin")
            };

            return new ChallengeResult(provider, properties);
        }

        [Route("[controller]/login/external/handle")]
        public async Task<IActionResult> HandleExternalLogin()
        {
            string email = User.Claims
                .Where(c => c.Type.Contains("emailaddress"))
                .Select(c => c.Value)
                .FirstOrDefault();

            var user = Database.User
                .Include(u => u.Group)
                .Where(u => u.Email == email)
                .FirstOrDefault();

            if (user == null)
            {
                string firstName = User.Claims
                   .Where(c => c.Type.Contains("givenname"))
                   .Select(c => c.Value)
                   .FirstOrDefault();

                string lastName = User.Claims
                    .Where(c => c.Type.Contains("surname"))
                    .Select(c => c.Value)
                    .FirstOrDefault();

                user = new User
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    GroupId = 2
                };
                Database.User.Add(user);
                await Database.SaveChangesAsync();
                user.Group = Database.UserGroup
                    .Where(g => g.Id == 2)
                    .FirstOrDefault();
            }

            await Authenticate(user);

            return RedirectToAction("Index", "Home");
        }

        /*********************************************************************************************/

        private async Task Authenticate(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Group.Name),
                new Claim("UserId", user.Id.ToString())
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

            user = Database.User
                .Where(u => u.Id == user.Id)
                .FirstOrDefault();
            user.LastSeenOn = DateTime.Today;
            await Database.SaveChangesAsync();
        }
    }
}
