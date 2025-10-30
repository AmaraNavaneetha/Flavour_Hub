using Microsoft.EntityFrameworkCore;
using restapp.Dal;
using restapp.Services; // 👈 Add this using directive

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

            // ⭐ CRITICAL FIX: Register DBServices
            // AddScoped is appropriate for services that rely on DbContext (which is also Scoped).
            builder.Services.AddScoped<DBServices>();

            var app = builder.Build();

            //added
            app.UseSession();

            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}