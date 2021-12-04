using Microsoft.Extensions.Hosting;
using NCrontab;
using VkGroupsPostSyncHelper.Logic;
using VkGroupsPostSyncHelper.VK;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VkGroupsPostSyncHelper
{
    public class ReadPostsService : IHostedService
    {
        ILogger<ReadPostsService> _logger;
        IHostApplicationLifetime _app;
        VkHandler _vkHandler;
        VKprocessService _vkService;

        CrontabSchedule _shedule;
        DateTime _sheduledTime;
        const string _shedulePattern_ = "*/60 * * * *";
        public ReadPostsService(IHostApplicationLifetime app, VkHandler vkHandler, VKprocessService vkService, ILogger<ReadPostsService> logger)
        {
            _app = app;
            _vkHandler = vkHandler;
            _vkService = vkService;
            _shedule = CrontabSchedule.Parse(_shedulePattern_, new CrontabSchedule.ParseOptions() { IncludingSeconds = false });
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                _logger.LogInformation("Start ReadPostsService");
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(Math.Max(0, (int)(_sheduledTime.Subtract(DateTime.Now).TotalMilliseconds)), cancellationToken);
                    try
                    {
                        _logger.LogDebug("Start loading new posts");
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
                        _logger.LogDebug($"Load complete. Total added: {totalAdded}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error on reading new VK posts");
                        _app.StopApplication();
                    }
                    _sheduledTime = _shedule.GetNextOccurrence(DateTime.Now);
                    _logger.LogDebug($"Next read posts try at {_sheduledTime}");
                }

            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("End work ReadPostsService");
            return Task.CompletedTask;
        }
    }
}
