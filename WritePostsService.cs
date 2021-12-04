using Microsoft.Extensions.Hosting;
using NCrontab;
using VkGroupsPostSyncHelper.Logic;
using VkGroupsPostSyncHelper.Telegram;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VkGroupsPostSyncHelper
{
    public class WritePostsService: IHostedService
    {
        ILogger<WritePostsService> _logger;
        IHostApplicationLifetime _app;
        TgHandler _tgHandler;
        VKprocessService _vkService;
        TGprocessService _tgService;

        CrontabSchedule _shedule;
        DateTime _sheduledTime;
        const string _shedulePattern_ = "*/1 * * * *";
        public WritePostsService(IHostApplicationLifetime app, TgHandler tgHandler, VKprocessService vkService, TGprocessService tgService, ILogger<WritePostsService> logger)
        {
            _app = app;
            _tgHandler = tgHandler;
            _vkService = vkService;
            _tgService = tgService;
            _shedule = CrontabSchedule.Parse(_shedulePattern_, new CrontabSchedule.ParseOptions() { IncludingSeconds = false });
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                _logger.LogInformation("Start WritePostsService");
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(Math.Max(0, (int)(_sheduledTime.Subtract(DateTime.Now).TotalMilliseconds)), cancellationToken);
                    try
                    {
                        _logger.LogDebug("Start posting to TG");
                        var newPost = _tgService.GetLastReadedPost();
                        if (newPost != null)
                        {
                            _logger.LogDebug($"posting {newPost.Id} to Tg");
                            var postDate = await _tgHandler.Post(newPost, cancellationToken);
                            if (postDate.HasValue)
                            {
                                _logger.LogInformation($"posting {newPost.Id} success");
                                await _vkService.MarkPostSended(newPost.Id, postDate.Value);
                            }
                            else
                            {
                                _logger.LogDebug($"posting {newPost.Id} failed");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error on saving new TG post");
                        _app.StopApplication();
                    }
                    _sheduledTime = _shedule.GetNextOccurrence(DateTime.Now);
                    _logger.LogDebug($"Next post try at {_sheduledTime}");
                }

            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("End work WritePostsService");
            return Task.CompletedTask;
        }
    }
}
