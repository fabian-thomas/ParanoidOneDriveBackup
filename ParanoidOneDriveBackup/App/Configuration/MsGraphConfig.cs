using System.ComponentModel;

namespace ParanoidOneDriveBackup.App.Configuration
{
    [Description("MsGraph")]
    internal class MsGraphConfig
    {
        public string ClientId { get; set; }
        public string[] Scopes { get; set; }
    }
}
