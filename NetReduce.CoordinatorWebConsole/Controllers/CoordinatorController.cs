using NetReduce.CoordinatorWebConsole.Models;
using NetReduce.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetReduce.CoordinatorWebConsole.Controllers
{
    [Authorize]
    // [RequireHttps]
    public class CoordinatorController : Controller
    {
        private static object UriProviderLock = new object();
        public static UriProvider UriProvider = new UriProvider();

        //
        // GET: /Coordinator/
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Uris")]
        public ActionResult AddUri(string uriString)
        {
            var uri = new Uri(uriString);
            var errors = new List<string>();
            lock (UriProviderLock)
            {
                if (!UriProvider.Uris.Contains(uri))
                {
                    UriProvider.Uris.Add(uri);
                }
                else
                {
                    errors.Add("Specified uri already exists!");
                }
            }

            return this.Json(new AddUriViewModel()
                {
                    Uris = this.GetRegisteredUris(),
                    Errors = errors
                });
        }

        public ActionResult Uris()
        {
            return this.Json(this.GetRegisteredUris(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("Task")]
        public ActionResult SubmitTask(string mapCode, string reduceCode)
        {
            // take map code, reduce code, and submitted files and start job
            throw new NotImplementedException();
        }

        [HttpPost]
        [ActionName("Files")]
        public ActionResult SubmitFile(IEnumerable<HttpPostedFileBase> files)
        {
            return this.Json("");
        }

        public ActionResult Files()
        {
            return this.Json(new[] { new { Uri = "http://alamakota:18765/asdf" } }, JsonRequestBehavior.AllowGet);
        }

        private ICollection<string> GetRegisteredUris()
        {
            return UriProvider.Uris.Select(u => u.OriginalString).ToList();
        }
    }
}