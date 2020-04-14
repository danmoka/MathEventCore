using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using MathEvent.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MathEvent.Models;
using MathEvent.Helpers;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Components;

namespace MathEvent
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();


            services.AddServerSideBlazor();
            //services.AddHttpClient();
            //// Server Side Blazor doesn't register HttpClient by default
            //if (!services.Any(x => x.ServiceType == typeof(HttpClient)))
            //{
            //    // Setup HttpClient for server side in a client side compatible fashion
            //    services.AddScoped<HttpClient>(s =>
            //    {
            //        // Creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
            //        var uriHelper = s.GetRequiredService<NavigationManager>();
            //        return new HttpClient
            //        {
            //            BaseAddress = new Uri(uriHelper.BaseUri)
            //        };
            //    });
            //}


            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            UserDataPathWorker.Init(env);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
            });
        }
    }
}
