using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VkGroupsPostSyncHelper.SQLite;
using VkNet.Model.Attachments;

namespace VkGroupsPostSyncHelper.Logic
{
    public class VKprocessService
    {
        MainDbContext _context;
        private IConfiguration _config;
        private ILogger<VKprocessService> _logger;
        public VKprocessService(MainDbContext context, IConfiguration config, ILogger<VKprocessService> logger)
        {
            _config = config;
            _context = context;
            _logger = logger;
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
                    var addedPost = new VkGroupPost()
                    {
                        Text = post.Text,
                        VkId = post.Id,
                        PostDate = post.Date
                    };
                    await _context.VkGroupPosts.AddAsync(addedPost);
                    _logger.LogInformation($"Try add new post with id:{addedPost.VkId} and date:{addedPost.PostDate}");
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
                                        byte[] imageBytes = await wc.DownloadDataTaskAsync(url);
                                        if (imageBytes != null)
                                        {
                                            img.Data = imageBytes;
                                        }
                                    }

                                    await _context.VkPostImages.AddAsync(img);
                                    _logger.LogInformation($"Try add new post image with id:{img.VkId} and url:{img.Url}");
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
            _logger.LogDebug($"Mark post with id:{id} and vkId:{post.VkId} sended");
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
