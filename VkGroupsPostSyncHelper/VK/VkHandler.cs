using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace VkGroupsPostSyncHelper.VK
{
    public class VkHandler
    {
        private IConfiguration _config;
        private ILogger<VkHandler> _logger;
        public VkHandler(IConfiguration config, ILogger<VkHandler> logger)
        {
            _config = config;
            _logger = logger;
        }

        private String ApplicationKey
        { 
            get 
            {
                return _config.GetSection("VK")[nameof(ApplicationKey)];
            } 
        }

        private long GroupID
        {
            get
            {
                long result;
                if (long.TryParse(_config.GetSection("VK")[nameof(GroupID)], out result))
                {
                    return result;
                }
                return 0;
            }
        }

        public async Task<IList<Post>> GetPosts(ulong offset)
        {
            try
            {
                var vkApi = new VkApi();
                _logger.LogDebug($"Try Authorize in VK");
                vkApi.Authorize(new ApiAuthParams
                { 
                    AccessToken = this.ApplicationKey, 
                    Settings = Settings.All 
                });
                _logger.LogDebug($"Try Get Posts from VK with offset {offset}");
                var result = await vkApi.Wall.GetAsync(new WallGetParams
                {
                    // для сообществ id должно начинаться с -
                    OwnerId = this.GroupID,
                    Count = 10,
                    Offset = offset
                });
                var posts = result.WallPosts;
                _logger.LogDebug($"Result: {posts?.Count ?? 0} posts");
                return posts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in loading posts from vk. AppKey: {this.ApplicationKey} Group: {this.GroupID} Offset: {offset}");
                return null;
            }
        }
    }
}
