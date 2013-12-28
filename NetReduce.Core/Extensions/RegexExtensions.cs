namespace NetReduce.Core.Extensions
{
    public static class RegexExtensions
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
