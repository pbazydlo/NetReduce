namespace NetReduce.Remote
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    [ServiceContract]
    public interface IRemoteWorkerService
    {
        [OperationContract]
        void Init(int workerId);

        [OperationContract]
        void Map(Uri uri, Uri mapFuncUri);

        [OperationContract]
        void Reduce(Uri uri, Uri reduceFuncUri);

        [OperationContract]
        string[] TryJoin(int workerId, Uri callbackUri);

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
        [OperationContract]
        Uri[] TransferFiles(int workerId, Dictionary<string, Uri> keysAndUris);

        [OperationContract]
        Uri PushFile(int workerId, string fileName, string content);
    }
}
