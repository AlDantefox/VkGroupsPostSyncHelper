using Microsoft.Extensions.Hosting;
using NCrontab;
using VkGroupsPostSyncHelper.Logic;
using VkGroupsPostSyncHelper.VK;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VkGroupsPostSyncHelper
{
    public class ReadPostsService : IHostedService
    {
        IHostApplicationLifetime _app;
        VkHandler _vkHandler;
        VKprocessService _vkService;

        CrontabSchedule _shedule;
        DateTime _sheduledTime;
        const string _shedulePattern_ = "*/60 * * * *";
        public ReadPostsService(IHostApplicationLifetime app, VkHandler vkHandler, VKprocessService vkService)
        {
            _app = app;
            _vkHandler = vkHandler;
            _vkService = vkService;
            _shedule = CrontabSchedule.Parse(_shedulePattern_, new CrontabSchedule.ParseOptions() { IncludingSeconds = false });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                Console.WriteLine("Start ReadPostsService");
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(Math.Max(0, (int)(_sheduledTime.Subtract(DateTime.Now).TotalMilliseconds)), cancellationToken);
                    try
                    {
                        Console.WriteLine("Start loading new posts");
                        int addedCount = 0, totalAdded = 0;
                        ulong loaded = 0;
                        do
                        {
                            var posts = await _vkHandler.GetPosts(loaded);
                            loaded += Convert.ToUInt64(posts.Count);
                            addedCount = await _vkService.SaveIfNotExists(posts);
                            totalAdded += addedCount;
                        }
                        while (addedCount > 0);
                        Console.WriteLine($"Load complete. Total added: {totalAdded}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in ReadPostsService: { ex.Message }");
                        _app.StopApplication();
                    }
                    _sheduledTime = _shedule.GetNextOccurrence(DateTime.Now);
                    Console.WriteLine($"Next read posts try at {_sheduledTime}");
                }

            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("End work ReadPostsService");
            return Task.CompletedTask;
        }
    }
}
