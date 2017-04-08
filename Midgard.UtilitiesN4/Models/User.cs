using Midgard.UtilitiesN4.Models.Options;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Midgard.UtilitiesN4.Models
{
    public class User : IDbModelWithTimestamp
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Reference]
        public virtual List<UserClaim> UserClaims { get; set; }
        [Reference]
        public virtual List<UserRole> UserRoles { get; set; }

        public string RoleList()
        {
            if (UserRoles == null || UserRoles.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(AuthOptions.Separator.ToString(),
                UserRoles.Select(ur => Enum.GetName(typeof(Role), ur.Role)));
        }
    }
}
