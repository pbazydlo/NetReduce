namespace NetReduce
{
    using System.Collections.Generic;

    public interface IMapProvider
    {
        IEnumerable<KeyValuePair<string, string>> Map(string key, string value);
    }
}
