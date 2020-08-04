using System;
using System.Collections.Generic;
using System.Text;

namespace ParanoidOneDriveBackup
{
    public static class Helper
    {
        public static string[] ScopesStringToArray(string scopesString)
        {
            return scopesString.Split(";");
        }
    }
}
