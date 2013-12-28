namespace NetReduce.Core.Tests
{
    using System.Collections.Generic;

    internal class SampleMapper : IMapProvider
    {
        public IEnumerable<KeyValuePair<string, string>> Map(string key, string value)
        {
            var result = new System.Collections.Generic.SortedList<string, string>();
            foreach (var w in value.Split(' '))
                result.Add(w, "1");

            return result;
        }
    }
}
