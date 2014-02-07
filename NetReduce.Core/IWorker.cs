namespace NetReduce.Core
{
    using System;
    using System.Collections.Generic;

    public interface IWorker
    {
        void Init();
        void Map(Uri inputFileName, Uri mapCodeFileName);
        void Reduce(string key, Uri reduceCodeFileName);
        IEnumerable<string> Join();
        IStorage Storage { get; set; }
        int Id { get; set; }

        /// <summary>
        /// The transfer files.
        /// </summary>
        /// <param name="workerId">
        /// Worker id of worker that will send its results to reducer.
        /// </param>
        /// <param name="keysAndUris">
        /// Mapping between keys and reducers.
        /// Uri = endpoint + worker id
        /// </param>
        /// <returns>
        /// The <see cref="Uri[]"/>.
        /// </returns>
        Uri[] TransferFiles(int workerId, Dictionary<string, Uri> keysAndUris);

        Uri EndpointUri { get; }
    }
}
