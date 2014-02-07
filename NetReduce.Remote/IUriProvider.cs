using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetReduce.Remote
{
    public interface IUriProvider
    {
        Uri GetNextUri();
        List<Uri> Uris { get; }
    }
}
