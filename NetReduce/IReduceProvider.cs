namespace NetReduce
{
    using System.Collections.Generic;

    public interface IReduceProvider
    {
        string Reduce(string key, IEnumerable<string> values);
    }
}
