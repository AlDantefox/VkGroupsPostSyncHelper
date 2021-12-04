using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public VkHandler(IConfiguration config)
        {
            _config = config;
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
                return long.Parse(_config.GetSection("VK")[nameof(GroupID)]);
            }
        }

        public async Task<IList<Post>> GetPosts(ulong offset)
        {
            try
            {
                var vkApi = new VkApi();
                vkApi.Authorize(new ApiAuthParams
                { 
                    AccessToken = this.ApplicationKey, 
                    Settings = Settings.All 
                });

                var result = await vkApi.Wall.GetAsync(new WallGetParams
                {
                    // для сообществ id должно начинаться с -
                    OwnerId = this.GroupID,
                    Count = 100,
                    Offset = offset
                });
                var posts = result.WallPosts;
                return posts;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
