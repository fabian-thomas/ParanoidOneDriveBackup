using System.ComponentModel;

namespace ParanoidOneDriveBackup.App.Configuration
{
    [Description("Backup")]
    class BackupConfig
    {
        public string Path { get; set; }
        public int RemainMaximum { get; set; }
    }
}
