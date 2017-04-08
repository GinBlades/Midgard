using ServiceStack.DataAnnotations;
using System;

namespace Midgard.UtilitiesN4.Models
{
    public class UserRole
    {
        [AutoIncrement]
        public int Id { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; }

        [ForeignKey(typeof(User), OnDelete = "CASCADE")]
        public int UserId { get; set; }
        [Reference]
        public User User { get; set; }
    }

    public enum Role
    {
        Guest, Member, Moderator, Administrator
    }
}
