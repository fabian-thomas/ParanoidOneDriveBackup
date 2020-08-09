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
    }
}
