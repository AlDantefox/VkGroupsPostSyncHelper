using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using VkGroupsPostSyncHelper.Models;

namespace VkGroupsPostSyncHelper.Telegram
{
    public class TgHandler
    {
        private IConfiguration _config;
        ILogger<TgHandler> _logger;

        public TgHandler(IConfiguration config, ILogger<TgHandler> logger)
        {
            _config = config;
            _logger = logger;
        }

        private String BotKey
        {
            get
            {
                return _config.GetSection("Telegram")[nameof(BotKey)];
            }
        }

        private ChatId ChatId
        {
            get
            {
                var tkey = _config.GetSection("Telegram")[nameof(ChatId)];
                if (tkey == null)
                {
                    return null;
                }
                return new ChatId(tkey);
            }
        }

        public async Task<DateTime?> Post(TelegramPost post, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug($"Try Authorize in Telegram");
                var botClient = new TelegramBotClient(this.BotKey);
                if (post.IsOnlyText)
                {
                    _logger.LogDebug($"Try send plain text message for Id: {post.Id}");
                    Message result = await botClient.SendTextMessageAsync(
                        this.ChatId,
                        post.Text,
                        cancellationToken: cancellationToken);
                    return result?.Date;
                }
                else if (post.UrlImageList.Count == 1)
                {
                    _logger.LogDebug($"Try send text message for Id: {post.Id} with one image");
                    Message result = await botClient.SendPhotoAsync(
                        this.ChatId,
                        new InputOnlineFile(post.UrlImageList[0]),
                        post.Text,
                        ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    return result?.Date;
                }
                else
                {
                    _logger.LogDebug($"Try send photo group message for Id: {post.Id}");
                    Message[] messages = await botClient.SendMediaGroupAsync(
                        this.ChatId,
                        post.UrlImageList.Select(url => new InputMediaPhoto(url)),
                        cancellationToken: cancellationToken
                    );
                    _logger.LogDebug($"Try send plain text message with photo group reply for Id: {post.Id}");
                    Message result = await botClient.SendTextMessageAsync(
                        this.ChatId,
                        post.Text,
                        replyToMessageId: messages[0].MessageId,
                        cancellationToken: cancellationToken);
                    return result?.Date;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in sending posts to Tg. BotKey: {this.BotKey} ChatId: {this.ChatId.Username}");
                return null;
            }
        }
    }
}
