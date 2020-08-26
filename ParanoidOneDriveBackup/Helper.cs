using System;
using System.ComponentModel;
using System.Reflection;

namespace ParanoidOneDriveBackup
{
    internal static class Helper
    {
        public static string GetDescription(Type type)
        {
            var descriptions = (DescriptionAttribute[])type.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return descriptions.Length == 0 ? null : descriptions[0].Description;
        }

        public static Version GetAppVersion() 
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
    }
}
