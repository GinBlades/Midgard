using Midgard.Utilities.Models;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using Midgard.Utilities.Services;

namespace Midgard.Utilities.Data
{
    public class Migrations
    {
        private readonly IDbConnectionFactory _conn;

        public Migrations(IDbConnectionFactory conn)
        {
            _conn = conn;
        }

        public void Up()
        {
            using (var db = _conn.Open())
            {
                db.CreateTableIfNotExists<User>();
                db.CreateTableIfNotExists<UserClaim>();
                db.CreateTableIfNotExists<UserRole>();

                db.ExecuteSql(@"
                    CREATE UNIQUE INDEX IX_UserClaim_Composite ON UserClaim (UserId, Provider)
                    CREATE UNIQUE INDEX IX_UserRole_Composite ON UserRole (UserId, Role)
                ");

                db.CreateIndex<User>(u => u.Email, unique: true);
                db.CreateIndex<User>(u => u.UserName, unique: true);

                SampleUser(db);
            }
        }

        public void Down()
        {
            using (var db = _conn.Open())
            {
                db.DropTable<UserClaim>();
                db.DropTable<UserRole>();
                db.DropTable<User>();
            }
        }

        private void SampleUser(IDbConnection db)
        {
            db.Save(new User
            {
                Email = "test@example.com",
                UserName = "test",
                Password = IdentityBasedHasher.HashPassword("secret").ToHashString(),
                UserClaims = new List<UserClaim>
                    {
                        new UserClaim
                        {
                            Provider = Provider.Local,
                            Claims = "test",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        }
                    },
                UserRoles = new List<UserRole>
                    {
                        new UserRole
                        {
                            Role = Role.Member,
                            CreatedAt = DateTime.Now
                        }
                    },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }, references: true);

            var user = db.LoadSelect<User>(u => u.UserName == "test");
        }
    }
}
