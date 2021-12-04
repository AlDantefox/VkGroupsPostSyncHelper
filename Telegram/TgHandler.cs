using Microsoft.Extensions.Configuration;
using VkGroupsPostSyncHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Threading;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.Enums;

namespace VkGroupsPostSyncHelper.Telegram
{
    public class TgHandler
    {
        private IConfiguration _config;

        public TgHandler(IConfiguration config)
        {
            _config = config;
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
                return new ChatId(_config.GetSection("Telegram")[nameof(ChatId)]);
            }
        }

        public async Task<DateTime?> Post(TelegramPost post, CancellationToken cancellationToken)
        {
            var botClient = new TelegramBotClient(this.BotKey);
            if (post.IsOnlyText)
            {
                Message result = await botClient.SendTextMessageAsync(
                    this.ChatId, 
                    post.Text,
                    cancellationToken: cancellationToken);
                return result?.Date;
            }
            else if (post.UrlImageList.Count == 1)
            {
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
                Message[] messages = await botClient.SendMediaGroupAsync(
                    this.ChatId,
                    post.UrlImageList.Select(url => new InputMediaPhoto(url)),
                    cancellationToken: cancellationToken
                );
                Message result = await botClient.SendTextMessageAsync(
                    this.ChatId,
                    post.Text,
                    replyToMessageId: messages[0].MessageId,
                    cancellationToken: cancellationToken);
                return result?.Date;
            }
        }
    }
}
