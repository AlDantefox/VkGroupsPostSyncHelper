using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using VkGroupsPostSyncHelper.DAL.SQLite;
using VkGroupsPostSyncHelper.Models;

namespace VkGroupsPostSyncHelper.Logic
{
    public class TGprocessService
    {
        MainDbContext _context;
        private ILogger<TGprocessService> _logger;
        public TGprocessService(MainDbContext context, ILogger<TGprocessService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Return last post (within publish of no more than 24 hours)
        /// </summary>
        public TelegramPost GetLastUnsyncPostForLastDay()
        {
            var dayAgo = DateTime.UtcNow.AddHours(-24);
            var post = _context.VkGroupPosts.Where(p => p.TransferDate == null && p.PostDate.HasValue
                                                    && p.PostDate.Value > dayAgo)
                                            .OrderByDescending(p => p.PostDate)
                                            .FirstOrDefault();
            if (post != null)
            {
                var attachments = _context.VkPostImages.Where(i => i.PostId == post.VkId).ToList();
                _logger.LogDebug($"Last unsync post: {post.VkId} from {post.PostDate} with {attachments.Count} attachments");
                if (attachments.Count > 0)
                {
                    return new TelegramPost(post, attachments);
                }
                else return new TelegramPost(post);
            }

            return null;
        }

        /// <summary>
        /// Return the most older post with limit
        /// </summary>
        /// <param name="oldBorder">Only posts newer than this</param>
        public TelegramPost GetOldestUnsyncPost(DateTime? oldBorder)
        {
            var post = _context.VkGroupPosts.Where(p => p.TransferDate == null && p.PostDate.HasValue
                                                    && (!oldBorder.HasValue || p.PostDate > oldBorder))
                                            .OrderBy(p => p.PostDate)
                                            .FirstOrDefault();
            if (post != null)
            {
                var attachments = _context.VkPostImages.Where(i => i.PostId == post.VkId).ToList();
                _logger.LogDebug($"Last unsync post: {post.VkId} from {post.PostDate} with {attachments.Count} attachments");
                if (attachments.Count > 0)
                {
                    return new TelegramPost(post, attachments);
                }
                else return new TelegramPost(post);
            }

            return null;
        }
    }
}
