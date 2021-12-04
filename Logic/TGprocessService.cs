using VkGroupsPostSyncHelper.Models;
using VkGroupsPostSyncHelper.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkGroupsPostSyncHelper.Logic
{
    public class TGprocessService
    {
        MainDbContext _context;
        public TGprocessService(MainDbContext context)
        {
            _context = context;
        }

        public TelegramPost GetLastReadedPost()
        {
            var post = _context.VkGroupPosts.Where(p => p.TransferDate == null).OrderByDescending(p => p.PostDate).FirstOrDefault();
            if (post != null)
            {
                var attachments = _context.VkPostImages.Where(i => i.PostId == post.VkId).ToList();
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
