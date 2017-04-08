using Midgard.UtilitiesN4.Data;
using Midgard.UtilitiesN4.Services.Filters;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Midgard.SampleN4.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var connection = new OrmLiteConnectionFactory(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString,
                SqlServerDialect.Provider);
            var migrations = new Migrations(connection);
            // migrations.Up();
            return View();
        }

        [AuthorizationFilter("Member")]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        
        [AuthorizationFilter("Admin")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}