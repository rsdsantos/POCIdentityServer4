using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IdentityModel.Tokens.Jwt;

namespace WebClientMVC
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
            services.AddHttpClient();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Define que as claims default (sub, idp, etc) não serão renomeadas.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            #region Implicit Flow
            //services.AddAuthentication(opt =>
            //{
            //    opt.DefaultScheme = "Cookies";
            //    opt.DefaultChallengeScheme = "oidc";
            //})
            //.AddCookie("Cookies")
            //.AddOpenIdConnect("oidc", opt =>
            //{
            //    opt.Authority = "http://localhost:5000";
            //    opt.RequireHttpsMetadata = false;

            //    opt.ClientId = "mvc";
            //    opt.SaveTokens = true;
            //});
            #endregion
            #region Hybrid Flow
            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = "Cookies";
                opt.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", opt =>
            {
                opt.SignInScheme = "Cookies";

                opt.Authority = "http://localhost:5000";
                opt.RequireHttpsMetadata = false;

                opt.ClientId = "mvc";
                opt.ClientSecret = "secret";
                opt.ResponseType = "code id_token";

                opt.SaveTokens = true;
                opt.GetClaimsFromUserInfoEndpoint = true;

                opt.Scope.Add("api1");
                opt.Scope.Add("offline_access");
                opt.ClaimActions.MapJsonKey("website", "website"); // Mapeamento explicito da claim
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
