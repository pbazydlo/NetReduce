using NetReduce.CoordinatorWebConsole.Models;
using NetReduce.Remote;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;

namespace NetReduce.CoordinatorWebConsole.Controllers
{
    [Authorize]
    // [RequireHttps]
    public class CoordinatorController : Controller
    {
        private CSClient.CoordinatorServiceClient coordinator = new CSClient.CoordinatorServiceClient();
        private static object UriProviderLock = new object();
        public static UriProvider UriProvider = new UriProvider();

        //
        // GET: /Coordinator/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PerformanceStatistics(Uri uri)
        {
            if(uri==null)
            {
                return this.Json("incorrect uri");
            }

            var binding = new BasicHttpBinding();
            using(var client = new WSClient.RemoteWorkerServiceClient(binding, new EndpointAddress(uri)))
            {
                var statistics = client.GetPerformanceStatistics();
                var driveStatistics = statistics.DriveStatistics.First();
                
                return this.Json(new 
                {
                    CPU = statistics.LoadStatistics.TotalProcessorTimeCounterPercent.ToString("f2"),
                    Memory = statistics.LoadStatistics.UsedRamCounterPercent.ToString("f2"),
                    Disk = (Convert.ToDouble(driveStatistics.FreeSpace) / Convert.ToDouble(driveStatistics.TotalSize)).ToString("f2")
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Results()
        {
            var results = this.coordinator.GetResults();
            return this.Json(new
                {
                    Status = results.IsRunning ? "Job running" : "Idle",
                    Results = results.KeysAndValues.Select(kv => new { Key = kv.Item1, Value = kv.Item2 }).ToArray()
                }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Uris()
        {
            return this.Json(this.coordinator.GetWorkers(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ActionName("Task")]
        public ActionResult SubmitTask(List<Uri> filesToProcess, Uri mapCode, Uri reduceCode, int noOfMappers, int noOfReducers)
        {
            this.coordinator.RunJob(noOfMappers, noOfReducers, mapCode, reduceCode, filesToProcess.ToArray());
            return Json("");
        }

        [HttpPost]
        [ActionName("Files")]
        public ActionResult SubmitFile(IEnumerable<HttpPostedFileBase> files)
        {
            if(files!=null)
            {
                foreach (var file in files)
                {
                    if(file!=null)
                    {
                        using(var reader = new StreamReader(file.InputStream))
                        {
                            var content = reader.ReadToEnd();
                            this.coordinator.AddToStorage(file.FileName, content);
                        }
                    }
                }
            }

            return this.Json("");
        }

        [HttpPost]
        public ActionResult RemoveFile(Uri uri)
        {
            if (uri != null)
            {
                this.coordinator.RemoveFromStorage(uri);
                return this.Json("ok");
            }

            return this.Json("Bad URI");
        }

        public ActionResult Files()
        {
            return this.Json(this.coordinator.ListStorageFiles(), JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}