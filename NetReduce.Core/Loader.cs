namespace NetReduce.Core
{
    using System.IO;

    using CSScriptLibrary;
using System;

    public class Loader
    {
        public static T Load<T>(string fileName, IStorage storage) 
            where T : class
        {
            bool success = false;
            while (!success)
            {
                try
                {
                    return CSScript.Evaluator.LoadCode<T>(storage.Read(fileName));
                }
                catch (MissingMethodException)
                {
                    // ignore
                }
            }

            return null;
        }
    }
}
