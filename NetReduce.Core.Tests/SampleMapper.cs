namespace NetReduce.Core.Tests
{
    using System.Collections.Generic;

    internal class SampleMapper : IMapProvider
    {
        public IEnumerable<KeyValuePair<string, string>> Map(string key, string value)
        {
            var result = new List<KeyValuePair<string, string>>();
            foreach (var w in value.Split(' '))
                result.Add(new KeyValuePair<string, string>(w, "1"));

            return result;
        }
    }
}
