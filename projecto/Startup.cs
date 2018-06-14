using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Projecto.Infrastructure.Attributes;
using Projecto.Models;

namespace Projecto
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
                {
                    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(name => "{0} cannot be empty");
                    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(name => "{0} is invalid.");
                    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(name => "{0} cannot be empty.");
                });

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseMySql(Configuration["ConnectionStrings:MySQL"]);
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                })
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authorization:Google:Id"];
                    options.ClientSecret = Configuration["Authorization:Google:Secret"];
                    options.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/account/login/google");
                })
                .AddFacebook(options =>
                {
                    options.ClientId = Configuration["Authorization:Facebook:Id"];
                    options.ClientSecret = Configuration["Authorization:Facebook:Secret"];
                    options.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/account/login/facebook");
                })
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = Configuration["Authorization:Microsoft:Id"];
                    options.ClientSecret = Configuration["Authorization:Microsoft:Secret"];
                    options.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/account/login/microsoft");
                });

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStatusCodePages();
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "", template: "{controller=Home}/{action=Index}");
            });
        }
    }
}
