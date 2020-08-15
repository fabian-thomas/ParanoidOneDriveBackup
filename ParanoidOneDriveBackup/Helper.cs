using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        public static FileStream MakeUnique(string path, string optionalSuffix = "") // taken from https://stackoverflow.com/questions/1078003/c-how-would-you-make-a-unique-filename-by-adding-a-number
        {
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExt = Path.GetExtension(path);

            if (!File.Exists(path))
                return File.Create(path);
            else
            {
                var suffixPath = Path.Combine(dir, $"{fileName}{optionalSuffix}{fileExt}");
                if (!File.Exists(suffixPath))
                    return File.Create(suffixPath);
            }

            for (int i = 1; ; i++)
            {
                path = Path.Combine(dir, $"{fileName}_{i}{fileExt}");
                if (!File.Exists(path))
                    return File.Create(path);
            }
        }
    }
}
