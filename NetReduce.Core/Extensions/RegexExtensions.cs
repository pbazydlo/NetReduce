using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NetReduce.Core.Extensions
{
    internal static class RegexExtensions
    {
        public static string GuidRegexString
        {
            get
            {
                return "[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12}";
            }
        }
    }
}
