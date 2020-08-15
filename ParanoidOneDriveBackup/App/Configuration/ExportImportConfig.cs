using System.ComponentModel;

namespace ParanoidOneDriveBackup.App.Configuration
{
    [Description("ExportImport")]
    class ExportImportConfig : MsGraph
    {
        public string ZipApiUrl { get; set; }
    }
}
