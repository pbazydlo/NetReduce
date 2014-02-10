namespace NetReduce
{
    using System.Collections.Generic;

    public interface IReduceProvider
    {
        KeyValuePair<string, string> Reduce(string key, IEnumerable<string> values);
    }
}
