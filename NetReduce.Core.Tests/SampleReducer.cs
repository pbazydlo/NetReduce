namespace NetReduce.Core.Tests
{
    using System.Collections.Generic;

    internal class SampleReducer : IReduceProvider
    {
        public KeyValuePair<string, string> Reduce(string key, IEnumerable<string> values)
        {
            try
            {
                int result = 0;
                foreach (var value in values)
                {
                    result += int.Parse(value);
                }

                return new KeyValuePair<string, string>(key, result.ToString());
            }
            catch (System.Exception ex)
            {
                return new KeyValuePair<string, string>(string.Format("{0}_error", key), ex.Message);
            }
        }
    }
}
