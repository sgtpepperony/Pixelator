namespace webattempt.wwwroot

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
        });

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = long.MaxValue;
        });

        services.AddAntiforgery(options =>
        {
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        services.AddHttpsRedirection(options =>
        {
            options.HttpsPort = 443; // Specify the HTTPS port
        });

        app.use(
  session({
        secret: process.env.SECRET_KEY,
    resave: false,
    saveUninitialized: true,
    cookie:
            {
            sameSite: "none",
      secure: "auto",
    },
  })
);

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseMvc();
    }
}