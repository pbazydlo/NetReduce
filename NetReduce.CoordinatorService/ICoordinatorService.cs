﻿namespace NetReduce.CoordinatorService
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel;

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ICoordinatorService" in both code and config file together.
    [ServiceContract]
    public interface ICoordinatorService
    {
        [OperationContract]
        void AddWorker(Uri uri);

        [OperationContract]
        Uri[] GetWorkers();

        [OperationContract]
        void RemoveWorker(Uri uri);

        [OperationContract]
        bool RunJob(int numberOfMappers, int numberOfReducers, Uri mapCodeFile, Uri reduceCodeFile, Uri[] filesToProcess);

        [OperationContract]
        Uri AddToStorage(string fileName, string content);

        [OperationContract]
        Uri[] ListStorageFiles();

        [OperationContract]
        void RemoveFromStorage(Uri uri);

        [OperationContract]
        void CleanStorage();

        [OperationContract]
        MapReduceResult GetResults();
    }

    [DataContract]
    public class MapReduceResult
    {
        [DataMember]
        public Tuple<string, string>[] KeysAndValues { get; set; }

        [DataMember]
        public bool IsRunning { get; set; }
    }
}
