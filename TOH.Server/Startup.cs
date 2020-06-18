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
using System.Reflection;
using TOH.Network.Abstractions;
using TOH.Network.Common;
using TOH.Network.Packets;
using TOH.Network.Server;
using TOH.Server.Data;
using TOH.Server.PacketHandlers;
using TOH.Server.Services;
using TOH.Server.Systems;

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
            services.Configure<ServerOptions>(Configuration.GetSection("ServerOptions"));

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

            services.AddTransient<RandomService, RandomService>();
            services.AddTransient<PlayerManager, PlayerManager>();
            services.AddTransient<PlayerService, PlayerService>();

            services.AddTransient<TimerService, TimerService>();
            services.AddTransient<IPacketConverter, JsonPacketConverter>();
            services.AddSingleton<ConnectionManager, ConnectionManager>();
            services.AddSingleton<PVPBattleLobbyService, PVPBattleLobbyService>();
            services.AddSingleton<BattleSystem, BattleSystem>();

            services.AddTransient<IPacketHandler<PingPacket>, PingPacketHandler>();
            services.AddTransient<IPacketHandler<FindBattlePacket>, FindBattlePacketHandler>();
            services.AddTransient<IPacketHandler<SetBattleUnitsPacket>, SetBattleUnitsPacketHandler>();
            services.AddTransient<IPacketHandler<BattleTurnCommandPacket>, BattleTurnCommandPacketHandler>();
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
                    await context.Response.WriteAsync("TOH.Server");
                });
            });
        }
    }
}
