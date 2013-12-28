﻿namespace NetReduce.Core.Tests
{
    using System.Collections.Generic;

    internal class SampleReducer : IReduceProvider
    {
        public string Reduce(string key, IEnumerable<string> values)
        {
            try
            {
                int result = 0;
                foreach (var value in values)
                {
                    result += int.Parse(value);
                }

                return result.ToString();
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
