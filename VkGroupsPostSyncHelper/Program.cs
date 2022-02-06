using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VkGroupsPostSyncHelper.Logic;
using VkGroupsPostSyncHelper.DAL.SQLite;
using VkGroupsPostSyncHelper.Telegram;
using VkGroupsPostSyncHelper.VK;
using System;
using Serilog;

namespace VkGroupsPostSyncHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console(
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose, 
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                    .WriteTo.File("Logs/log-.log",
                        rollingInterval: RollingInterval.Month,
                        retainedFileCountLimit: 10,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}",
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                    .CreateLogger();
                var builder = CreateHostBuilder(args).Build();
                builder.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "App not started");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder =>
            {
                var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                //place here your real key values
                .AddJsonFile("appsettings.Local.json", false)
                .Build();
                builder.AddConfiguration(config);
            })
            .UseSerilog()
            .ConfigureServices(services =>
            {
                services.
                    AddEntityFrameworkSqlite().
                    AddDbContext<MainDbContext>(ServiceLifetime.Transient);
                services.AddHostedService<ReadPostsService>();
                services.AddHostedService<WritePostsService>();
                services.AddSingleton<VkHandler>();
                services.AddSingleton<TgHandler>();
                services.AddTransient<VKprocessService>();
                services.AddTransient<TGprocessService>();
            });
    }
}
