using VkGroupsPostSyncHelper.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkGroupsPostSyncHelper.Models
{
    public record TelegramPost
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public IList<string> UrlImageList { get; set; }
        public bool IsOnlyText 
        {
            get 
            { 
                return this.UrlImageList == null || this.UrlImageList.Count == 0; 
            } 
        }

        public TelegramPost(VkGroupPost post)
        {
            Id = post.Id;
            Text = post.Text;
        }

        public TelegramPost(VkGroupPost post, IEnumerable<VkPostImage> images)
            :this(post)
        {
            this.UrlImageList = images.Select(i => i.Url).ToList();
        }
    }
}
