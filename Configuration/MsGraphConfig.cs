using System.ComponentModel;

namespace ParanoidOneDriveBackup
{
    [Description("MsGraph")]
    class MsGraphConfig
    {
        public string ClientId { get; set; }
        public string[] Scopes { get; set; }
    }
}
