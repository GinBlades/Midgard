# Midgard

Simple authentication using OrmLite.

Sample `Startup` configuration:

	// ConfigureServices
	services.AddSingleton<IDbConnectionFactory>(_ =>
	{
		return new OrmLiteConnectionFactory(
			_config.GetConnectionString("DefaultConnection"), SqlServerDialect.Provider
		);
	});

	services.AddSingleton<Migrations>();

	services.AddDistributedMemoryCache();
	services.AddSession(options => {
		options.IdleTimeout = TimeSpan.FromSeconds(60);
		options.CookieHttpOnly = true;
	});

	services.AddScoped<AccountHelper>();

	// Configure
	app.UseCookieAuthentication(new CookieAuthenticationOptions() {
		AuthenticationScheme = AuthOptions.Middleware,
		LoginPath = new PathString("/Account/Login"),
		AccessDeniedPath = new PathString("/Account/Forbidden"),
		AutomaticAuthenticate = true,
		AutomaticChallenge = true
	});

	app.UseSession();

	// Example of OrmLite filters
	OrmLiteConfig.InsertFilter = (dbCmd, row) =>
	{
		var auditRow = row as IDbModelWithTimestamp;
		if (auditRow != null)
		{
			auditRow.CreatedAt = auditRow.UpdatedAt = DateTime.Now;
		}
	};

	migrations.Up();

Sample `AccountController`:

    public class AccountController : Controller
    {
        private readonly IDbConnectionFactory _conn;
        private readonly AccountHelper _helper;

        public AccountController(IDbConnectionFactory conn, AccountHelper helper)
        {
            _conn = conn;
            _helper = helper;
        }

        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginFormObject { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginFormObject lfo)
        {
            if (ModelState.IsValid)
            {
                User user = await _helper.GetUser(lfo, ModelState);
                if (user != null)
                {
                    await _helper.LocalLogin(user, HttpContext);
                    return RedirectToLocal(lfo.ReturnUrl);
                } else
                {
                    TempData["Alert"] = "Invalid username/password";
                }
            }
            return View(lfo);
        }

        public IActionResult Register(string returnUrl = null)
        {
            return View(new RegisterFormObject { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterFormObject rfo)
        {
            if (ModelState.IsValid && await _helper.CheckUser(rfo, ModelState))
            {
                var user = await _helper.CreateUser(rfo);
                await _helper.LocalLogin(user, HttpContext);
                return RedirectToLocal(rfo.ReturnUrl);
            }
            return View(rfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync(AuthOptions.Middleware);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            } else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }

Some view samples:

	// Account/Login.cshtml
	@model LoginFormObject

	<form asp-action="Login">
		<div asp-validation-summary="ModelOnly"></div>
		<input type="hidden" asp-for="ReturnUrl" />
		<div class="form-group">
			<label asp-for="UsernameOrEmail"></label>
			<input asp-for="UsernameOrEmail" />
			<span asp-validation-for="UsernameOrEmail"></span>
		</div>
		<div class="form-group">
			<label asp-for="Password"></label>
			<input asp-for="Password" />
			<span asp-validation-for="Password"></span>
		</div>
		<div class="form-group">
			<label asp-for="RememberMe">Remember Me</label>
			<input asp-for="RememberMe" />
		</div>
		<div class="form-group">
			<input type="submit" value="Log In" />
		</div>
	</form>

	// Account/Register.cshtml
	@model RegisterFormObject

	<form asp-action="Register">
		<div asp-validation-summary="ModelOnly"></div>
		<input type="hidden" asp-for="ReturnUrl" />
		<div class="form-group">
			<label asp-for="UserName"></label>
			<input asp-for="UserName" />
			<span asp-validation-for="UserName"></span>
		</div>
		<div class="form-group">
			<label asp-for="Email"></label>
			<input asp-for="Email" />
			<span asp-validation-for="Email"></span>
		</div>
		<div class="form-group">
			<label asp-for="Password"></label>
			<input asp-for="Password" />
			<span asp-validation-for="Password"></span>
		</div>
		<div class="form-group">
			<label asp-for="PasswordConfirmation"></label>
			<input asp-for="PasswordConfirmation" />
			<span asp-validation-for="PasswordConfirmation"></span>
		</div>
		<div class="form-group">
			<input type="submit" value="Register" />
		</div>
	</form>

	// Menu
	@if (Context.User.Identity.IsAuthenticated)
	{
		<li>
			<form asp-action="Logout" asp-controller="Account">
				<input type="submit" value="Log Out" />
			</form>
		</li>
	} else
	{
		<li>
			<a asp-action="Login" asp-controller="Account">Log In</a>
			<a asp-action="Register" asp-controller="Account">Register</a>
		</li>
	}

## TODOs:

* Wrap Startup configuration
* Add 3rd party authentication
