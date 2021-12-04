using Microsoft.Extensions.Configuration;
using VkGroupsPostSyncHelper.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model.Attachments;

namespace VkGroupsPostSyncHelper.Logic
{
    public class VKprocessService
    {
        MainDbContext _context;
        private IConfiguration _config;
        public VKprocessService(MainDbContext context, IConfiguration config)
        {
            _config = config;
            _context = context;
        }

        private bool LoadImageData 
        {
            get     
            {
                return Boolean.Parse(_config.GetSection("VK")[nameof(LoadImageData)]);
            } 
        }

        /// <summary>
        /// Add new posts to DB
        /// </summary>
        /// <returns>Count of created</returns>
        public async Task<int> SaveIfNotExists(IList<Post> posts)
        {
            var added = 0;
            foreach (var post in posts)
            {
                var existing = _context.VkGroupPosts.FirstOrDefault(p => p.VkId == post.Id);
                if (existing == null && post.Id.HasValue)
                {
                    var addedPost = await _context.VkGroupPosts.AddAsync(new VkGroupPost() { 
                        Text = post.Text, 
                        VkId = post.Id,
                        PostDate = post.Date
                    });
                    added++;

                    if (post.Attachments != null && post.Attachments.Count > 0)
                    {
                        foreach (var attachment in post.Attachments)
                        {
                            if (attachment.Type == typeof(VkNet.Model.Attachments.Photo))
                            {
                                var vkPhoto = post.Attachments[0].Instance as VkNet.Model.Attachments.Photo;
                                var biggestSize = vkPhoto.Sizes.Last();
                                if (biggestSize != null)
                                {
                                    var url = biggestSize.Url.AbsoluteUri;
                                    var img = new VkPostImage()
                                    {
                                        VkId = vkPhoto.Id,
                                        Url = url,
                                        Width = biggestSize.Width,
                                        Height = biggestSize.Height,
                                        PostId = post.Id.Value,
                                    };
                                    if (this.LoadImageData)
                                    {
                                        using var wc = new WebClient();
                                        byte[] imageBytes = wc.DownloadData(url);
                                        if (imageBytes != null)
                                        {
                                            img.Data = imageBytes;
                                        }
                                    }

                                    _context.VkPostImages.Add(img);
                                }
                            }
                            else
                            {
                                //TODO Check other attachments
                            }
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
            return added;
        }

        public async Task MarkPostSended(int id, DateTime postDate)
        {
            var post = await _context.VkGroupPosts.FindAsync(id);
            post.TransferDate = postDate;
            await _context.SaveChangesAsync(); 
        }

        public void PrintAllPosts()
        {
            var posts = _context.VkGroupPosts.ToList();
            foreach (var post in posts)
            {
                Console.WriteLine($"Post #{post.Id} from {post.PostDate.Value} '{post.Text}'");
            }
        }
    }
}
