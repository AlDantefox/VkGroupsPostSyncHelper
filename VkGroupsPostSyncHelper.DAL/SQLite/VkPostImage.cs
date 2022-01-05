using System;
using System.ComponentModel.DataAnnotations;

namespace VkGroupsPostSyncHelper.DAL.SQLite
{
    public class VkPostImage
    {
        [Key]
        public int Id { get; set; }
        public long? VkId { get; set; }
        public ulong Height { get; set; }
        public ulong Width { get; set; }
        public long PostId { get; set; }
        public String Url { get; set; }
        public byte[] Data { get; set; }
    }
}
