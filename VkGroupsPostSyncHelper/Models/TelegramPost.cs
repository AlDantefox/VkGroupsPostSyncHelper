using System;
using System.Collections.Generic;
using System.Linq;
using VkGroupsPostSyncHelper.DAL.SQLite;

namespace VkGroupsPostSyncHelper.Models
{
    public record TelegramPost
    {
        public int Id { get; set; }
        public String Text { get; set; }
        public IList<string> UrlImageList { get; set; }
        public DateTime? PostDate { get; set; }
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
            PostDate = post.PostDate;
        }

        public TelegramPost(VkGroupPost post, IEnumerable<VkPostImage> images)
            :this(post)
        {
            this.UrlImageList = images.Select(i => i.Url).ToList();
        }
    }
}
