using System.ComponentModel;

namespace ParanoidOneDriveBackup.App.Configuration
{
    abstract class MsGraph
    {
        public string ClientId { get; set; }
        public string[] Scopes { get; set; }
    }
}
