using Microsoft.Extensions.Hosting;
using NCrontab;
using VkGroupsPostSyncHelper.Logic;
using VkGroupsPostSyncHelper.Telegram;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace VkGroupsPostSyncHelper
{
    public class WritePostsService: IHostedService
    {
        private ILogger<WritePostsService> _logger;
        private IHostApplicationLifetime _app;
        private IConfiguration _config;

        private TgHandler _tgHandler;
        private VKprocessService _vkService;
        private TGprocessService _tgService;

        private CrontabSchedule _shedulePostNew;
        private DateTime _shedulePostNewTime;

        private bool _postOnlyNew = false;
        private DateTime? _oldDateTimeAfterUTC = null;

        private CrontabSchedule _shedulePostOld;
        private DateTime _shedulePostOldTime;
        public WritePostsService(IHostApplicationLifetime app, TgHandler tgHandler, VKprocessService vkService, TGprocessService tgService, ILogger<WritePostsService> logger, IConfiguration config)
        {
            _app = app;
            _tgHandler = tgHandler;
            _vkService = vkService;
            _tgService = tgService;
            _logger = logger;
            _config = config;

            var shedulePatternNew = _config.GetSection("Telegram")["PostNewestCrontab"];
            _shedulePostNew = CrontabSchedule.Parse(shedulePatternNew, new CrontabSchedule.ParseOptions() { IncludingSeconds = false });
            UpdateSheduleNew();

            var postOnlyNew = _config.GetSection("Telegram")["PostOnlyNew"];
            if (Boolean.TryParse(postOnlyNew, out var result))
            {
                _postOnlyNew = result;
            }
            else
            {
                _postOnlyNew = false;
            }

            if (!_postOnlyNew)
            {
                var utcDateTimeAfter = _config.GetSection("Telegram")["UTCDateTimeAfter"];
                if (DateTime.TryParseExact(utcDateTimeAfter,
                    "dd.MM.yyyy HH:mm",
                    System.Globalization.CultureInfo.GetCultureInfo("ru-RU"),
                    System.Globalization.DateTimeStyles.None | System.Globalization.DateTimeStyles.AssumeUniversal,
                    out var parsed))
                {
                    _oldDateTimeAfterUTC = parsed;
                }
                else
                {
                    _oldDateTimeAfterUTC = null;
                }

                var shedulePatternOld = _config.GetSection("Telegram")["PostOldestCrontab"];
                _shedulePostOld = CrontabSchedule.Parse(shedulePatternOld, new CrontabSchedule.ParseOptions() { IncludingSeconds = false });
                UpdateSheduleOld();
            }
            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                _logger.LogInformation("Start WritePostsService for New Messages");
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(Math.Max(0, (int)(_shedulePostNewTime.Subtract(DateTime.Now).TotalMilliseconds)), cancellationToken);
                    try
                    {
                        _logger.LogDebug("Start search new post for TG");
                        var newPost = _tgService.GetLastUnsyncPostForLastDay();
                        if (newPost != null)
                        {
                            _logger.LogDebug($"posting {newPost.Id} from {newPost.PostDate?.ToString("yyyy-MM-dd HH:mm:ss")} to Tg");
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
                        _logger.LogError(ex, $"Error on processing new TG post");
                        _app.StopApplication();
                    }
                    UpdateSheduleNew();
                }

            }, cancellationToken);

            if (!_postOnlyNew)
            {
                Task.Run(async () =>
                {
                    _logger.LogInformation("Start WritePostsService for Old Messages");
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(Math.Max(0, (int)(_shedulePostOldTime.Subtract(DateTime.Now).TotalMilliseconds)), cancellationToken);
                        try
                        {
                            _logger.LogDebug("Start search old post for TG");
                            var oldPost = _tgService.GetOldestUnsyncPost(_oldDateTimeAfterUTC);
                            if (oldPost != null)
                            {
                                _logger.LogDebug($"posting {oldPost.Id} from {oldPost.PostDate?.ToString("yyyy-MM-dd HH:mm:ss")} to Tg");
                                var postDate = await _tgHandler.Post(oldPost, cancellationToken);
                                if (postDate.HasValue)
                                {
                                    _logger.LogInformation($"posting {oldPost.Id} success");
                                    await _vkService.MarkPostSended(oldPost.Id, postDate.Value);
                                }
                                else
                                {
                                    _logger.LogDebug($"posting {oldPost.Id} failed");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error on processing old TG post");
                            _app.StopApplication();
                        }
                        UpdateSheduleOld();
                    }

                }, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("End work WritePostsService");
            return Task.CompletedTask;
        }

        private void UpdateSheduleNew()
        {
            _shedulePostNewTime = _shedulePostNew.GetNextOccurrence(DateTime.Now);
            _logger.LogDebug($"Next write new post try at {_shedulePostNewTime.ToString("yyyy-MM-dd HH:mm:ss")}");
        }

        private void UpdateSheduleOld()
        {
            if (!_postOnlyNew)
            {
                _shedulePostOldTime = _shedulePostOld.GetNextOccurrence(DateTime.Now);
                _logger.LogDebug($"Next write old post try at {_shedulePostOldTime.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
        }
    }
}
