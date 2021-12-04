using System;
using System.ComponentModel.DataAnnotations;

namespace VkGroupsPostSyncHelper.SQLite
{
    public class VkGroupPost
    {
        [Key]
        public int Id { get; set; }
        public long? VkId { get; set; }
        public String Text { get; set; }
        public DateTime? PostDate { get; set; }
        public DateTime? TransferDate { get; set; } = null;
    }
}
