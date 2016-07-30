﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ConCode.NET.Web.Data;
using ConCode.NET.Web.Models;
using ConCode.NET.Web.Services;
using ConCode.NET.Core.Data;
using ConCode.NET.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using ConCode.NET.Core.Domain.Interfaces;

namespace ConCode.NET.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ConCode")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IConferenceDataProvider, InMemoryConferenceDataProvider>();
            services.AddTransient<ISessionService, SessionService>();
            services.AddTransient<ISpeakerService, SpeakerService>();
            services.AddTransient<ITalkService, TalkService>();
            services.AddTransient<ISponsorService, SponsorService>();
            services.AddTransient<IVenueService, VenueService>();
            services.AddTransient<IAttendeeService, AttendeeService>();

            // Preserve object references in JSON
            services.AddMvc().AddJsonOptions(x => x.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.All);

            // Require SSL
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new RequireHttpsAttribute());
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseFacebookAuthentication(new FacebookOptions()
            {
                //AppId = Configuration["Authentication:Facebook:AppId"],
                //AppSecret = Configuration["Authentication:Facebook:AppSecret"]
                AppId = "1751083965130261",
                AppSecret = "c7338a55d95b7477c87224d79a1cc457"
            });

            app.UseTwitterAuthentication(new TwitterOptions()
            {
                ConsumerKey = "iPRM2uhtpwfwPU9GK8QbXXZMW",
                ConsumerSecret = "CKhhQKPQJjplp8G2o5BrjmvFZeqdpVRYZAobHZrxTtevHIHtLU"
            });


            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
