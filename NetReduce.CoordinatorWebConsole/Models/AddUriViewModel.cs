using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetReduce.CoordinatorWebConsole.Models
{
    public class AddUriViewModel
    {
        public ICollection<string> Uris { get; set; }
        public ICollection<string> Errors { get; set; }
    }
}