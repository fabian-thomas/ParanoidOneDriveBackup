using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ParanoidOneDriveBackup
{
    static class Helper
    {
        public static string GetDescription(Type type)
        {
            var descriptions = (DescriptionAttribute[])type.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (descriptions.Length == 0)
                return null;
            else
                return descriptions[0].Description;
        }

        public static string GetApplicationDataFolderPath()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (path == "")
                // home/<user>/.config
                path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/.config";
            return path;
        }
    }
}
