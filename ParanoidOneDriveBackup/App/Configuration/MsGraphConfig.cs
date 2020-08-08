using System.ComponentModel;

namespace ParanoidOneDriveBackup.App.Configuration
{
    [Description("MsGraph")]
    class MsGraphConfig
    {
        public string ClientId { get; set; }
        public string[] Scopes { get; set; }
    }
}
