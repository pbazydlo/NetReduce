using System.Web;
using System.Web.Mvc;

namespace NetReduce.CoordinatorWebConsole
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
