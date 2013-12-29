namespace NetReduce.Core
{
    using System;

    using CSScriptLibrary;

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

        public static void CleanAssemblyCache()
        {
            CSScript.Evaluator.Reset();
        }
    }
}
