using Microsoft.Extensions.Hosting;
using NCrontab;
using VkGroupsPostSyncHelper.Logic;
using VkGroupsPostSyncHelper.Telegram;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace VkGroupsPostSyncHelper
{
    public class WritePostsService: IHostedService
    {
        IHostApplicationLifetime _app;
        TgHandler _tgHandler;
        VKprocessService _vkService;
        TGprocessService _tgService;

        CrontabSchedule _shedule;
        DateTime _sheduledTime;
        const string _shedulePattern_ = "*/1 * * * *";
        public WritePostsService(IHostApplicationLifetime app, TgHandler tgHandler, VKprocessService vkService, TGprocessService tgService)
        {
            _app = app;
            _tgHandler = tgHandler;
            _vkService = vkService;
            _tgService = tgService;
            _shedule = CrontabSchedule.Parse(_shedulePattern_, new CrontabSchedule.ParseOptions() { IncludingSeconds = false });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                Console.WriteLine("Start WritePostsService");
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(Math.Max(0, (int)(_sheduledTime.Subtract(DateTime.Now).TotalMilliseconds)), cancellationToken);
                    try
                    {
                        Console.WriteLine("Start posting to TG");
                        var newPost = _tgService.GetLastReadedPost();
                        if (newPost != null)
                        {
                            Console.WriteLine($"posting {newPost.Id} to Tg");
                            var postDate = await _tgHandler.Post(newPost, cancellationToken);
                            if (postDate.HasValue)
                            {
                                Console.WriteLine($"posting {newPost.Id} success");
                                await _vkService.MarkPostSended(newPost.Id, postDate.Value);
                            }
                            else
                            {
                                Console.WriteLine($"posting {newPost.Id} failed");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error in WritePostsService: { ex.Message }");
                        _app.StopApplication();
                    }
                    _sheduledTime = _shedule.GetNextOccurrence(DateTime.Now);
                    Console.WriteLine($"Next post try at {_sheduledTime}");
                }

            }, cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("End work WritePostsService");
            return Task.CompletedTask;
        }
    }
}
