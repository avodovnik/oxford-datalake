using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageTagging
{
    public class Helper
    {
        public static bool HasTag(string tag, string column)
        {
            column = column.ToLower();
            tag = tag.ToLower();

            return column.Contains(tag);
        }
    }
}
