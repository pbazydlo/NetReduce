namespace NetReduce.Remote
{
    using System;
    using System.Collections.Generic;

    public class UriProvider : IUriProvider
    {
        private List<Uri> uris = new List<Uri>();

        public List<Uri> Uris
        {
            get
            {
                return uris;
            }
        }

        private int counter = 0;

        private object urisLock = new object();

        public Uri GetNextUri()
        {
            lock (urisLock)
            {
                return uris[(counter++) % uris.Count];
            }
        }
    }
}
