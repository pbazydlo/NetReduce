namespace NetReduce.Core
{
    using System.IO;

    using CSScriptLibrary;

    public class Loader
    {
        public static T Load<T>(string filePath) 
            where T : class
        {
            using (var sr = new StreamReader(filePath))
            {
                return CSScript.Evaluator.LoadCode<T>(sr.ReadToEnd());
            }
        }
    }
}
