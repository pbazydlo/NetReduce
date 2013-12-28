namespace NetReduce.Core
{
    using System.IO;

    using CSScriptLibrary;

    public class Loader
    {
        public static T Load<T>(string fileName, IStorage storage) 
            where T : class
        {
                return CSScript.Evaluator.LoadCode<T>(storage.Read(fileName));
        }
    }
}
