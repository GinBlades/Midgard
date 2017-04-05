using Midgard.Utilities.Models;
using Midgard.Utilities.Models.FormObjects;
using Midgard.Utilities.Models.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Midgard.Utilities.Services
{
    /// <summary>
    /// Collection of asynchronous helper methods to log in users from an Account management controller.
    /// </summary>
    public class AccountHelper
    {
        private readonly IDbConnectionFactory _conn;

        public AccountHelper(IDbConnectionFactory conn)
        {
            _conn = conn;
        }

        /// <summary>
        /// Checks to see if the username is valid and available in the database
        /// </summary>
        /// <param name="rfo">Form object passed in from the registration form</param>
        /// <param name="modelState">Current ModelState of the controller</param>
        /// <returns>false if there are any errors, true if user is valid and available</returns>
        public async Task<bool> CheckUser(RegisterFormObject rfo, ModelStateDictionary modelState)
        {
            if (!rfo.UserName.All(c => char.IsLetterOrDigit(c)))
            {
                modelState.AddModelError("UserName", "UserName can only contain letters and numbers");
                return false;
            }
            using (var db = _conn.Open())
            {
                if (await db.ExistsAsync<User>(u => u.Email == rfo.Email))
                {
                    modelState.AddModelError("Email", "Email already registered");
                    return false;
                }
                if (await db.ExistsAsync<User>(u => u.UserName == rfo.UserName))
                {
                    modelState.AddModelError("UserName", "UserName already taken");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates a new user from the registration form. Hashes the password and inserts into the database.
        /// </summary>
        /// <param name="rfo">Form object from the registration form</param>
        /// <returns>Returns created user</returns>
        public async Task<User> CreateUser(RegisterFormObject rfo)
        {
            rfo.Password = IdentityBasedHasher.HashPassword(rfo.Password).ToHashString();
            var user = rfo.ToUser();
            using (var db = _conn.Open())
            {
                var userId = await db.InsertAsync(user);
                user.Id = (int)userId;
            }
            return user;
        }

        /// <summary>
        /// Builds an identity for the user logging in with a local username and password.
        /// Includes roles, name, and email in ClaimsIdentity
        /// </summary>
        /// <param name="user">Registered user from the database</param>
        /// <param name="httpContext">HttpContext from the controller</param>
        /// <returns>void</returns>
        public async Task LocalLogin(User user, HttpContext httpContext)
        {
            // Built ClaimsPrincipal based on
            // http://stackoverflow.com/questions/20254796/why-is-my-claimsidentity-isauthenticated-always-false-for-web-api-authorize-fil
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Role, user.RoleList()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var identity = new ClaimsIdentity(claims, AuthOptions.Identity);
            var principal = new ClaimsPrincipal(new[] { identity });
            await httpContext.Authentication.SignInAsync(AuthOptions.Middleware, principal);
        }

        /// <summary>
        /// Finds a user based on the login form object. If 'username or email' contains an '@' symbol, check email. Otherwise check username.
        /// Symbols are not allowed in the username. IdentityBasedHasher is used to verify.
        /// </summary>
        /// <param name="lfo">Form object from the login form</param>
        /// <param name="modelState">Current ModelState from the controller</param>
        /// <returns>User if email/username and password are verified.</returns>
        public async Task<User> GetUser(LoginFormObject lfo, ModelStateDictionary modelState)
        {
            User user;
            using (var db = _conn.Open())
            {
                if (lfo.UsernameOrEmail.Contains("@"))
                {
                    user = (await db.LoadSelectAsync<User>(u => u.Email == lfo.UsernameOrEmail)).FirstOrDefault();
                } else
                {
                    user = (await db.LoadSelectAsync<User>(u => u.UserName == lfo.UsernameOrEmail)).FirstOrDefault();
                }

                if (user == null)
                {
                    modelState.AddModelError("UsernameOrEmail", "User not found.");
                    return null;
                }

                var valid = IdentityBasedHasher.VerifyHashedPassword(user.Password, lfo.Password);

                if (!valid)
                {
                    modelState.AddModelError("Password", "Password is incorrect");
                    return null;
                }

                return user;
            }
        }
    }
}
