using AzureAdInSSR8.Components;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace AzureAdInSSR8
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Add services to the container.
            builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
               .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
               .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
               .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
               .AddInMemoryTokenCaches();

            builder.Services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();

            builder.Services.AddRazorPages();

            builder.Services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy
                options.FallbackPolicy = options.DefaultPolicy;
            });


            builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.Events.OnSignedOutCallbackRedirect = async context =>
                {
                    context.HttpContext.Response.Redirect(context.Options.SignedOutRedirectUri);
                    context.HandleResponse();
                };

            });

            builder.Services.AddScoped<MicrosoftIdentityConsentAndConditionalAccessHandler>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorPages();
            app.MapControllers();
            //app.MapBlazorHub();

            //app.MapFallbackToPage("/{param?}", "/_Host");

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            //app.UseRewriter(new RewriteOptions().Add(
            //context =>
            //{
            //    if (context.HttpContext.Request.Path == "/MicrosoftIdentity/Account/SignedOut")
            //    {
            //        context.HttpContext.Response.Redirect("/");
            //    }
            //}));

            app.Run();
        }
    }
}
