using Midgard.UtilitiesN4.Models.FormObjects;
using Midgard.UtilitiesN4.Services;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Midgard.SampleN4.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IDbConnectionFactory _connection;

        public AccountController()
        {
            _connection = new OrmLiteConnectionFactory(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString,
                SqlServerDialect.Provider);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginFormObject lfo)
        {
            if (!ModelState.IsValid)
            {
                return View(lfo);
            }
            var helper = new AccountHelper(_connection);
            var user = await helper.GetUser(lfo, ModelState);
            if (user != null)
            {
                helper.LocalLogin(user, HttpContext.ApplicationInstance.Context);
                return RedirectToAction("Index", "Home");
            }
            return View(lfo);
        }
    }
}
