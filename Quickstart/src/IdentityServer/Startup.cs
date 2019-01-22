using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace IdentityServer
{
    public class Startup
    {      
        public IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment environment)
        {
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configuração para EF Core
            const string connectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;database=IdentityServer4.Quickstart.EntityFramework-2.0.0;trusted_connection=yes;";
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // Habilita o UI do OIDC em MVC
            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            // Configura o IdentityServer para utilizar o EntityFramework.
            var builder = services.AddIdentityServer()
                // Recupera os dados de usuários da memória
                .AddTestUsers(Config.GetUsers())
                
                // Recupera dados de configuração do EF (clients, resources)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })

                // Recupera dados operacionais do EF (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // Habilita limpeza automática do token (opcional)
                    options.EnableTokenCleanup = true;
                });


            // Configura o IdentityServer em memória
            //var builder = services.AddIdentityServer()
            //    .AddInMemoryIdentityResources(Config.GetIdentityResources())
            //    .AddInMemoryApiResources(Config.GetApis())
            //    .AddInMemoryClients(Config.GetClients())
            //    .AddTestUsers(Config.GetUsers());

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Popula a base de dados através do migration. Este método deve ser utilizado uma vez e apenas na POC.
            // A partir da primeira carga este método deve ser comentado para que a base não seja gerada a cada execução.
            //InitializeDatabase(app);

            app.UseStaticFiles();

            app.UseIdentityServer();

            // Habilita o UI do IdentityServer4 em MVC
            app.UseMvcWithDefaultRoute();
        }

        /// <summary>
        /// Esse método executa os scripts de migration do IS4 para esta POC.
        /// Para o ambiente de Produçao é necessário definir uma estratégia de criação da base de dados.
        /// O diretório ~\Quickstart\src\IdentityServer\Data\Migrations\IdentityServer possui os scripts.
        /// </summary>
        /// <param name="app"></param>
        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApis())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}