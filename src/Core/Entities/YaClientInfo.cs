using System;

namespace YA.UserWorker.Core.Entities
{
    public class YaClientInfo : IRowVersionedEntity, IAuditedEntityBase
    {
        public Guid YaClientInfoID { get; set; }
        public string ClientVersion { get; set; }
        public string IpAddress { get; set; }
        public string CountryName { get; set; }
        public string RegionName { get; set; }
        public string Os { get; set; }
        public string OsVersion { get; set; }
        public string DeviceModel { get; set; }
        public string Browser { get; set; }
        public string BrowserVersion { get; set; }
        public string ScreenResolution { get; set; }
        public string ViewportSize { get; set; }
        public long Timestamp { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public byte[] tstamp { get; set; }
    }
}
