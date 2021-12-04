using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VkGroupsPostSyncHelper.Logic;
using VkGroupsPostSyncHelper.SQLite;
using VkGroupsPostSyncHelper.Telegram;
using VkGroupsPostSyncHelper.VK;
using System;

namespace VkGroupsPostSyncHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var builder = CreateHostBuilder(args).Build();
                builder.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder => {
                var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                //place here your real key values
                .AddJsonFile("appsettings.Local.json", false)
                .Build();

                builder.AddConfiguration(config);
            })
            .ConfigureServices(services => {
                //services.Configuration;
                services.
                    AddEntityFrameworkSqlite().
                    AddDbContext<MainDbContext>();
                services.AddHostedService<ReadPostsService>();
                services.AddHostedService<WritePostsService>();
                services.AddSingleton<VkHandler>();
                services.AddSingleton<TgHandler>();
                services.AddTransient<VKprocessService>();
                services.AddTransient<TGprocessService>();
            });
    }
}
