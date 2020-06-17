using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using TOH.Network.Packets;
using TOH.Server.PacketHandlers;

namespace TOH.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var server = new GameServer(host);

            server.AddPacketHandler<PingPacket, PingPacketHandler>();
            server.AddPacketHandler<FindBattlePacket, FindBattlePacketHandler>();
            server.AddPacketHandler<SetBattleUnitsPacket, SetBattleUnitsPacketHandler>();
            server.AddPacketHandler<BattleTurnCommandPacket, BattleTurnCommandPacketHandler>();

            await server.StartAsync();

            host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(server.StopAsync);

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hostsettings.json", optional: false)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseConfiguration(configuration);
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
