using Microsoft.EntityFrameworkCore;
using restapp.Dal;

namespace restapp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            //added - to create or use sessions we added this
            builder.Services.AddSession();

            //added 
            //to get connection string from appsettings.json
            string conStr = builder.Configuration.GetConnectionString("SQLServerConnection");
            builder.Services.AddDbContext<RestContext>(options => options.UseSqlServer(conStr));

            var app = builder.Build();
            //added
            app.UseSession();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
