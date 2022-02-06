using Microsoft.Extensions.Hosting;
using NCrontab;
using VkGroupsPostSyncHelper.Logic;
using VkGroupsPostSyncHelper.VK;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace VkGroupsPostSyncHelper
{
    public class ReadPostsService : IHostedService
    {
        private ILogger<ReadPostsService> _logger;
        private IHostApplicationLifetime _app;
        private IConfiguration _config;

        private VkHandler _vkHandler;
        private VKprocessService _vkService;

        private CrontabSchedule _shedule;
        private DateTime _sheduledTime;
        public ReadPostsService(IHostApplicationLifetime app, VkHandler vkHandler, VKprocessService vkService, ILogger<ReadPostsService> logger, IConfiguration config)
        {
            _app = app;
            _vkHandler = vkHandler;
            _vkService = vkService;
            _logger = logger;
            _config = config;

            var shedulePattern = _config.GetSection("VK")["CheckNewPostsCrontab"];
            _shedule = CrontabSchedule.Parse(shedulePattern, new CrontabSchedule.ParseOptions() { IncludingSeconds = false });
            UpdateShedule();
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
                            if (posts == null)
                            {
                                _logger.LogDebug($"Load failed. Break current try");
                                break;
                            }
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
                    UpdateShedule();                    
                }

            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("End work ReadPostsService");
            return Task.CompletedTask;
        }

        private void UpdateShedule()
        {
            _sheduledTime = _shedule.GetNextOccurrence(DateTime.Now);
            _logger.LogDebug($"Next read posts try at {_sheduledTime.ToString("yyyy-MM-dd HH:mm:ss")}");
        }
    }
}
