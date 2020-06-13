using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.Server;
using System;
using System.Linq;
using System.Reflection;
using TOH.Server.Data;
using TOH.Server.Services;

namespace TOH.Server
{
    class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<GameDbContext>(options =>
            {
                options.UseLazyLoadingProxies();
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly(migrationsAssembly));
            });

            services.AddCodeFirstGrpc(config =>
            {
                config.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
            });

            services.AddDataProtection()
                .SetApplicationName("TOH.Server")
                .PersistKeysToDbContext<GameDbContext>()
                .SetDefaultKeyLifetime(TimeSpan.FromDays(1825));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<PlayerService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Server Pong");
                    var req = context.Request.Query.Keys.FirstOrDefault();
                    await context.Response.WriteAsync($"REQUEST > {req}");
                });
            });
        }
    }
}
